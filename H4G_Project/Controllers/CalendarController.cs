using Microsoft.AspNetCore.Mvc;
using H4G_Project.DAL;
using H4G_Project.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Google.Cloud.Firestore;

namespace H4G_Project.Controllers
{
    public class CalendarController : Controller
    {
        private EventsDAL eventsDAL = new EventsDAL();

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await eventsDAL.GetAllEvents();

            return Json(events.Select(e => new
            {
                id = e.Id,
                title = e.Name,
                start = e.Start.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.End.HasValue ? e.End.Value.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss") : null
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] EventRegistrationRequest request)
        {
            try
            {
                // Get event details
                var events = await eventsDAL.GetAllEvents();
                var eventDetails = events.FirstOrDefault(e => e.Id == request.EventId);

                if (eventDetails == null)
                {
                    return Json(new { success = false, message = "Event not found" });
                }

                // Create registration object
                EventRegistration registration = new EventRegistration
                {
                    EventId = request.EventId,
                    EventName = request.EventName,
                    Role = request.Role,
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PreferredRole = request.PreferredRole,
                    Skills = request.Skills,
                    DietaryRequirements = request.DietaryRequirements,
                    EmergencyContact = request.EmergencyContact,
                    PaymentStatus = "Pending",
                    PaymentAmount = request.Role == "Volunteer" ? 0.0 : 50.0,
                    RegistrationDate = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                // Generate mock QR code
                string mockQrCode = $"EVENT-{request.EventId.Substring(0, Math.Min(6, request.EventId.Length))}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                registration.QrCode = mockQrCode;

                // Save registration
                string registrationId = await eventsDAL.AddRegistration(registration);

                if (string.IsNullOrEmpty(registrationId))
                {
                    return Json(new { success = false, message = "Failed to create registration" });
                }

                // If volunteer, auto-confirm (no payment needed)
                if (request.Role == "Volunteer")
                {
                    await eventsDAL.UpdatePaymentStatus(registrationId, "Completed", mockQrCode);
                }

                return Json(new
                {
                    success = true,
                    message = "Registration created successfully",
                    registrationId = registrationId,
                    qrCode = mockQrCode,
                    paymentAmount = registration.PaymentAmount,
                    requiresPayment = request.Role == "Participant"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationRequest request)
        {
            try
            {
                bool success = await eventsDAL.UpdatePaymentStatus(
                    request.RegistrationId,
                    "Completed",
                    request.QrCode
                );

                if (success)
                {
                    var registration = await eventsDAL.GetRegistrationById(request.RegistrationId);
                    return Json(new
                    {
                        success = true,
                        message = "Payment confirmed successfully",
                        registration = new
                        {
                            id = registration.Id,
                            eventName = registration.EventName,
                            fullName = registration.FullName,
                            role = registration.Role,
                            qrCode = registration.QrCode,
                            paymentStatus = registration.PaymentStatus
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to confirm payment" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }

    // Request models
    public class EventRegistrationRequest
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredRole { get; set; }
        public string Skills { get; set; }
        public string DietaryRequirements { get; set; }
        public string EmergencyContact { get; set; }
    }

    public class PaymentConfirmationRequest
    {
        public string RegistrationId { get; set; }
        public string QrCode { get; set; }
    }
}