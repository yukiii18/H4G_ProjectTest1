using Microsoft.AspNetCore.Mvc;
using H4G_Project.DAL;
using H4G_Project.Models;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
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
        public IActionResult AddUser(string applicantName = "", string applicantEmail = "")
        {
            // Pass application data to view for pre-population
            ViewBag.ApplicantName = applicantName;
            ViewBag.ApplicantEmail = applicantEmail;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(IFormCollection form)
        {
            string username = form["Username"];
            string email = form["Email"];
            string role = form["Role"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("", "Please fill in all required fields.");
                return View();
            }

            // Generate a random temporary password
            string temporaryPassword = GenerateRandomPassword();

            try
            {
                // 1️⃣ Create user in Firebase
                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                {
                    Email = email,
                    Password = temporaryPassword
                });

                await _userContext.AddUser(new User
                {
                    Username = username,
                    Email = email,
                    Role = role
                });

                // Pass the generated password to the view for display
                TempData["SuccessMessage"] = $"User account created successfully for {username}.";
                TempData["GeneratedPassword"] = temporaryPassword;
                TempData["UserEmail"] = email;
                return View("UserCreated");
            }
            catch (FirebaseAuthException ex)
            {
                ModelState.AddModelError("", $"Error creating user: {ex.Message}");
                return View();
            }
        }

        // Show create event page
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new Event());
        }

        // Handle create event
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            string Name,
            DateTime Start,
            DateTime? End,
            string Details,
            DateTime RegistrationDueDate,
            int MaxParticipants)
        {
            if (string.IsNullOrEmpty(Name))
            {
                ModelState.AddModelError("Name", "Event name is required");
                return View();
            }

            // Create Event object
            Event ev = new Event
            {
                Name = Name,
                Start = Timestamp.FromDateTime(Start.ToUniversalTime()),
                End = End.HasValue
                    ? Timestamp.FromDateTime(End.Value.ToUniversalTime())
                    : null,
                RegistrationDueDate = Timestamp.FromDateTime(RegistrationDueDate.ToUniversalTime()),
                MaxParticipants = MaxParticipants,
                Details = Details
            };

            bool success = await _eventsDAL.AddEvent(ev);

            if (success)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(CreateEvent));
            }

            ModelState.AddModelError("", "Failed to create event.");
            return View();
        }


        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ===============================
        // DELETE USER (FOR TESTING)
        // ===============================
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required.";
                return RedirectToAction("Index");
            }

            try
            {
                // 1️⃣ Delete from Firebase Authentication
                var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                await FirebaseAuth.DefaultInstance.DeleteUserAsync(userRecord.Uid);

                // 2️⃣ Delete from your Firestore database
                await _userContext.DeleteUser(email);

                TempData["SuccessMessage"] = $"User {email} deleted successfully from both Authentication and Database.";
            }
            catch (FirebaseAuthException ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user from Authentication: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
