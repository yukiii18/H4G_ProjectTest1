using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using H4G_Project.DAL;
using H4G_Project.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DateTime = System.DateTime;

namespace H4G_Project.Controllers
{
    public class StaffController : Controller
    {
        StaffDAL staffContext = new StaffDAL();

        public async Task<IActionResult> Index()
        {
            string memberEmail = HttpContext.Session.GetString("email");
            Staff staff = await staffContext.GetStaffByEmail(memberEmail);
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public async Task<ActionResult> AddNewStaff()
        {
            Staff staff = new Staff();
            return View(staff);
        }

        [HttpPost]
        public async Task<ActionResult> NewStaff(IFormCollection form)
        {
            // Instantiate a new User object with form data
            Staff staff = new Staff
            {
                Username = form["Username"], // Bind the username from the form
                Email = form["Email"], // Bind the email from the form
                Password = form["Password"] // Bind the password from the form
                //LastDayOfService = DateTime.Parse(form["LastDayOfService"])
            };

            // Hash the password before saving to the database
            //user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // The model is valid, proceed to hash the password and save the user to Firestore
            bool addUserResult = await staffContext.AddStaff(staff);

            // Check if the user was successfully added
            if (addUserResult)
            {
                // Redirect to Index Page ie. Main page
                Console.WriteLine("Success");
                return RedirectToAction("Index", "Staff"); //Success
            }
            else
            {
                // If there was a problem saving the user, redirect back to current page
                Console.WriteLine("Error");
                return View(); // Error
            }
        }

        public async Task<ActionResult> LogInUser()
        {
            Staff staff = new Staff();
            return View(staff);
        }

        [HttpPost]
        public async Task<ActionResult> LogInUser(IFormCollection form)
        {
            // Retrieve the user from the database by the email
            // Assuming you have a method like GetUserByEmail in your AuthDAL
            Staff staff = await staffContext.GetStaffByEmail(form["Email"]);

            if (staff != null)
            {
                // Replace this with a hash comparison if you implement hashed password
                if (staff.Password == form["Password"])
                {
                    //var userData = new { user.Username, user.Email };
                    //string userJson = System.Text.Json.JsonSerializer.Serialize(userData);
                    HttpContext.Session.SetString("Username", staff.Username);
                    TempData["Username"] = staff.Username;
                    TempData.Keep("Username");
                    Console.WriteLine(TempData["Username"]);
                    HttpContext.Session.SetString("UserEmail", staff.Email);
                    return RedirectToAction("Index", "Staff");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return RedirectToAction("Index", "Home");

        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}