using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class DeliveryFailure
    {
        [Display (Name = "Report ID")]
        public int reportID { get; set; }
        [Display (Name = "Parcel ID")]
        [Required]
        public int parcelID { get; set; }
        [Display (Name = "Delivery Man ID")]
        [Required]
        public int deliveryManID { get; set; }
        [Display (Name = "Failure Type")]
        [Required]
        [RegularExpression("^(1|2|3|4)$")]
        public char failureType { get; set; }
        [Display (Name = "Description")]
        [StringLength (255,ErrorMessage = "Description length cannot exceed 255 characters")]
        [Required (ErrorMessage = "Please input a description!")]
        public string description { get; set; }
        [Display (Name = "Station Manager ID")]
        public int? stationMgrID { get; set; }
        [Display(Name = "Follow up Description")]
        [StringLength(255, ErrorMessage = "Description length cannot exceed 255 characters")]
        [Required(ErrorMessage = "Please input a description!")]
        public string? followUpAction { get; set; }
        [Display (Name = "Date created")]
        [DataType (DataType.Date)]
        [DisplayFormat (DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime dateCreated { get; set; }
    }
}
