using Microsoft.CodeAnalysis;
using System.Text.Json;

namespace H4G_Project.DAL
{
    public class InternationalCallingCodeDAL
    {
        List<InternationalCallingCode> codes = new List<InternationalCallingCode>();
        public InternationalCallingCodeDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("InternationalCallingCode.json");

            using (StreamReader sr = new StreamReader("InternationalCallingCode.json"))
            {
                string jsonData = sr.ReadToEnd();
                codes = JsonSerializer.Deserialize<List<InternationalCallingCode>>(jsonData);
            }
        }
        // Get the international calling code based on the country selected (if don't have return empty string)
        public string GetCode(string country)
        {
            foreach (var item in codes)
            {
                if (item.country == country)
                {
                    return item.code.ToString();
                }
            }
            return "";
        }
    }
    public class InternationalCallingCode
    {
        public string country { get; set; }
        public string id { get; set; }
        public int code { get; set; }
    }
}
