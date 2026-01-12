using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class FeedbackEnquiry
    {
        [Display (Name = "Feedback ID")]
        public int feedbackEnquiryID { get; set; }
        [Display (Name = "Member ID")]
        [Required(ErrorMessage = "Member ID canot be null!")]
        public int memberID { get; set; }
        [Display (Name = "Description")]
        [StringLength (255,ErrorMessage = "Description cannot be more than 255 characters")]
        [Required (ErrorMessage = "Description cannot be empty!")]
        public string content { get; set; }
        [Display (Name = "Date and Time Posted")]
        [DataType (DataType.Date)]
        [DisplayFormat (DataFormatString = "{0:dd-MM-yyyy")]
        [Required]
        public DateTime dateTimePosted { get; set; }
        [Required] //check
        public int? staffID { get; set; }
        [Display(Name = "Response Description")]
        [StringLength(255, ErrorMessage = "Description cannot be more than 255 characters")]
        [Required(ErrorMessage = "Description cannot be empty!")]
        public string? response { get; set; }
        [Display (Name = "Status")]
        [Required]
        [RegularExpression("^(0|1)$")]
        public char status { get; set; }
    }
}
