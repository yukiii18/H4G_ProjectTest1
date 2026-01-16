using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    [FirestoreData]
    public class EventRegistration
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("eventId")]
        [Required]
        public string EventId { get; set; }

        [FirestoreProperty("eventName")]
        public string EventName { get; set; }

        [FirestoreProperty("role")]
        [Required]
        public string Role { get; set; } // "Volunteer" or "Participant"

        [FirestoreProperty("fullName")]
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [FirestoreProperty("email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [FirestoreProperty("phoneNumber")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        // Volunteer-specific fields
        [FirestoreProperty("preferredRole")]
        public string PreferredRole { get; set; }

        [FirestoreProperty("skills")]
        public string Skills { get; set; }

        // Participant-specific fields
        [FirestoreProperty("dietaryRequirements")]
        public string DietaryRequirements { get; set; }

        [FirestoreProperty("emergencyContact")]
        public string EmergencyContact { get; set; }

        [FirestoreProperty("paymentStatus")]
        public string PaymentStatus { get; set; } // "Pending", "Completed"

        [FirestoreProperty("paymentAmount")]
        public double PaymentAmount { get; set; }

        [FirestoreProperty("qrCode")]
        public string QrCode { get; set; }

        [FirestoreProperty("registrationDate")]
        public Timestamp RegistrationDate { get; set; }

        [FirestoreProperty("paymentDate")]
        public Timestamp? PaymentDate { get; set; }
    }
}
