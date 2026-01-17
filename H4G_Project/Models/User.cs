using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using DateTime = System.DateTime;

namespace H4G_Project.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        [Display(Name = "Username")]
        [Required]
        public string? Username { get; set; } = string.Empty;

        [FirestoreProperty]
        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [FirestoreProperty]
        [Display(Name = "Role")]
        [Required]
        public string? Role { get; set; } = string.Empty;

        [FirestoreProperty]
        [Display(Name = "Engagement Type")]
        public string? EngagementType { get; set; } = "Ad hoc engagement"; // Default engagement type
    }
}