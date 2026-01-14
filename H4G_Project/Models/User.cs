using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace H4G_Project.Models
{
    [FirestoreData]
    public class User
    {
        /* [Display (Name = "User ID")]
         public int memberID { get; set; }
         */

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

        /*[Display (Name = "Salutation")]
        [RegularExpression("Mr|Mrs|Ms|Dr|Mdm", ErrorMessage = "Please select a valid salutation!")]
        [Required(ErrorMessage = "Please select a valid salutation!")]
        [StringLength (5,ErrorMessage = "Salutation length somehow more than 5")]
        public string salutation { get; set; }
        [Display(Name = "Contact Number")]
        [RegularExpression(@"^\+\d{1,3}\d{7,15}$")]
        [Required]
        public string telNo { get; set; }
        [Display (Name = "Email")]
        [RegularExpression(@"^.+@.+\..+$", ErrorMessage = "Invalid Email")]
        [StringLength (50,ErrorMessage = "Email address cannot exceed 50 characters")]
        [Required (ErrorMessage = "Please input an email address!")]
        [EmailAddress]
        [ValidateEmailExists]
        public string emailAddr { get; set; }
        [Display (Name = "Password")]
        [StringLength (25,ErrorMessage = "Password cannot exceed 25 characters")]
        [Required (ErrorMessage = "Please input a password!")]
        public string password { get; set; }
        [Display (Name = "Date of Birth")]
        [DataType (DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [ValidateDOB]
        public DateTime? birthDate { get; set; }
        [Display(Name = "City")]
        [StringLength(50, ErrorMessage = "City length cannot be more than 50 characters")]
        public string? city { get; set; }
        [Display(Name = "Country")]
        [StringLength(50, ErrorMessage = "Country length cannot be more than 50 characters")]
        [Required(ErrorMessage = "Please input a valid country!")]
        public string country { get; set; }
        */
    }
}
