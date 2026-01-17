using Microsoft.AspNetCore.Mvc;
using H4G_Project.DAL;
using H4G_Project.Models;
using FirebaseAdmin.Auth;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace H4G_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly UserDAL _userContext = new UserDAL();
        private readonly EventsDAL _eventsDAL = new EventsDAL();


        // Attendance system
        public IActionResult ScanQR()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> MarkAttendance([FromBody] QRAttendanceRequest request)
        {
            if (string.IsNullOrEmpty(request.QrCode))
                return Json(new { success = false, message = "Invalid QR code" });

            string scannedQr = request.QrCode.Trim();


            // ✅ Find the event matching the scanned QR
            var events = await _eventsDAL.GetAllEvents();



            var ev = events.FirstOrDefault(e => e.QrCode == scannedQr);

            if (ev == null)
            {
                return Json(new { success = false, message = "QR code not recognized" });
            }



            // ✅ Find current user registration
            string? userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return Json(new { success = false, message = "User not logged in" });

            var registrations = await _eventsDAL.GetRegistrationsByEventId(ev.Id);
            var registration = registrations.FirstOrDefault(r => r.Email == userEmail);

            if (registration == null)
                return Json(new { success = false, message = "You are not registered for this event" });

            // ✅ Mark attendance
            registration.Attendance = true;
            await _eventsDAL.UpdateRegistration(registration);

            return Json(new { success = true, message = "Attendance marked!" });
        }




        public class QRAttendanceRequest
        {
            public string QrCode { get; set; }
        }



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
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Index", "Home");

            var events = await _eventsDAL.GetEventsByUserEmail(userEmail);

            var eventComments = new Dictionary<string, List<CommentVM>>();

            foreach (var ev in events)
            {
                var comments = await _eventsDAL.GetCommentTree(ev.Id);
                eventComments[ev.Id] = comments;
            }

            ViewBag.EventComments = eventComments;
            return View(events);
        }


        // Add comment or reply
        [HttpPost]
        public async Task<IActionResult> AddComment(string eventId, string comment, string parentCommentId = null)
        {
            if (string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(eventId))
            {
                TempData["Message"] = "Invalid comment submission.";
                return RedirectToAction("ViewAllEvents");
            }

            string username = HttpContext.Session.GetString("Username") ?? "Anonymous";
            string role = HttpContext.Session.GetString("UserRole") ?? "Participant";
            string email = HttpContext.Session.GetString("UserEmail") ?? "";

            await _eventsDAL.AddComment(eventId, username, email, comment, role, parentCommentId);

            return RedirectToAction("ViewAllEvents");
        }






    }
}
