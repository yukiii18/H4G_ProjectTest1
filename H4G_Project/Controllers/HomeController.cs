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
        private UserDAL userContext = new UserDAL();
        private StaffDAL staffContext = new StaffDAL();

        // GET: HomeController
        // Login page (no time for an actual homepage)
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
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

            if (userContext.GetUserByEmail(memberemail) != null)
            {
                HttpContext.Session.SetString("email", memberemail);
                //HttpContext.Session.SetString("RoleObject", JsonConvert.SerializeObject(userContext.Login(memberemail, memberpassword)));
                return RedirectToAction("Index", "User");
            }
            /*if (staffContext.Login(staffemail, staffpassword) != null)
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

            }*/

            TempData["MemberMessage"] = "Invalid member login details";
            TempData["StaffMessage"] = "Invalid staff login details";
            return RedirectToAction("Index", "Home");
        }

        // GET: HomeController/Create
        // Create a member page
        // Additional dropdown list details are added for country and cities (special feature)

    }
}
