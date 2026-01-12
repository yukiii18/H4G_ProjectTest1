using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Text.Json;

namespace H4G_Project.DAL
{
    public class countriesDAL
    {
        private Dictionary<string, List<string>> locations { get; set; }
        public countriesDAL()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("countries.json");
            using (StreamReader sr = new StreamReader("countries.json"))
            {
                string jsonData = sr.ReadToEnd();
                locations = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonData);
            }
        }
        //get the countries based from the json file
        public List<SelectListItem> getCountries()
        {
            List<SelectListItem> countryDropDown = new List<SelectListItem>();
            foreach (string countryName in locations.Keys)
            {
                countryDropDown.Add(new SelectListItem { Text = countryName, Value = countryName });
            }
            return countryDropDown;
        }
        // get the cities based on the country selected
        public List<SelectListItem> getCities()
        {
            List<SelectListItem> citiesDropDown = new List<SelectListItem>();
            foreach (KeyValuePair<string, List<string>> pair in locations)
            {
                foreach (string city in pair.Value as List<string>)
                {
                    citiesDropDown.Add(new SelectListItem { Text = city, Value = $"{city},{pair.Key}" });
                }
            }
            return citiesDropDown;
        }

    }
}

