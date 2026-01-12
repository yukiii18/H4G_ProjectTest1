using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using H4G_Project.Models;
using H4G_Project.DAL;
using System.Diagnostics.Metrics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace H4G_Project.Controllers
{
    public class HomeController : Controller
    {
        private MemberDAL memberContext = new MemberDAL();
        private StaffDAL staffContext = new StaffDAL();
        private countriesDAL countriesContext = new countriesDAL();
        private InternationalCallingCodeDAL internationalCallingCodeContext = new InternationalCallingCodeDAL();
        // GET: HomeController
        // Login page (no time for an actual homepage)
        public ActionResult Index()
        {
            return View();
        }

        
        // check two different forms to see if there are any values in the text field.
        // Run login function to check the db for confirmation on the user for the form with details
        // go to respective pages when login is a success and set storage details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(IFormCollection formData)
        {
            string memberemail = formData["memberlogin"];
            string staffemail = formData["stafflogin"];
            string staffpassword = formData["staffpassword"];

            if (memberContext.Login(memberemail) != null)
            {
                HttpContext.Session.SetString("email",memberemail);
                //HttpContext.Session.SetString("RoleObject", JsonConvert.SerializeObject(memberContext.Login(memberemail, memberpassword)));
                return RedirectToAction("Index", "Member");
            }
            if (staffContext.Login(staffemail, staffpassword) != null)
            {
                Staff staff = staffContext.Login(staffemail, staffpassword);
                //HttpContext.Session.SetString("RoleObject", JsonConvert.SerializeObject(staff));
                HttpContext.Session.SetString("Role", staffemail);
                if (staff.appointment == "Front Office Staff")
                {
                    return RedirectToAction("Index", "FrontOffice");
                }
                if (staff.appointment == "Station Manager")
                {
                    return RedirectToAction("Index", "StationManager");
                }
                if (staff.appointment == "Admin Manager")
                {
                    return RedirectToAction("Index", "AdminManager");
                }
                if (staff.appointment == "Delivery Man")
                {
                    return RedirectToAction("Index", "DeliveryMan");
                }
                return NotFound("You currently do not have a role, please confirm with the admin manager your role");

            }

            TempData["MemberMessage"] = "Invalid member login details";
            TempData["StaffMessage"] = "Invalid staff login details";
            return RedirectToAction("Index", "Home");
        }

        // GET: HomeController/Create
        // Create a member page
        // Additional dropdown list details are added for country and cities (special feature)
        public ActionResult Create()
        {
            List<SelectListItem> countryDropdown = countriesContext.getCountries();
            List<SelectListItem> cityDropdown = new List<SelectListItem>();
            countryDropdown.Add(new SelectListItem { Text = "Select an option", Value = null, Selected = true, Disabled = true });
            cityDropdown.Add(new SelectListItem { Text = "Select a country", Value = null, Selected = true, Disabled = true });
            ViewData["Countries"] = countryDropdown;
            ViewData["Cities"] = cityDropdown;
            return View();
        }
        // check if the input memebr details are valid in the member model and create the user and redirect to the login page.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: HomeController/Create
        
        public ActionResult Create(Member member)
        {
            member.telNo = "+" + internationalCallingCodeContext.GetCode(member.country) + member.telNo;
            if (ModelState.IsValid)
            {
                member.memberID = memberContext.CreateUser(member);
                TempData["UserCreationSuccessful"] = true; // login page
                return RedirectToAction("Index", "Home");
            }
            else
            {

                return RedirectToAction("Create", "Home", member);
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            List<Staff> staffList = staffContext.GetAllStaff();
            return View(staffList);
        }
    }
}
