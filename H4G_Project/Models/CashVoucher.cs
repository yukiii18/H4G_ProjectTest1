using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class CashVoucher
    {
        [Display(Name = "Voucher ID Test")]
        public int cashVoucherID { get; set; }
        [Display(Name = "Staff ID")]
        [Required(ErrorMessage = "Staff ID cannot be empty!")]
        public int staffID { get; set; }
        [Display(Name = "Voucher amount")]
        [Range(0, 999, ErrorMessage = "Voucher amount out of range!")]
        [Required]
        public decimal Amount { get; set; }
        [Display(Name = "Currency")]
        [Required(ErrorMessage = "Please input a currency!")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Please input a valid currency!")]
        [DefaultValue("SGD")]
        public string currency { get; set; }
        [Display(Name = "Issuing reason")]
        [RegularExpression("^(1|2|3)$")]
        [Required]
        public char issueingCode { get; set; }
        [Display(Name = "Receiver's name")]
        [Required(ErrorMessage = "Please input a name!")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters!")]
        public string receiverName { get; set; }
        [Display(Name = "Receiver's contact")]
        [Required(ErrorMessage = "Please input a contact number!")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        [RegularExpression("/^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$/", ErrorMessage = "Invalid Email")]
        public string receiverTelNo { get; set; }
        [Display(Name = "Date Issued")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [Required]
        public DateTime dateTimeIssued { get; set; }
        [Display(Name = "Status")]
        [RegularExpression("^(0|1)$")]
        [Required]
        public string status { get; set; }
    }
}
