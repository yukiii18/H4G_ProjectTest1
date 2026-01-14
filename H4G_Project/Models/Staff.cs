using Google.Cloud.Firestore;
using Google.Type;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using DateTime = System.DateTime;


namespace H4G_Project.Models
{
    [FirestoreData]
    public class Staff
    {
        [FirestoreProperty]
        [Display(Name = "Username")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [Required(ErrorMessage = "Please input a name!")]
        public string Username { get; set; }

        [FirestoreProperty]
        [Display(Name = "Password")]
        [StringLength(25, ErrorMessage = "Password cannot exceed 25 characters")]
        [Required(ErrorMessage = "Please input a password!")]
        public string Password { get; set; }

        [FirestoreProperty]
        [Display(Name = "Email")]
        [RegularExpression(@"^.+@.+\..+$", ErrorMessage = "Invalid Email")]
        [StringLength(50, ErrorMessage = "Email address cannot exceed 50 characters")]
        [Required(ErrorMessage = "Please input an email address!")]
        [EmailAddress]
        [ValidateEmailExists]
        public string Email { get; set; }

        [FirestoreProperty]
        [Display(Name = "LastDayOfService")]
        public DateTime LastDayOfService { get; set; }

        /*[Display (Name = "ID")]
        public int staffID { get; set; }
        [Display (Name = "Name")]
        [StringLength (50,ErrorMessage = "Name length cannot be more than 50 characters")]
        [Required (ErrorMessage = "Please input a name!")]
        public string staffName { get; set; }
        [Display (Name = "Login ID")]
        [StringLength (20,ErrorMessage = "Login ID length cannot be more than 20 characters")]
        [Required (ErrorMessage = "Please input a login ID!")]
        [RegularExpression(@"^.+@.+\..+$", ErrorMessage = "Invalid Email")]
        [EmailAddress]
        [ValidateEmailExists]
        public string loginID { get; set; }
        [Display(Name = "Password")]
        [StringLength(25, ErrorMessage = "Password length cannot be more than 25 characters")]
        [Required(ErrorMessage = "Please input a valid password!")]
        [DataType (DataType.Password)]
        public string password { get; set; }
        [Display(Name = "Role")]
        [StringLength(25)]
        [RegularExpression("^(Front Office Staff|Station Manager|Admin Manager|Delivery Man)$")]
        public string? appointment { get; set; }
        [Display (Name = "Contact Number")]
        [RegularExpression (@"^\+\d{1,3}\d{7,15}$")]
        public string? officeTelNo { get; set; }
        [Display(Name = "Location")]
        [StringLength(50, ErrorMessage = "Location cannot be more than 50 characters")]
        public string? location { get; set; }
        */
    }
}
