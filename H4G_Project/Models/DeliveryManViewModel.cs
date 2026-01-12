namespace H4G_Project.Models
{
    public class DeliveryManViewModel
    {
        public Staff deliveryMan { get; set; }
        public List<Parcel> parcelList { get; set; }
        public DeliveryManViewModel() 
        { 
            parcelList = new List<Parcel>();
        }
    }
}
