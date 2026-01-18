using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using H4G_Project.DAL;
using H4G_Project.Models;
using H4G_Project.Services;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Google.Cloud.Firestore.V1;
using System.Dynamic;
using System.Net;
using System.Net.Mail;
using Google.Cloud.Storage.V1;

namespace H4G_Project.Controllers
{
    public class ApplicationController : Controller
    {
        ApplicationDAL applicationContext = new ApplicationDAL();
        private readonly IConfiguration _config;
        private readonly NotificationService _notificationService;

        // Combine both into one constructor
        public ApplicationController(IConfiguration config, NotificationService notificationService)
        {
            _config = config;
            _notificationService = notificationService;
        }

        // Example usage
        public async Task<IActionResult> NotifyUser(string token)
        {
            await _notificationService.SendNotificationAsync(token, "Update", "Your application status changed.");
            return Ok("Notification sent");
        }


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
            return View("~/Views/Home/Create.cshtml");
        }

        // Handle form submission (POST)
        [HttpPost]
        public async Task<IActionResult> NewApplication(Application application, IFormFile medicalReport, IFormFile idDocument)
        {
            try
            {
                // Add some debugging
                Console.WriteLine($"Received application for: {application?.CaregiverName}");
                Console.WriteLine($"Medical report file: {medicalReport?.FileName} ({medicalReport?.Length} bytes)");
                Console.WriteLine($"ID document file: {idDocument?.FileName} ({idDocument?.Length} bytes)");

                bool success = await applicationContext.AddApplication(application, medicalReport, idDocument);

                if (success)
                {
                    Console.WriteLine("Application saved successfully");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Failed to save application");
                    ViewBag.ErrorMessage = "Failed to save application. Please try again.";
                    return View("~/Views/Home/Create.cshtml", application);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NewApplication: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("~/Views/Home/Create.cshtml", application);
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
                return RedirectToAction("AddUser", "Staff", new
                {
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(string applicationId, string applicantName, string applicantEmail)
        {
            try
            {
                bool success = await applicationContext.UpdateApplicationStatus(applicationId, "Declined");

                if (success)
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(
                            _config["EmailSettings:SenderEmail"],
                            _config["EmailSettings:SenderPassword"]
                        ),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_config["EmailSettings:SenderEmail"]),
                        Subject = "Application Status Update",
                        Body = $"Dear {applicantName},<br/><br/>We regret to inform you that your application has been declined.<br/><br/>Regards,<br/>Support Team",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(applicantEmail);

                    await smtpClient.SendMailAsync(mailMessage);

                    TempData["SuccessMessage"] = $"Application for {applicantName} declined and email sent via Gmail.";
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