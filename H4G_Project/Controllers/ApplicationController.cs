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
        EventsDAL eventsContext = new EventsDAL();
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

        // Show list of applications with tabs for client applications and volunteer registrations
        public async Task<ActionResult> Index()
        {
            var applications = await applicationContext.GetAllApplications();

            // Sort applications: Pending first, then Declined, then Approved at bottom
            var sortedApplications = applications
                .OrderBy(a => a.Status == "Pending" ? 0 : (a.Status == "Declined" ? 1 : 2))
                .ToList();

            return View(sortedApplications);
        }

        public async Task<ActionResult> VolunteerApplications()
        {
            var applications = await applicationContext.GetAllVolunteerApplications();

            // Sort applications: Pending first, then Declined, then Approved at bottom
            var sortedApplications = applications
                .OrderBy(a => a.Status == "Pending" ? 0 : (a.Status == "Declined" ? 1 : 2))
                .ToList();

            return View(sortedApplications);
        }

        // Show volunteer registrations page
        public async Task<ActionResult> VolunteerRegistrations()
        {
            var volunteerRegistrations = await eventsContext.GetVolunteerRegistrations();
            var allEvents = await eventsContext.GetAllEvents();

            // Sort volunteer registrations: Pending first, then Declined, then Approved at bottom
            var sortedVolunteerRegistrations = volunteerRegistrations
                .OrderBy(v => v.Status == "Pending" ? 0 : (v.Status == "Declined" ? 1 : 2))
                .ToList();

            ViewBag.AllEvents = allEvents;
            return View(sortedVolunteerRegistrations);
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

        [HttpGet]
        public IActionResult NewVolunteerApplication()
        {
            return View("~/Views/Home/CreateVolunteer.cshtml");
        }

        // Handle form submission from volunteer form (POST)
        [HttpPost]
        public async Task<IActionResult> NewVolunteerApplication(VolunteerApplication application, IFormFile resume)
        {
            try
            {
                // Validate DateOfBirth if provided
                if (!string.IsNullOrEmpty(application.DateOfBirth))
                {
                    if (DateTime.TryParse(application.DateOfBirth, out DateTime parsedDate))
                    {
                        if (parsedDate.Date > DateTime.Today)
                        {
                            ViewBag.ErrorMessage = "Date of Birth cannot be in the future.";
                            return View("~/Views/Home/CreateVolunteer.cshtml", application);
                        }

                        // Optional: Check for reasonable age limits (e.g., not older than 120 years)
                        var minDate = DateTime.Today.AddYears(-120);
                        if (parsedDate.Date < minDate)
                        {
                            ViewBag.ErrorMessage = "Please enter a valid date of birth.";
                            return View("~/Views/Home/CreateVolunteer.cshtml", application);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid date format for Date of Birth.";
                        return View("~/Views/Home/CreateVolunteer.cshtml", application);
                    }
                }

                bool success = await applicationContext.AddVolunteerApplication(application, resume);

                if (success)
                {
                    Console.WriteLine("Application saved successfully");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Failed to save application");
                    ViewBag.ErrorMessage = "Failed to save application. Please try again.";
                    return View("~/Views/Home/CreateVolunteer.cshtml", application);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NewApplication: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("~/Views/Home/CreateVolunteer.cshtml", application);
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

                    TempData["SuccessMessage"] = $"Application for {applicantName} declined successfully.";
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

        // Approve volunteer registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVolunteerRegistration(string registrationId, string volunteerName, string volunteerEmail)
        {
            try
            {
                bool success = await eventsContext.UpdateVolunteerRegistrationStatus(registrationId, "Approved");

                if (success)
                {
                    TempData["SuccessMessage"] = $"Volunteer registration for {volunteerName} approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve volunteer registration.";
                }

                return RedirectToAction("VolunteerRegistrations");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving volunteer registration: {ex.Message}";
                return RedirectToAction("VolunteerRegistrations");
            }
        }

        // Reject volunteer registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectVolunteerRegistration(string registrationId, string volunteerName, string volunteerEmail)
        {
            try
            {
                bool success = await eventsContext.UpdateVolunteerRegistrationStatus(registrationId, "Declined");

                if (success)
                {
                    TempData["SuccessMessage"] = $"Volunteer registration for {volunteerName} declined successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to decline volunteer registration.";
                }

                return RedirectToAction("VolunteerRegistrations");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error declining volunteer registration: {ex.Message}";
                return RedirectToAction("VolunteerRegistrations");
            }
        }
    }
}