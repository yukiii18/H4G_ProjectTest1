using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using H4G_Project.DAL;
using H4G_Project.Models;
using Newtonsoft.Json;

namespace H4G_Project.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            MemberDAL memberDAL = new MemberDAL();
            string memberEmail = HttpContext.Session.GetString("email");
            Member member = memberDAL.Login(memberEmail);
            List<CashVoucher> vouchers = memberDAL.GetVouchers(member);
            List<Parcel> parcels = memberDAL.GetParcels(member);
            //List<Inbox> inboxes = memberDAL.GetInbox(member);

            // get first 3 items
            if (parcels.Count > 3)
            {
                parcels = parcels.GetRange(0, 3);
            }
            Customer customer = new Customer();
            customer.Vouchers = vouchers;
            customer.Parcels = parcels;
            customer.Member = member;
            //customer.Inboxes = inboxes;


            HttpContext.Session.SetString("CustomerObject", JsonConvert.SerializeObject(customer));
            return View(customer);
        }
        public IActionResult TrackMyItems()
        {
            string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
            Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

            return View(customer);
        }
        public ActionResult Search(string query)
        {
            // Perform the search operation using the query parameter
            ParcelDAL parcelContext = new ParcelDAL();

            string strCustomerObj = HttpContext.Session.GetString("CustomerObject");
            Customer customer = JsonConvert.DeserializeObject<Customer>(strCustomerObj);

            List<Parcel> searchResults = parcelContext.SearchParcelByUser(customer.Member, query);

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

            MemberDAL memberDAL = new MemberDAL();
            memberDAL.SubmitFeedback(customer.Member, feedback, rating, comments);
            return Ok("Feedback submitted");
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}