using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;

namespace H4G_Project.Models
{
	public class ParcelWithDelvHist
	{
		[Display(Name = "Parcel ID")]
		public int parcelId { get; set; }

		[Display(Name = "Description")]
		[StringLength(255, ErrorMessage = "Parcel description cannot exceed 255 characters")]
		public string? itemDescription { get; set; }

		[Display(Name = "Sender's name")]
		[Required(ErrorMessage = "Please input a name!")]
		[StringLength(50, ErrorMessage = "Name cannot be more than 50 characters!")]
		public string senderName { get; set; }

		[Display(Name = "Sender's contact")]
		[Required(ErrorMessage = "Please input a contact number!")]
		[StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
		public string senderTelNo { get; set; }

		[Display(Name = "Receiver's name")]
		[Required(ErrorMessage = "Please input a name!")]
		[StringLength(50, ErrorMessage = "Name cannot be more than 50 characters!")]
		public string receiverName { get; set; }

		[Display(Name = "Receiver's contact")]
		[Required(ErrorMessage = "Please input a contact number!")]
		[StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
		public string receiverTelNo { get; set; }

		[Display(Name = "Delivery Address")]
		[Required(ErrorMessage = "Please input a an address!")]
		[StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
		public string deliveryAddress { get; set; }

		[Display(Name = "Sent From (City)")]
		[StringLength(50, ErrorMessage = "City length cannot be more than 50 characters")]
		[Required(ErrorMessage = "Please input a valid city!")]
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

		[Display(Name = "Weight (kg)")]
		[Required(ErrorMessage = "Please input a valid parel weight!")]
		[Range(0, 999, ErrorMessage = "Invalid parcel weight!")]
		[DefaultValue(0)]
		public double parcelWeight { get; set; }

		[Display(Name = "Shipping Rate")]
		[DisplayFormat(DataFormatString = "{0:#,##0.##}")]
		[DefaultValue(0)]
		public decimal deliveryCharge { get; set; }

		[Display(Name = "Currency")]
		[StringLength(3, MinimumLength = 3, ErrorMessage = "Invalid currency!")]
		public string currency { get; set; }

		[Display(Name = "Estimated delivery date")]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
		public DateTime? targetDeliveryDate { get; set; }

		[Display(Name = "Status")]
		[DefaultValue(0)]
		[RegularExpression("^(0|1|2|3|4)$")]
		public char deliveryStatus { get; set; }

		[Display(Name = "Deliveryman ID")]
		public int? deliveryManId { get; set; }

		public string description { get; set; }
	}
}
