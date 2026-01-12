using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using H4G_Project.DAL;
using H4G_Project.Models;

namespace H4G_Project.Controllers
{
    public class AdminManagerController : Controller
    {
        private ShippingRateDAL shipContext = new ShippingRateDAL();
        private CashVoucherDAL cashContext = new CashVoucherDAL();
        private MemberDAL memberContext = new MemberDAL();
        private StaffDAL staffContext = new StaffDAL();
        private readonly HttpClient _httpClient;
        private List<SelectListItem> countryList = new List<SelectListItem>();
        private List<SelectListItem> cityList = new List<SelectListItem>();
        private List<String> checkCountryExist = new List<String>();
        
        public AdminManagerController()
        {
            //Generates API to get the country and cities to add to the list
            List<CashVoucher> cashVouchers = cashContext.GetAllCashVoucher();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.freecurrencyapi.com");
            List<ShippingRate> shipList = shipContext.GetAllShipRates();
            foreach (ShippingRate s in shipList)
            {
                if (!checkCountryExist.Contains(s.toCountry))
                {
                    countryList.Add(
                    new SelectListItem
                    {
                        Value = s.toCountry.ToString(),
                        Text = s.toCountry.ToString(),
                    });
                    checkCountryExist.Add(s.toCountry);
                }
                cityList.Add(
                new SelectListItem
                {
                    Value = s.toCity.ToString(),
                    Text = s.toCity.ToString(),
                });
            }
        }
        //method below doesnt do anything...
        public IActionResult CreateShipping()
        {
            ViewData["countries"] = countryList;
            ViewData["cities"] = cityList;
            ViewData["ShowResult"] = false;
            return View();
        }
        public IActionResult Index()
        {
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                String loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Admin Manager")
                {
                    return View();
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ViewShippingRate()
        {
            // Retrieve the shipping rates to be displayed

            List<ShippingRate> shippingRateList = shipContext.GetAllShipRates();
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Admin Manager")
                {
                    return View(shippingRateList);
                }
            }
            return RedirectToAction("Index", "Home");

            
        }

        public ActionResult ShippingRateCreate()
        {
            // Create a new instance of ShippingRate
            var shippingRate = new ShippingRate();
            ViewData["countries"] = countryList;
            ViewData["cities"] = cityList;
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Admin Manager")
                {
                    return View();
                }
            }
            return RedirectToAction("Index", "Home");
            // Pass the shippingRate object as a collection to the view
            
        }
        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShippingRateCreate(ShippingRate ships)
        {
            
            ViewData["countries"] = countryList;
            ViewData["cities"] = cityList;

            
            if (ModelState.IsValid)
            {
                if (shipContext.Validation(ships))
                {
                    //error message for validation
                    ModelState.AddModelError(string.Empty,"Error! ShippingRate Already Exists!");
                    return View(ships);
                }
                
                ships.lastUpdatedBy = 4;
                // Set the created object to a newly added shipping rate's identifier
                ships.ShippingRateID = shipContext.Add(ships);
                
                
                return RedirectToAction("ViewShippingRate");
            }
            else
            {
                //Input validation fails, return to the Create view
                //to display error message
                return View(ships);
            }
        }
        public IActionResult GetCities(string country)
        {
            // Get the list of cities based on the selected country
            List<string> cities = shipContext.GetCitiesByCountry(country);

            return Json(cities);
        }


        // GET: AdminController/Edit/5
        public ActionResult EditShippingRate(int id)
        {
            
            if (id == null)
            { 
                return RedirectToAction("Index");
            }
            ViewData["countries"] = countryList;
            //get the specific details about shipping rates
            ShippingRate ship = shipContext.GetDetails(id);
   
            if (ship == null)
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
                if (staff.appointment == "Admin Manager")
                {
                    return View(ship);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditShippingRate(ShippingRate ship)
        {
            
            ViewData["countries"] = countryList;
            if (ModelState.IsValid)
            {
                //Update shipping rates to database
                shipContext.Update(ship);
                return RedirectToAction("ViewShippingRate");
            }
            else
            {
                //Input validation fails, return to the view
                //to display error message
                return View(ship);
            }
        }
        // GET: AdminController/Delete/5
        public ActionResult DeleteShippingRate(int? id)
        {
            
            if (id == null)
            {
                //Return to listing page, not allowed to edit
                return RedirectToAction("Index");
            }
            //gets details of shipping rate in a new view
            ShippingRate ship = shipContext.GetDetails(id.Value);
            if (ship == null)
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
                if (staff.appointment == "Admin Manager")
                {
                    return View(ship);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteShippingRate(int id)
        {  
            shipContext.Delete(id);
            return RedirectToAction("ViewShippingRate");
        }

        public IActionResult Logout()
        {
            // Perform logout logic here
            // For example, clear session variables, sign out the user, etc.

            // Redirect to the login page or any other desired page
            return RedirectToAction("Index", "Home");
        }
        // GET: AdminController/Details/5
        public ActionResult DetailsShippingRate(int id)
        {
            

            ShippingRate shippin = shipContext.GetDetails(id);
            ShippingRate shipDetails = MapToShipRate(shippin);
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Admin Manager")
                {
                    return View(shipDetails);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ShippingRate MapToShipRate(ShippingRate shippin)
        {
            string branchName = "";
            if (shippin.ShippingRateID != null)
            {
                // Retrieve a list of all shipping rates from the database
                List<ShippingRate> shipList = shipContext.GetAllShipRates();
            }

            ShippingRate shipDetails = new ShippingRate
            {
                ShippingRateID = shippin.ShippingRateID,
                fromCity = shippin.fromCity,
                fromCountry = shippin.fromCountry,
                toCity = shippin.toCity,
                toCountry = shippin.toCountry,
                shippingRate = shippin.shippingRate,
                currency = shippin.currency,
                transitTime = shippin.transitTime,
                lastUpdatedBy = shippin.lastUpdatedBy,

            };

            return shipDetails;
        }
        public ActionResult ViewMemberBirthdayVoucher(Member member)
        {
            // Retrieve a list of all members from the database
            List<Member> memberList = memberContext.GetAllMembers();
            List<Member> birthdayList = new List<Member>();
            // Iterate through each member in the memberList to find members with birthdays in the current month
            foreach (Member members in memberList)
            {
                if (members.birthDate.Value.Month == DateTime.Now.Month)
                {
                    birthdayList.Add(members);
                }
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
                if (staff.appointment == "Admin Manager")
                {
                    return View(birthdayList);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Confirm(int id)
        {
            TempData["SuccessMessage"] = false;
            // Retrieve details of a specific member based on the provided id from the database
            Member member = memberContext.GetDetails(id);
            // Retrieve a list of all cash vouchers from the database
            List<CashVoucher> cashVouchers = cashContext.GetAllCashVoucher();
            // Iterate through each cash voucher in the cashVouchers list to find a match for the member's telephone number
            foreach (CashVoucher cashVoucher in cashVouchers)
            {
                if(cashVoucher.receiverTelNo == member.telNo)
                {
                    // Check if the cash voucher has a specific issuing code '1' and was issued in the current year
                    if (cashVoucher.issueingCode == '1' && cashVoucher.dateTimeIssued.Year == DateTime.Now.Year)
                    {
                        return View("VoucherDetails");
                    }
                }
            }
            // Check if 'id' is null, and the "VoucherDetails" TempData flag is set to true
            if (id==null && TempData["VoucherDetails"] != null && (bool)TempData["VoucherDetails"])
            {
               
                
                return View("VoucherDetails");
            }
            else
            {
               
                
                return RedirectToAction("ConfirmIssueVoucher", new {id=member.memberID});
            }
            
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmIssueVoucher(CashVoucher cashvoucher)
        {


            // Create a new cash voucher in the database
            cashContext.Create(cashvoucher);
            TempData["Message"] = "Happy Birthday You Received a $10 Voucher!";
            // Check if the user is logged in (checks the "Role" session variable)
            if (HttpContext.Session.GetString("Role") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string loginid = HttpContext.Session.GetString("Role");
                Staff staff = staffContext.FindStaffByLoginID(loginid);
                if (staff.appointment == "Admin Manager")
                {
                    return RedirectToAction("ViewMemberBirthdayVoucher");
                }
            }
            return RedirectToAction("Index", "Home");


        }

        [HttpPost]
        public ActionResult Create()
        {
            // Logic to handle Create button click
            TempData["VoucherDetails"] = true;

            return RedirectToAction("Confirm");
        }
        public ActionResult ConfirmIssueVoucher(int id)
        {
            // Retrieve details of a specific member based on the provided id from the database
            Member member = memberContext.GetDetails(id);
            CashVoucher cashVoucher = new CashVoucher();

            // Set values for the 'cashVoucher' object
            cashVoucher.staffID = 3;
            cashVoucher.Amount = 10;
            cashVoucher.currency = "SGD";
            cashVoucher.issueingCode = '1';
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
                if (staff.appointment == "Admin Manager")
                {
                    return View(cashVoucher);
                }
            }
            return RedirectToAction("Index", "Home");
        }

    } 
}

