using Microsoft.AspNetCore.Mvc.Rendering;

namespace H4G_Project.Models
{
    /*
    public class CurrencyViewModel
    {
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Amount { get; set; }
        public List<SelectListItem> AvailableCurrencies { get; set; }
    }*/

    public class CurrencyViewModel
    {
        public double SGDExchangeRate { get; set; }
        public Dictionary<string, double> OtherCurrencies { get; set; }
    }

}
