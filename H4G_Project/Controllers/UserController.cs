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

            if (string.IsNullOrEmpty(comment))
                return BadRequest("Comment cannot be empty");

            bool success = await _eventsDAL.AddComment(eventId, username, email, comment);

            if (success)
                return RedirectToAction("ViewAllEvents");

            return BadRequest("Failed to add comment");
        }




    }
}
