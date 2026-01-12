using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class DeliveryHistory
    {
        [Display(Name = "Record ID")]
        public int recordID { get; set; }
        [Display (Name = "Parcel ID")]
        public int parcelID { get; set; }
        [Display (Name = "Description")]
        [StringLength (255,ErrorMessage = "Description length cannot exceed 255 characters")]
        public string description { get; set; }

    }
}
