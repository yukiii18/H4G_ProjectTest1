using Microsoft.AspNetCore.Mvc;
using H4G_Project.DAL;
using H4G_Project.Models;
using FirebaseAdmin.Auth;
using System.Threading.Tasks;

namespace H4G_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly UserDAL _userContext = new UserDAL();
        private readonly EventsDAL _eventsDAL = new EventsDAL();


        // ===============================
        // DASHBOARD
        // ===============================
        public async Task<IActionResult> Index()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Index", "Home");

            User user = await _userContext.GetUserByEmail(email);
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "Participant";

            return View(user);
        }

//        // ===============================
//        // REGISTER
//        // ===============================
//        [HttpPost]
//        public async Task<IActionResult> NewUser(IFormCollection form)
//        {
//            string email = form["Email"];
//            string password = form["Password"];
//            string username = form["Username"];
//            string role = form["Role"];
//
//            try
//            {
//                // Create user in Firebase
//                await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
//                {
//                    Email = email,
//                    Password = password
//                });
//
//                // Save to your database
//                await _userContext.AddUser(new User
//                {
//                    Username = username,
//                    Email = email,
//                    Role = role
//                });
//
//                return RedirectToAction("Index", "Home");
//            }
//            catch (FirebaseAuthException ex)
//            {
//                ModelState.AddModelError("", ex.Message);
//                return View("AddNewUser");
//            }
//        }

        // ===============================
        // FIREBASE TOKEN LOGIN
        // ===============================
        public class FirebaseTokenRequest
        {
            public string Token { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseTokenRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
                return BadRequest("Missing token");

            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Token);
                string email = decoded.Claims["email"].ToString();

                User user = await _userContext.GetUserByEmail(email);
                if (user == null) return Unauthorized();

                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role ?? "Participant");

                return Ok();
            }
            catch
            {
                return Unauthorized();
            }
        }

        // ===============================
        // FALLBACK LOGIN (for existing users not in Firebase Auth)
        // TEMPORARY: For testing purposes only
        // ===============================
        [HttpPost]
        public async Task<ActionResult> LogInUser(IFormCollection form)
        {
            // Retrieve the user from the database by the email
            User user = await _userContext.GetUserByEmail(form["Email"]);

            if (user != null)
            {
                // TEMPORARY: Skip password check for testing
                // TODO: Remove this once users are migrated to Firebase Auth
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role ?? "Participant");
                TempData["Username"] = user.Username;
                TempData.Keep("Username");
                return RedirectToAction("Index", "User");
            }

            ModelState.AddModelError(string.Empty, "User not found. Please check your email.");
            return RedirectToAction("Index", "Home");
        }

        // ===============================
        // PASSWORD RESET
        // ===============================
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(IFormCollection form)
        {
            string email = form["Email"];
            string currentPassword = form["CurrentPassword"];
            string newPassword = form["NewPassword"];
            string confirmPassword = form["ConfirmPassword"];

            // Validate inputs
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(currentPassword) || 
                string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New passwords do not match.";
                return View();
            }

            if (newPassword.Length < 8)
            {
                TempData["ErrorMessage"] = "New password must be at least 8 characters long.";
                return View();
            }

            try
            {
                // 1. Verify current password by trying to sign in
                var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
                
                // Get user by email
                var userRecord = await auth.GetUserByEmailAsync(email);
                
                // Note: Firebase Admin SDK doesn't have a direct way to verify password
                // In a real implementation, you might want to use Firebase Client SDK
                // For now, we'll update the password directly
                
                // 2. Update password in Firebase
                var updateRequest = new UserRecordArgs()
                {
                    Uid = userRecord.Uid,
                    Password = newPassword
                };
                
                await auth.UpdateUserAsync(updateRequest);

                TempData["SuccessMessage"] = "Password reset successfully! You can now login with your new password.";
                return View();
            }
            catch (FirebaseAuthException ex)
            {
                TempData["ErrorMessage"] = $"Error resetting password: {ex.Message}";
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "User not found or invalid email.";
                return View();
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
        // VIEW ALL EVENTS
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ViewAllEvents()
        {
            string? userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Index", "Home");

            // Get only events user is registered for
            var events = await _eventsDAL.GetEventsByUserEmail(userEmail);

            return View(events);
        }

        // Comments Section

        [HttpPost]
        public async Task<IActionResult> AddComment(string eventId, string comment)
        {
            string username = HttpContext.Session.GetString("Username") ?? "Anonymous";
            string email = HttpContext.Session.GetString("UserEmail") ?? "";
            string role = HttpContext.Session.GetString("UserRole") ?? "";

            if (string.IsNullOrEmpty(comment))
                return BadRequest("Comment cannot be empty");

            bool success = await _eventsDAL.AddComment(eventId, username, email, comment, role);

            if (success)
                return RedirectToAction("ViewAllEvents");

            return BadRequest("Failed to add comment");
        }




    }
}
