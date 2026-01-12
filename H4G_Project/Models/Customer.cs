using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class Customer
    {
        public List<CashVoucher>? Vouchers { get; set; }
        public List<Parcel>? Parcels { get; set; }
        public Member? Member { get; set; }
        public List<Inbox>? Inboxes { get; set; }
    }
}
