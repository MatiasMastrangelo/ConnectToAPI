using ConnectToAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ConnectToAPI.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> GetData(IFormCollection collection)
		{
			string postcode = collection["postcode"];

			Certificates res = new();

			JObject? json_public_buildings = await GetPublicBuildingsPerformances(postcode);

			if (json_public_buildings != null)
			{
				if (json_public_buildings["column-names"] != null)
					res.PublicBuildingsDataColumnNames = json_public_buildings["column-names"]!.Select(j => j.ToString()).ToList();

				if (json_public_buildings["rows"] != null)
					res.PublicBuildingsData = json_public_buildings["rows"]!.Select(j => j.ToString()).ToList();
			}

			JObject? json_domestic = await GetDomesticPerformances(postcode);

			if (json_domestic != null)
			{
				if (json_domestic["column-names"] != null)
					res.DomesticDataColumnNames = json_domestic["column-names"]!.Select(j => j.ToString()).ToList();

				if (json_domestic["rows"] != null)
					res.DomesticData = json_domestic["rows"]!.Select(j => j.ToString()).ToList();

			}

			return View(res);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[NonAction]
		public async Task<JObject?> GetPublicBuildingsPerformances(string postcode)
		{

			string content = await RequestAPI($"https://epc.opendatacommunities.org/api/v1/display/search?postcode={postcode}");

			if (string.IsNullOrWhiteSpace(content))
				return null;

			return JObject.Parse(content);


		}

		[NonAction]
		public async Task<JObject?> GetDomesticPerformances(string postcode)
		{

			string content = await RequestAPI($"https://epc.opendatacommunities.org/api/v1/domestic/search?postcode={postcode}");

			if (string.IsNullOrWhiteSpace(content))
				return null;

			return JObject.Parse(content);

		}

		[NonAction]
		private async Task<string> RequestAPI(string url)
		{
			string res = "";

			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "bXJtYXN0cmFAZ21haWwuY29tOjY1MDFlZmIwNTdlYzM0ZGIyNjg3MWNkYmJkMjgzNzM0NjRiMDQ1NTE=");
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
				var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();
				res = await response.Content.ReadAsStringAsync();

			}

			return res;
		}
	}
}