namespace H4G_Project.Models
{
    public class PendingParcelData
    {
        public int id { get; set; }
        public int? deliveryManId { get; set; }
        public string? errorMsg { get; set; }
    }
    public class AirportParcelData
    {
        public int id { get; set; }
        public string collected { get; set; }
    }
   
    
}
