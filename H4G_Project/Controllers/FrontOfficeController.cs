using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Net.Http;
using H4G_Project.DAL;
using Newtonsoft.Json;
using H4G_Project.Models;
using System.Security.Policy;

namespace H4G_Project.Controllers
{
	public class FrontOfficeController : Controller
	{
		private ParcelDAL parcelContext = new ParcelDAL();
		private DeliveryHistoryDAL deliveryHistoryContext = new DeliveryHistoryDAL();
		private ShippingRateDAL shipContext = new ShippingRateDAL();
		private PaymentDAL paymentContext = new PaymentDAL();
		private StaffDAL staffContext = new StaffDAL();
		private CashVoucherDAL cashContext = new CashVoucherDAL();
		private readonly HttpClient _httpClient;

		private List<SelectListItem> countryList = new List<SelectListItem>();
		private List<SelectListItem> cityList = new List<SelectListItem>();
		private List<SelectListItem> voucherList = new List<SelectListItem>();
		private List<string> paymentTypeList = new List<string> { "CASH", "VOUC" };
		private List<string> cashVoucherList = new List<string> { "Pending Collection", "Collected" };
		private List<SelectListItem> currencyCode = new List<SelectListItem> { };
		private List<string> checkCountryExist = new List<string>();
		public FrontOfficeController()
		{
			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri("https://api.freecurrencyapi.com");
			List<ShippingRate> shipList = shipContext.GetAllShipRates();

			// Populate country and city dropdown list
			foreach (ShippingRate s in shipList)
			{
				if (!checkCountryExist.Contains(s.toCountry))
				{
					countryList.Add(
					new SelectListItem
					{
						Value = s.toCountry.ToString(),
						Text = s.toCountry.ToString(),
					});
					checkCountryExist.Add(s.toCountry);
				}
				cityList.Add(
				new SelectListItem
				{
					Value = s.toCity.ToString(),
					Text = s.toCity.ToString(),
				});
			}
            foreach (string s in cashVoucherList)
            {
                voucherList.Add(
                new SelectListItem
                {
                    Value = s,
                    Text = s
                });
            }
        }

		// HomePage for Front Office
		public IActionResult Index()
		{
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View();
				}
			}
			return RedirectToAction("Index", "Home");
		}

		// Create Parcel Record page

		public IActionResult CreateParcelRecord()
		{
			ViewData["countries"] = countryList;
			ViewData["cities"] = cityList;
			ViewData["ShowResult"] = false;
			Parcel parcel = new Parcel
			{
				toCountry = "Singapore",
				toCity = "Singapore"
			};
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View(parcel);
				}
			}
			return RedirectToAction("Index", "Home");
		}

		// After click compute button, show the calculations
		[HttpPost]
		public ActionResult CreateParcelRecord(Parcel parcel)
		{
			List<ShippingRate> shipList = shipContext.GetAllShipRates();
			Decimal rate = 0;

			// Get the shipping rate based on destination
			foreach (ShippingRate s in shipList)
			{
				if (s.toCity == parcel.toCity && s.toCountry == parcel.toCountry)
				{
					rate = s.shippingRate;
					DateTime transit = DateTime.Now.AddDays(s.transitTime);
					string formattedDate = transit.ToString("dd-MM-yyyy");
					parcel.targetDeliveryDate = DateTime.ParseExact(formattedDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
				}
			}

			// Calculate delivery charge
			Decimal deliveryCharge = rate * (decimal)parcel.parcelWeight;
			ViewData["ShippingRate"] = string.Format("S${0:0.00}/kg", rate);
			ViewData["DeliveryChargeRaw"] = string.Format("({0:0.00} x {1}) = S${2:0.00}", rate, parcel.parcelWeight, deliveryCharge);
			ViewData["DeliveryChargeRounded"] = string.Format("S${0:0.00}", Math.Round(deliveryCharge));
			if (Math.Round(deliveryCharge) < 5)
			{
				ViewData["DeliveryChargeFinal"] = "S$5.00";
				parcel.deliveryCharge = 5;
			}
			else
			{
				ViewData["DeliveryChargeFinal"] = string.Format("S${0:0.00}", Math.Round(deliveryCharge));
				parcel.deliveryCharge = Math.Round(deliveryCharge);
			}

			ViewData["Destination"] = parcel.toCity + ", " + parcel.toCountry;
			ViewData["ShowResult"] = true;
			ViewData["countries"] = countryList;
			ViewData["cities"] = cityList;

			// check if logged in
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View(parcel);
				}
			}
			return RedirectToAction("Index", "Home");
		}

		// After click add button, add the parcel record to database
		[HttpPost]
		public ActionResult Add(Parcel parcel)
		{
			parcel.senderTelNo = "+" + parcel.senderTelNo;
			parcel.receiverTelNo = "+" + parcel.receiverTelNo;
			parcel.parcelId = parcelContext.Add(parcel);
			DeliveryHistory dh = new DeliveryHistory();
			dh.parcelID = parcel.parcelId;
			string role = HttpContext.Session.GetString("Role");
			DateTime received = DateTime.Now;
			string formattedDate = received.ToString("d MMM yyyy h:mm:ss tt");
			dh.description = string.Format("Received parcel by {0} on {1}.", role, formattedDate);
			dh.recordID = deliveryHistoryContext.AddRecord(dh);
			return RedirectToAction("Index");
		}

		// see all parcels
		public IActionResult ViewParcels()
		{
			List<Parcel> parcelList = parcelContext.GetAllParcels();
			return View(parcelList);
		}

		// Get all the cities that belongs to the selected country
		public IActionResult GetCities(string country)
		{
			// Get the list of cities based on the selected country
			List<string> cities = shipContext.GetCitiesByCountry(country);

			return Json(cities);
		}

		// Select which parcel to pay
		[HttpGet]
		public IActionResult SelectParcel()
		{
			ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString();
			List<Parcel> parcelList = parcelContext.GetAllParcels();
			List<Parcel> unPaidParcels = new List<Parcel>();
			foreach (Parcel parcel in parcelList)
			{
				decimal paid = 0;
				List<PaymentTransaction> paymentList = paymentContext.GetPaymentByParcelId(parcel.parcelId);
				foreach (PaymentTransaction payment in paymentList)
				{
					paid += payment.amtTran;
				}
				if (paid < parcel.deliveryCharge)
				{
					unPaidParcels.Add(parcel);
				}
			}
			// check if logged in
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View(unPaidParcels);
				}
			}
			return RedirectToAction("Index", "Home");
		}

		// After selecting parcel to pay, check if parcel havent been fully paid
		[HttpPost]
		public ActionResult SelectParcel(int id)
		{
			Parcel parcel = parcelContext.GetParceById(id);
			List<PaymentTransaction> paymentList = paymentContext.GetPaymentByParcelId(id);
			decimal paid = 0;
			foreach (PaymentTransaction payment in paymentList)
			{
				paid += payment.amtTran;
			}
			if (paid < parcel.deliveryCharge)
			{
				return RedirectToAction("CreatePayment", "FrontOffice", new { id = id, paid = paid });
			}
			else
			{
				TempData["ErrorMessage"] = "Please enter a valid parcel to make payment";
				return RedirectToAction("SelectParcel", "FrontOffice");
			}
		}

		// create payment record page
		[HttpGet]
		public ActionResult CreatePayment(int id, decimal paid)
		{
			ViewData["tranType"] = paymentTypeList;
			ViewData["ParcelId"] = id;
			ViewData["Date"] = DateTime.Now;
			ViewData["Paid"] = paid;
			Parcel parcel = parcelContext.GetParceById(id);
			ViewData["Left"] = Math.Round(parcel.deliveryCharge - paid, 2);
			// check if logged in
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View();
				}
			}
			return RedirectToAction("Index", "Home");
		}

		// Add payment record to database
		[HttpPost]
		public IActionResult CreatePayment(PaymentTransaction payment)
		{
			payment.transactionID = paymentContext.Add(payment);
			return RedirectToAction("Index");
		}

		// View all payment made
		public IActionResult ViewPayment()
		{
			List<PaymentTransaction> paymentList = paymentContext.GetAllPayment();
			// check if logged in
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View(paymentList);
				}
			}
			return RedirectToAction("Index", "Home");
		}


		// Log Out of Front Office
		public IActionResult LogOut()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "Home");
		}

		public ActionResult CashVoucherDetails()
		{
			// Set a default value for the ViewData key "Result" to 'false'
			ViewData["Result"] = false;

			// Check if TempData contains the keys "name" and "telNo"
			if (TempData.ContainsKey("name") && TempData.ContainsKey("telNo"))
			{
				// Retrieve a list of cash vouchers based on the "name" and "telNo" TempData values
				List<CashVoucher> cashVoucherList = cashContext.GetSpecificCashVoucher(TempData["name"]?.ToString(), TempData["telNo"]?.ToString());
				// Check if any cash vouchers were found for the provided customer name and telephone number
				if (cashVoucherList.Count > 0)
				{
					ViewData["Result"] = true;

					ViewData["Customer"] = cashVoucherList[0].receiverName;
					// Loop through the cash vouchers in the list
					foreach (CashVoucher cashVoucher in cashVoucherList)
					{
						if (cashVoucher.status == "0")
						{

							cashVoucher.status = "Pending Collection";

						}
						else
						{

							cashVoucher.status = "Collected";

						}

					}
					return View(cashVoucherList);
				}

			}
			// Check if the user is logged in (checks the "Role" session variable)
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View();
				}
			}
			return RedirectToAction("Index", "Home");
		}
		public ActionResult Searcher(string name, string telno)
		{
			// Check if both 'name' and 'telno' parameters are not null
			if (name != null && telno != null)
			{
				// If both 'name' and 'telno' are provided, store them in TempData
				TempData["name"] = name;
				TempData["telNo"] = telno;
			}
			return RedirectToAction("CashVoucherDetails");
		}
		public ActionResult EditVoucherStatus(int id)
		{

			if (id == null)
			{
				return RedirectToAction("Index");
			}


			ViewData["voucherList"] = voucherList;
			// Retrieve details of cash voucher based on the provided 'id' from the database
			CashVoucher cash = cashContext.GetDetails(id);


			// Check if the user is logged in (checks the "Role" session variable)
			if (HttpContext.Session.GetString("Role") == null)
			{
				return RedirectToAction("Index", "Home");
			}
			else
			{
				string loginid = HttpContext.Session.GetString("Role");
				Staff staff = staffContext.FindStaffByLoginID(loginid);
				if (staff.appointment == "Front Office Staff")
				{
					return View(cash);
				}
			}
			return RedirectToAction("Index", "Home");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditVoucherStatus(CashVoucher cash)
		{
			// Check the current status of the cash voucher
			if (cash.status == "Pending Collection")
			{
				cash.status = "0";
			}
			else
			{
				cash.status = "1";
			}
			cashContext.Update(cash);

			return RedirectToAction("CashVoucherDetails");
		}
	

		// page that shows all exchange rate from SGD
		public async Task<ActionResult> CurrencyExchange()
		{
			string[] currencyCodes = {
	"EUR", "USD", "JPY", "BGN", "CZK", "DKK", "GBP", "HUF", "PLN", "RON",
			"SEK", "CHF", "ISK", "NOK", "HRK", "RUB", "TRY", "AUD", "BRL", "CAD",
			"CNY", "HKD", "IDR", "ILS", "INR", "KRW", "MXN", "MYR", "NZD", "PHP",
			"SGD", "THB", "ZAR"
};
			foreach (string code in currencyCodes)
			{
				currencyCode.Add(
					new SelectListItem
					{
						Value = code,
						Text = code,
					});
			}


			string separator = "%2C";
			string currencyCodesString = string.Join(separator, currencyCodes);


			string url = $"https://api.freecurrencyapi.com/v1/latest?apikey=fca_live_B7dVhoLveNwdEeUV8tEkX6iOYXCitHQk3fh2gXlr&currencies={currencyCodesString}&base_currency=SGD";
			HttpResponseMessage response = await _httpClient.GetAsync(url);
			//response.EnsureSuccessStatusCode();

			// Check if the request was successful
			if (response.IsSuccessStatusCode)
			{
				// Read the response content as a string
				string data = await response.Content.ReadAsStringAsync();
				var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, CurrencyExchangeRates>>(data);

				CurrencyViewModel cvm = new CurrencyViewModel();
				cvm.OtherCurrencies = new Dictionary<string, double>();
				foreach (var property in typeof(CurrencyExchangeRates).GetProperties())
				{
					if (property.Name != "SGD")
					{
						double exchangeRate = (double)property.GetValue(jsonObj["data"]);
						string currencyCode = property.Name;
						cvm.OtherCurrencies.Add(currencyCode, exchangeRate);
					}
				}
				ViewData["currencyCode"] = currencyCode;
				// check if logged in
				if (HttpContext.Session.GetString("Role") == null)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					string loginid = HttpContext.Session.GetString("Role");
					Staff staff = staffContext.FindStaffByLoginID(loginid);
					if (staff.appointment == "Front Office Staff")
					{
						return View(cvm);
					}
				}
				return RedirectToAction("Index", "Home");
			}
			else
			{
				Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
			}
			return null;
		}

		// page that shows the exchange rate based on the selected country code and amount
		[HttpPost]
		public async Task<ActionResult> CurrencyExchange(double amount, string from, string to)
		{
			string[] currencyCodes = {
	"EUR", "USD", "JPY", "BGN", "CZK", "DKK", "GBP", "HUF", "PLN", "RON",
			"SEK", "CHF", "ISK", "NOK", "HRK", "RUB", "TRY", "AUD", "BRL", "CAD",
			"CNY", "HKD", "IDR", "ILS", "INR", "KRW", "MXN", "MYR", "NZD", "PHP",
			"SGD", "THB", "ZAR"
};
			foreach (string code in currencyCodes)
			{
				currencyCode.Add(
					new SelectListItem
					{
						Value = code,
						Text = code,
					});
			}
			string url = $"https://api.freecurrencyapi.com/v1/latest?apikey=fca_live_B7dVhoLveNwdEeUV8tEkX6iOYXCitHQk3fh2gXlr&currencies={to}&base_currency={from}";
			ViewData["ShowResult"] = true;
			HttpResponseMessage response = await _httpClient.GetAsync(url);
			if (response.IsSuccessStatusCode)
			{
				// Read the response content as a string
				string data = await response.Content.ReadAsStringAsync();
				var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, CurrencyExchangeRates>>(data);

				CurrencyViewModel cvm = new CurrencyViewModel();
				cvm.OtherCurrencies = new Dictionary<string, double>();
				foreach (var property in typeof(CurrencyExchangeRates).GetProperties())
				{

					if (property.Name == to)
					{
						double exchangeRate = (double)property.GetValue(jsonObj["data"]) * amount;
						ViewData["display"] = amount.ToString() + " " + from + " = " + exchangeRate.ToString("F2") + " " + to;
					}
				}
				ViewData["currencyCode"] = currencyCode;
				// check if logged in
				if (HttpContext.Session.GetString("Role") == null)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					string loginid = HttpContext.Session.GetString("Role");
					Staff staff = staffContext.FindStaffByLoginID(loginid);
					if (staff.appointment == "Front Office Staff")
					{
						return View(cvm);
					}
				}
				return RedirectToAction("Index", "Home");
			}
			else
			{
				Console.WriteLine($"Failed to get data. Status code: {response.StatusCode}");
			}
			return null;
		}
	}
}