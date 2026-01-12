using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class PaymentTransaction
    {
        [Display(Name = "Transaction ID")]
        public int transactionID { get; set; }
        [Display(Name = "Parcel ID")]
        [Required (ErrorMessage = "Please input a valid parcel ID!")]
        public int parcelID { get; set; }
        [Display (Name = "Amount payable")]
        [Required (ErrorMessage = "Please input an amount" )]
        [Range (0,999,ErrorMessage = "Amount is not within range!")]
        [DisplayFormat (DataFormatString = "{0:#,##0.0}")]
        public decimal amtTran { get; set; }
        [Display (Name = "Currency")]
        [Required (ErrorMessage = "Please input a currency!")]
        [StringLength (3,MinimumLength = 3,ErrorMessage = "Please input a valid currency!")]
        [DefaultValue ("SGD")]
        public string currency { get; set; }
        [Display (Name = "Transaction type")]
        [Required(ErrorMessage = "Please choose a transaction type")]
        [RegularExpression("^(CASH|VOUC)$", ErrorMessage = "Invalid word. Only 'CASH' and 'VOUC' are allowed.")]
        [DefaultValue ("SGD")]
        public string tranType { get; set; }
        [Display (Name = "Date of payment")]
        [DataType (DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}",ApplyFormatInEditMode = true)]
        public DateTime tranDate { get; set; }
    }
}
