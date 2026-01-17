using Microsoft.AspNetCore.Mvc;
using H4G_Project.DAL;
using H4G_Project.Models;
using FirebaseAdmin.Auth;
using System.Threading.Tasks;
using System.Linq;

namespace H4G_Project.Controllers
{
    public class StaffController : Controller
    {
        private readonly StaffDAL _staffContext = new StaffDAL();
        private readonly EventsDAL _eventsDAL = new EventsDAL();
        private readonly UserDAL _userContext = new UserDAL();


        // ===============================
        // DASHBOARD
        // ===============================
        public async Task<IActionResult> Index()
        {
            string? email = HttpContext.Session.GetString("StaffEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Index", "Home");

            Staff? staff = await _staffContext.GetStaffByEmail(email);
            if (staff == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }

            return View(staff);
        }

        // ===============================
        // REPORTS (FILTERABLE)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Reports(string? roleFilter, string? eventFilter)
        {
            var registrations = await _eventsDAL.GetAllRegistrations();

            // Get unique options for dropdowns
            ViewData["AllRoles"] = registrations
                .Where(r => !string.IsNullOrEmpty(r.Role))
                .Select(r => r.Role)
                .Distinct()
                .ToList();

            ViewData["AllEvents"] = registrations
                .Where(r => !string.IsNullOrEmpty(r.EventName))
                .Select(r => r.EventName)
                .Distinct()
                .ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(roleFilter))
                registrations = registrations
                    .Where(r => r.Role?.Equals(roleFilter, System.StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();

            if (!string.IsNullOrEmpty(eventFilter))
                registrations = registrations
                    .Where(r => r.EventName?.Equals(eventFilter, System.StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();

            ViewData["RoleFilter"] = roleFilter;
            ViewData["EventFilter"] = eventFilter;

            return View(registrations);
        }

        // ===============================
        // REGISTER NEW STAFF
        // ===============================
        [HttpPost]
        public async Task<IActionResult> NewStaff(IFormCollection form)
        {
            string? email = form["Email"];
            string? password = form["Password"];
            string? username = form["Username"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("", "Please fill in all required fields.");
                return View("AddNewStaff");
            }

            try
            {
                await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                {
                    Email = email,
                    Password = password
                });

                await _staffContext.AddStaff(new Staff
                {
                    Username = username,
                    Email = email
                });

                return RedirectToAction("Index", "Home");
            }
            catch (FirebaseAuthException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("AddNewStaff");
            }
        }

        // ===============================
        // FIREBASE TOKEN LOGIN
        // ===============================
        public class FirebaseTokenRequest
        {
            public string? Token { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseTokenRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
                return BadRequest("Missing token");

            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Token);
                string email = decoded.Claims["email"]?.ToString() ?? "";

                Staff? staff = await _staffContext.GetStaffByEmail(email);
                if (staff == null)
                    return Unauthorized();

                HttpContext.Session.SetString("StaffUsername", staff.Username ?? "");
                HttpContext.Session.SetString("StaffEmail", staff.Email ?? "");

                return Ok();
            }
            catch
            {
                return Unauthorized();
            }
        }

        // ===============================
        // LOGOUT
        // ===============================
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ===============================
        // ADD USER
        // ===============================
        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(IFormCollection form)
        {
            string username = form["Username"];
            string email = form["Email"];
            string password = form["Password"];
            string role = form["Role"];

            // Password validation
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
                return View();
            }

            try
            {
                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(
                    new UserRecordArgs
                    {
                        Email = email,
                        Password = password
                    });

                await _userContext.AddUser(new User
                {
                    Username = username,
                    Email = email,
                    Role = role
                });

                TempData["SuccessMessage"] = "User account created for " + email;
                return RedirectToAction("AddUser");

            }
            catch (FirebaseAuthException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }





    }
}
