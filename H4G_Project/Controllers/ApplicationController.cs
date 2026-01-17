using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using H4G_Project.DAL;
using H4G_Project.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Firebase.Storage;
using Google.Cloud.Firestore.V1;
using System.Dynamic;


namespace H4G_Project.Controllers
{
    public class ApplicationController : Controller
    {
        ApplicationDAL applicationContext = new ApplicationDAL();

        // Show list of applications
        public async Task<ActionResult> Index()
        {
            var applications = await applicationContext.GetAllApplications();
            
            // Sort applications: Pending first, then Declined, then Approved at bottom
            // Note: Applications remain "Pending" until user account is actually created
            var sortedApplications = applications
                .OrderBy(a => a.Status == "Approved" ? 2 : (a.Status == "Declined" ? 1 : 0))
                .ToList();
            
            return View(sortedApplications);
        }

        // Show the form (GET)
        [HttpGet]
        public IActionResult NewApplication()
        {
            return View();
        }

        // Handle form submission (POST)
        [HttpPost]
        public async Task<IActionResult> NewApplication(Application application, IFormFile medicalReport)
        {
            bool success = await applicationContext.AddApplication(application, medicalReport);

            if (success)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Error");
            }
        }

        // Approve application
        [HttpPost]
        public async Task<IActionResult> ApproveApplication(string applicationId, string applicantName, string applicantEmail)
        {
            try
            {
                // Don't update status here - only redirect to user creation
                // Status will be updated to "Approved" only when user account is successfully created
                return RedirectToAction("AddUser", "Staff", new { 
                    applicantName = applicantName, 
                    applicantEmail = applicantEmail,
                    applicationId = applicationId
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing application: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Reject application
        [HttpPost]
        public async Task<IActionResult> RejectApplication(string applicationId, string applicantName)
        {
            try
            {
                // Update application status to "Declined"
                bool success = await applicationContext.UpdateApplicationStatus(applicationId, "Declined");
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Application for {applicantName} has been declined.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to decline application.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error declining application: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}