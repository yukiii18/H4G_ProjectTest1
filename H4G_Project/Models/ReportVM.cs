using System.Composition;

namespace H4G_Project.Models
{
    public class ReportVM
    {
        public int id {  get; set; }
        public string? description { get; set; }
        public int? details { get; set; }
       
    }
    public class MainViewModel
    {
        public List<ReportVM> Reports { get; set; }
    }
}
