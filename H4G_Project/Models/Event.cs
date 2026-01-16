using Google.Cloud.Firestore;
using System;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    [FirestoreData]
    public class Event
    {
        // Firestore document ID (not stored in Firestore)
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("eventID")]
        public int eventID { get; set; }

        [FirestoreProperty("name")]
        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [FirestoreProperty("details")]
        public string details { get; set; }

        [FirestoreProperty("eventPhoto")]
        public string eventPhoto { get; set; }

        [FirestoreProperty("start")]
        [Required]
        public Timestamp Start { get; set; }

        [FirestoreProperty("end")]
        public Timestamp? End { get; set; }
    }
}
