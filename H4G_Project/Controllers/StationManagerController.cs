using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using H4G_Project.DAL;
using H4G_Project.Models;
using Newtonsoft.Json;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol;

namespace H4G_Project.Controllers
{
    public class StationManagerController : Controller
    {
        ParcelDAL parcelContext = new ParcelDAL();
        StaffDAL staffContext = new StaffDAL();
        MemberDAL memberContext = new MemberDAL();
        CashVoucherDAL cashContext = new CashVoucherDAL();
        DeliveryHistoryDAL deliveryHistoryContext = new DeliveryHistoryDAL();
		DeliveryFailureDAL deliveryFailureContext = new DeliveryFailureDAL();
        //FeedbackEnquiryDAL feedbackEnquiryContext = new FeedbackEnquiryDAL();
        Dictionary<int, string> deliverymanname = new Dictionary<int, string>();
        Dictionary<char, string> deliverystatus = new Dictionary<char, string>();

        // adding the deliverystatus and the delivery man name for UI
        public StationManagerController(){
            deliverystatus.Add('0',"Pending");
            deliverystatus.Add('1', "In Progress");
            deliverystatus.Add('2', "In Progress");
            deliverystatus.Add('3', "Completed");
            deliverystatus.Add('4', "Failed");
               
            foreach(var staff in staffContext.GetAllStaff())
            {
                deliverymanname.Add(staff.staffID, staff.staffName);
            }
        }
        // GET: StationManagerController
        // Home page
        public ActionResult Index()
        { 
            if (HttpContext.Session.GetString("Role") != null)
            {
                //delivery failure
                //parcels yet to assign
                //inbox

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        
        }
        // Get the available delivery man based on the parcel location 
        public static List<SelectListItem> getDeliveryMan(Parcel parcel) // untested
        {
            StaffDAL staffContext = new StaffDAL();
            ParcelDAL parcelContext = new ParcelDAL();
            List<Staff> staffList = staffContext.GetAllStaff();
            List<SelectListItem> deliveryManDropdown = new List<SelectListItem>();
            deliveryManDropdown.Add(new SelectListItem { Text = "--Choose an option--", Value = "" ,Selected = true});
            foreach(var staff in staffList)
            {
                if (staff.appointment == "Delivery Man" && parcelContext.GetLastKnowParcelLocation(parcel).Contains(staff.location) && staffContext.CheckStaffAvailability(staff))
                {
                    deliveryManDropdown.Add(new SelectListItem { Text = staff.staffName, Value = staff.staffID.ToString() });
                }
            }
            return deliveryManDropdown;
        }
         
        // call the DAL and get the parcelList and throw them into parcel yet to be collected,assigned, and the remaining
        public ActionResult Deliveries()
            {
            if (HttpContext.Session.GetString("Role") != null)
            {
                Staff staff = staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role"));
				List<Parcel> parcelList = parcelContext.GetAllParcels();
                List<Parcel> airportParcel = new List<Parcel>();
                List<Parcel> pendingParcel = new List<Parcel>();
                List<Parcel> remainingParcel = new List<Parcel>();
                foreach (var parcel in parcelList)
                {
                    if (parcel.deliveryStatus == '0' && staff.location == parcel.toCountry && parcelContext.GetLastKnowParcelLocation(parcel).Contains("airport"))
                    {

                        if (parcel.targetDeliveryDate != null)
                        {
                            if ((DateTime.Now - (DateTime)parcel.targetDeliveryDate).Days >= -2)
                            {
                                airportParcel.Add(parcel);

                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (parcel.deliveryStatus == '0' && parcel.deliveryManId == null && parcelContext.GetLastKnowParcelLocation(parcel).Contains(staff.location))
                    {
                        pendingParcel.Add(parcel);
                    }
                    else
                    {
                        remainingParcel.Add(parcel);
                    }
                }
                ViewData["airportParcel"] = airportParcel;
                ViewData["pendingParcel"] = pendingParcel;
                ViewData["remainingParcel"] = remainingParcel;
                ViewData["deliverystatus"] = deliverystatus;
                ViewData["deliveryman"] = deliverymanname;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        // get the yet to be assigned pending parcel data via AJAX req and check if staff is available
        // update the DB according to the parcel sending location
        [HttpPost]
        [Consumes("application/json")]

        //[ValidateAntiForgeryToken] DO NOT ADD THIS IT IS DISRUPTING THE JSON DATA SENT
        public ActionResult DeliveriesPending([FromBody] List<PendingParcelData> pendingParcelData)
        {
            List<int> errorParcels = new List<int>();
            foreach(var p in pendingParcelData)
            {
                if ( p.deliveryManId != null)
                {
                    Parcel parcel = parcelContext.GetParceById(p.id);
                    Staff staff = staffContext.FindStaffByStaffID((int)p.deliveryManId);
                    if (staffContext.CheckStaffAvailability(staff))
                    {
                        string status = "0";
                        if (parcelContext.GetLastKnowParcelLocation(parcel).Contains(parcel.toCountry))
                        {
                            status = "1";  
                        }
                        else
                        {
                            status = "2";
                        }
                        parcelContext.UpdateParcel(p.id, "DeliveryStatus", status); //here not working
                        parcelContext.UpdateParcel(p.id,"DeliveryManID",p.deliveryManId.ToString());
                    }
                    else
                    {
                        errorParcels.Add(p.id);
                    }
                }
            }
            return Json(new { success = true,errorParcels = errorParcels});
        }
        // get the yet to be assigned airport parcel data via AJAX req and check if staff is available (parcel will not show unless it is closer to target delivery date)
        // update the DB according to the parcel sending location
        [HttpPost]
        [Consumes("application/json")]

        //[ValidateAntiForgeryToken] DO NOT ADD THIS IT IS DISRUPTING THE JSON DATA SENT
        public ActionResult DeliveriesAirport([FromBody] List<AirportParcelData> airportParcelData)
        {
            foreach (var parcel in airportParcelData)
            {
                if (parcel.collected == "true")
                {
                    parcelContext.UpdateParcel(parcel.id, "DeliveryStatus", "0");
                    parcelContext.UpdateParcel(parcel.id, "DeliveryManID", "NULL");
                    DeliveryHistory dh = new DeliveryHistory();
                    dh.parcelID = parcel.id;
                    dh.description = $"Received parcel by {HttpContext.Session.GetString("Role")} on {DateTime.Now.ToString("dd MMM yyyy h:mm tt")}.";
                    dh.recordID = deliveryHistoryContext.AddRecord(dh);
                }
            }
            return Json(new { success = true });
        }
        // get the data based on the parcel id in the db and display the details in a model
        public ActionResult IndividualParcel(int id)
        {
            if (id != null && parcelContext.GetParceById(id) != null)
            {
                ViewData["deliverystatus"] = deliverystatus;
                Parcel parcel = parcelContext.GetParceById(id);
                return View(parcel);
            }
            else
            {
                return NotFound("Parcel does not exist");
            }
        }
        //get the staffs under the station manager (same location) 
        // check if the staff view assigned parcels has been clicked and show the parcels currently assigned to the delivery man
        public ActionResult StaffView(int? id)
        {
            if (HttpContext.Session.GetString("Role") != null)
            {
                List<DeliveryManViewModel> deliveryManViewModelList = new List<DeliveryManViewModel>();
                List<Staff> staffList = staffContext.GetAllStaff();
                List<Parcel> parcelList = parcelContext.GetAllParcels();
                Staff staff = staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role"));
                foreach(var s in staffList)
                {
                    if (s.location == staff.location && s.appointment == "Delivery Man")
                    {
                        DeliveryManViewModel vm = new DeliveryManViewModel();
                        vm.deliveryMan = s;
                        foreach (var p in parcelList)
                        {
                            if (p.deliveryManId == s.staffID && (p.deliveryStatus == '1' || p.deliveryStatus == '2'))
                            {
                                vm.parcelList.Add(p);
                            }
                        }
                        deliveryManViewModelList.Add(vm);

                    }
                }
                if (id != null)
                {
                    ViewData["id"] = id;
                }
                ViewData["deliverystatus"] = deliverystatus;
                return View(deliveryManViewModelList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        
        public ActionResult Inbox()
        {
            return View();
            /*
             * same thing as assigned delveries or like LW layout for simplicity?
             * get all the feedback under the staff or check if member asking is same country as stationamanger
             * allow a post request with small update
             * make the unanwered posts bold to show urgent
             */
        }

        public ActionResult FailureIssue()
        {
            // Get the list of all delivery failures from the database
            List<DeliveryFailure> deliveryFailureList = deliveryFailureContext.GetAllDeliveryFailure();
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Station Manager")
                {
                    return View(deliveryFailureList);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Confirmation(int id)
        {
            // Get the list of all member details from the database
            Member member = memberContext.GetDetails(id);
            
            

            return RedirectToAction("CashVoucherIssuing", new { id = member.memberID });
           


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CashVoucherIssuing(CashVoucher cashvoucher)
        {
            cashContext.Create(cashvoucher);
            return RedirectToAction("FailureIssue");


        }
        public ActionResult CashVoucherIssuing(int id)
        {
            // Get details of a member based on the provided 'id' from the database
            Member member = memberContext.GetDetails(id);
            CashVoucher cashVoucher = new CashVoucher();

            // Set properties of the 'cashVoucher' object with some values
            cashVoucher.staffID = 3;
            cashVoucher.Amount = 20;
            cashVoucher.currency = "SGD";
            cashVoucher.issueingCode = '2';
            cashVoucher.receiverName = member.name;
            cashVoucher.receiverTelNo = member.telNo;
            cashVoucher.dateTimeIssued = DateTime.Now;


            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Station Manager")
                {
                    return View(cashVoucher);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult EditFailureDesc(int id)
        {
            
            if (id == null)
            { 
                return RedirectToAction("Index");
            }
            // Get details of a delivery failure based on the provided 'id' from the database
            DeliveryFailure deliveryFail = deliveryFailureContext.GetDetails(id);

            if (deliveryFail == null)
            {
                //Return to listing page, not allowed to edit
                return RedirectToAction("Index");

            }
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Station Manager")
                {
                    return View(deliveryFail);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFailureDesc(DeliveryFailure deliveryFail)
        {
            
           
            if (ModelState.IsValid)
            {
                // Update Details of a delivery failure 
                deliveryFailureContext.Update(deliveryFail);
                return RedirectToAction("FailureIssue");
            }
            else
            {
                
                return View(deliveryFail);
            }
        }
        public ActionResult Account()
        {
            /*
             * Doing if i have time, get StaffByLoginID from HTtpsessionstring
             * Do wtv bs here to post and edit details
             */
            return View();
        }
        // remove the session stroage data and redirect to homepage
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        /*
         Failure report for SM - package 4
         Coupons and voucher issue - package 3
        */
    }
}