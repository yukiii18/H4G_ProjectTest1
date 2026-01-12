using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using H4G_Project.DAL;
using H4G_Project.Models;

namespace H4G_Project.Controllers
{
    public class DeliveryManController : Controller
    {
        private ParcelDAL parcelContext = new ParcelDAL();
        private StaffDAL staffContext = new StaffDAL();
        private DeliveryHistoryDAL deliveryHistoryContext = new DeliveryHistoryDAL();
        private DeliveryFailureDAL deliveryFailureContext = new DeliveryFailureDAL();
        Dictionary<char, string> deliverystatus = new Dictionary<char, string>();
        // styling details data for ViewData
        public DeliveryManController()
        {
            deliverystatus.Add('0', "Pending");
            deliverystatus.Add('1', "In Progress");
            deliverystatus.Add('2', "In Progress");
            deliverystatus.Add('3', "Completed");
            deliverystatus.Add('4', "Failed");
        }
        public IActionResult Index()
        {
            // if have time add notifications here
            return View();
        }
        // check the assigned parcels in the DB based on the staffid to assign the parcels
        public ActionResult Deliveries()
        {
            if (HttpContext.Session.GetString("Role") != null)
            {
                List<Parcel> pendingParcelList = new List<Parcel>();
                List<Parcel> remainingParcelList = new List<Parcel> ();
                List<SelectListItem> statusUpdate = new List<SelectListItem>();
                statusUpdate.Add(new SelectListItem { Text = "--Update Delivery--", Value = "",Selected = true}); 
                statusUpdate.Add(new SelectListItem { Text = "Completed", Value = "1" });
                statusUpdate.Add(new SelectListItem { Text = "Failed", Value = "2" });
                Staff staff = staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role"));
                foreach (var parcel in parcelContext.GetAllParcels())
                {
                    if ((parcel.deliveryStatus == '1' || parcel.deliveryStatus == '2') && parcel.deliveryManId == staff.staffID && !parcelContext.GetLastKnowParcelLocation(parcel).Contains("airport"))
                    {
                        pendingParcelList.Add(parcel);
                    }
                    else if (deliveryHistoryContext.CheckDeliveryActivity(staff, parcel.parcelId))
                    {
                        remainingParcelList.Add(parcel);
                    }
                }
                ViewData["remainingParcelList"] = remainingParcelList; //not used
                ViewData["StatusOptions"] = statusUpdate;
                return View(pendingParcelList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // update the delivery details of the parcel and update the DB accordingly
        [HttpPost]
        public ActionResult UpdateDeliveries(Dictionary<int,int> parcelStatus)
        {
            foreach(var item in parcelStatus)
            {
                int parcelID = item.Key;
                int status = item.Value;
                Parcel parcel = parcelContext.GetParceById(parcelID);
                
                if (status == 1) // if delivery successful check for delivery location and update the DB
                {
                    DeliveryHistory dh = new DeliveryHistory();
                    dh.parcelID = parcel.parcelId;
                    if (staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role")).location == parcel.toCountry)
                    {
                        dh.description = $"Parcel delivered successfully by {HttpContext.Session.GetString("Role")} on {DateTime.Now.ToString("d MMM yyyy h:mm tt")}";
                        parcelContext.UpdateParcel(parcel.parcelId, "DeliveryStatus", "3");

                    }
                    else
                    {
                        dh.description = $"Parcel delivered to airport by {HttpContext.Session.GetString("Role")} on {DateTime.Now.ToString("d MMM yyyy h:mm tt")}";
                        parcelContext.UpdateParcel(parcel.parcelId, "DeliveryStatus", "0");
                    }
                    dh.recordID = deliveryHistoryContext.AddRecord(dh);
                }
                else if (status == 2)
                {
                    parcelContext.UpdateParcel(parcelID, "DeliveryStatus", "4");
                }
                else
                {
                    continue;
                }
            }
            return RedirectToAction("Deliveries","DeliveryMan");
        }
        // get the failed parcels yet to have a delivery failure row in the db
        // get the delivery failure parcels and throw them into a viewdata to be displayed

        public ActionResult Reports()
        {
            if (HttpContext.Session.GetString("Role") != null)
            {
                List<DeliveryFailure> reports = new List<DeliveryFailure>();
                List<ReportVM> list = new List<ReportVM>();
                if (parcelContext.GetPendingFailures(staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role"))) != null)
                {
                    foreach (var parcel in parcelContext.GetPendingFailures(staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role"))))
                    {
                        DeliveryFailure reportVM = new DeliveryFailure();
                        reportVM.parcelID = parcel.parcelId;
                        reports.Add(reportVM);
                    }
                }
               
                foreach (var report in deliveryFailureContext.GetReports(staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role"))))
                {
                    DeliveryFailure reportVM = new DeliveryFailure();
                    reportVM.reportID = report.reportID;
                    reportVM.parcelID = report.parcelID;
                    reportVM.deliveryManID = report.deliveryManID;
                    reportVM.failureType = report.failureType;
                    reportVM.description = report.description;
                    reportVM.stationMgrID = report.stationMgrID;
                    reportVM.followUpAction = report.followUpAction;
                    reportVM.dateCreated = report.dateCreated;
                    reports.Add(reportVM);
                }
                ViewData["reports"] = reports;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }  
        }
        //Receive the Report details and check if the input fields are all there
        // then update the db
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public ActionResult PostReports(MainViewModel model)
        {
            foreach(var item in  model.Reports)
            {
               
                if (item.details != 0 && item.description != null)
                {
                    DeliveryFailure failure = new DeliveryFailure();
                    failure.parcelID = item.id;
                    failure.deliveryManID = staffContext.FindStaffByLoginID(HttpContext.Session.GetString("Role")).staffID;
                    failure.failureType = item.details.ToString()[0];
                    failure.description = item.description;
                    failure.dateCreated = DateTime.Now;
                    deliveryFailureContext.AddRecord(failure);
                }
                else
                {
                    continue;
                }
            }
            return RedirectToAction("Reports", "DeliveryMan");
        }
        // get the parcel details based on parcel id and display 

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
        public ActionResult Edit()
        {

            return View();
        }
        public ActionResult Account()
        {
            return View();
        }
        //log out by clearing session storage and going to the homepage
        public ActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
