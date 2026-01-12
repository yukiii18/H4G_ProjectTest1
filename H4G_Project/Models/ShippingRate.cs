using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class ShippingRate
    {
        [Display (Name = "ID")]
        public int ShippingRateID { get; set; }

        [Display (Name = "Sent From (City)")]
        [StringLength (50,ErrorMessage = "City length cannot be more than 50 characters")]
        [Required (ErrorMessage = "Please input a valid city!")]
        public string fromCity { get; set; }

        [Display(Name = "Sent From (Country)")]
        [StringLength(50, ErrorMessage = "Country length cannot be more than 50 characters")]
        [Required(ErrorMessage = "Please input a valid country!")]
        public string fromCountry { get; set; }

        [Display(Name = "Sent To (City)")]
        [StringLength(50, ErrorMessage = "City length cannot be more than 50 characters")]
        [Required(ErrorMessage = "Please input a valid city!")]
        public string toCity { get; set; }

        [Display(Name = "Sent To (Country)")]
        [StringLength(50, ErrorMessage = "Country length cannot be more than 50 characters")]
        [Required(ErrorMessage = "Please input a valid country!")]
        public string toCountry { get; set; }
        [Display (Name = "Shipping Rate")]
        [DisplayFormat (DataFormatString = "{0:#,##0.##}")]
        [DefaultValue (0)]
        public decimal shippingRate { get; set; }
        [Display (Name = "Currency")]
        [StringLength (3,MinimumLength = 3,ErrorMessage = "Please input a valid currency")]
        [DefaultValue("SGD")]
        public string currency { get; set; }
        [Display (Name = "Estimate time for arrvial")]
        [DefaultValue (1)]
        public int transitTime { get; set; }
        [Display (Name = "StaffID")]
        public int lastUpdatedBy { get; set; }
    }
}
