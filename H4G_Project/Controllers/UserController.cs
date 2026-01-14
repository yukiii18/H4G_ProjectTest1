using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using H4G_Project.DAL;
using H4G_Project.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace H4G_Project.Controllers
{
    public class UserController : Controller
    {
        UserDAL userContext = new UserDAL();

        public async Task<IActionResult> Index()
        {
            string memberEmail = HttpContext.Session.GetString("email");
            User user = await userContext.GetUserByEmail(memberEmail);
            return View();
        }

        public async Task<ActionResult> AddNewUser()
        {
            User user = new User();
            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> NewUser(IFormCollection form)
        {

            // Instantiate a new User object with form data
            User user = new User
            {
                Username = form["Username"], // Bind the username from the form
                Email = form["Email"], // Bind the email from the form
                Password = form["Password"] // Bind the password from the form
            };

            // Hash the password before saving to the database
            //user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // The model is valid, proceed to hash the password and save the user to Firestore
            bool addUserResult = await userContext.AddUser(user);

            // Check if the user was successfully added
            if (addUserResult)
            {
                // Redirect to Index Page ie. Main page
                Console.WriteLine("Success");
                return RedirectToAction("Index", "Home"); //Success
            }
            else
            {
                // If there was a problem saving the user, redirect back to current page
                Console.WriteLine("Error");
                return View("NewUser"); // Error
            }
        }


        public async Task<ActionResult> LogInUser()
        {
            User user = new User();
            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> LogInUser(IFormCollection form)
        {
            // Retrieve the user from the database by the email
            // Assuming you have a method like GetUserByEmail in your AuthDAL
            User user = await userContext.GetUserByEmail(form["Email"]);

            if (user != null)
            {
                // Replace this with a hash comparison if you implement hashed passwords
                Console.WriteLine(user.Password);
                if (user.Password == form["Password"])
                {
                    //var userData = new { user.Username, user.Email };
                    //string userJson = System.Text.Json.JsonSerializer.Serialize(userData);
                    HttpContext.Session.SetString("Username", user.Username);
                    TempData["Username"] = user.Username;
                    TempData.Keep("Username");
                    Console.WriteLine(TempData["Username"]);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    return RedirectToAction("Index", "User");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return RedirectToAction("Index", "Home");

        }

        /*
         public ActionResult Search(string query)
         {
             // Perform the search operation using the query parameter
             ParcelDAL parcelContext = new ParcelDAL();

             string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
             Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

             List<Parcel> searchResults = parcelContext.SearchParcelByUser(customer.User, query);

             // Return a partial view or HTML string containing the search results
             return PartialView("SearchResults", searchResults);
         }
         public ActionResult ViewDelivery(string query)
         {
             // Perform the search operation using the query parameter
             ParcelDAL parcelContext = new ParcelDAL();

             List<Parcel> searchResults = parcelContext.SearchParcelByStaff("6", query);

             // Return a partial view or HTML string containing the search results
             return PartialView("SearchDeliveryResults", searchResults);
         }
         public IActionResult ParcelItem()
         {
             string parcelId = HttpContext.GetRouteData().Values["id"].ToString();
             if (parcelId == "theme.css")
             {
                 return null;
             }
             string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
             Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

             for (int i = 0; i < customer.Parcels.Count; i++)
             {
                 if (customer.Parcels[i].parcelId == Int32.Parse(parcelId))
                 {
                     return View(customer.Parcels[i]);
                 }
             }

             return View(new Parcel());
         }
         [HttpGet]
         public IActionResult SubmitFeedback()
         {
             string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
             Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

             return View(customer);
         }

         [HttpGet]
         public IActionResult SubmitEnquiry()
         {
             string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
             Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

             return View(customer);
         }
         [HttpPost]
         public IActionResult SubmitFeedbackPost()
         {
             string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
             Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

             string feedback = HttpContext.Request.Form["feedback"];
             string rating = HttpContext.Request.Form["rating"];
             string comments = HttpContext.Request.Form["comments"];

             UserDAL userDAL = new UserDAL();
             userDAL.SubmitFeedback(customer.User, feedback, rating, comments);
             return Ok("Feedback submitted");
         }
         */

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}