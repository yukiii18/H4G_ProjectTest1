using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    [FirestoreData]
    public class Notification
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        [Required]
        public string UserId { get; set; } // User email who should receive the notification

        [FirestoreProperty]
        [Required]
        public string Title { get; set; }

        [FirestoreProperty]
        [Required]
        public string Message { get; set; }

        [FirestoreProperty]
        public string EventId { get; set; } // Related event ID

        [FirestoreProperty]
        public string EventName { get; set; } // Event name for easy display

        [FirestoreProperty]
        public string Type { get; set; } = "comment"; // notification type: comment, event_update, etc.

        [FirestoreProperty]
        public bool IsRead { get; set; } = false; // Whether user has read the notification

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty]
        public string CreatedBy { get; set; } // Staff member who created the notification
    }
}