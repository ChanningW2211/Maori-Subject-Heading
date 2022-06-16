using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;


namespace web
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void OnGet() { }

        [Required]
        [BindProperty]
        public string searchString { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");

            StringBuilder uri = new StringBuilder();
            uri.Append(_configuration["repository"]);
            string query = string.Format(
                @"SELECT distinct ?s {{
                {{ ?s <http://www.w3.org/2008/05/skos#prefLabel> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
                union {{ ?s <http://www.w3.org/2008/05/skos#usedFor> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
                union {{ ?s <http://www.w3.org/2008/05/skos#altLabel> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
                union {{ ?s <http://www.w3.org/2008/05/skos#tukutuku> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}}}", searchString?.ToLower());
            uri.Append(HttpUtility.UrlEncode(query));
            HttpResponseMessage response = await client.GetAsync(uri.ToString());

            var result = JsonSerializer.Deserialize<Helper.AllegroGraphJsonResult>(response.Content.ReadAsStringAsync().Result);
            List<string> iris = new List<string>();
            foreach (var match in result.values)
            {
                iris.Add(match[0]);
            }


            Helper.result[Helper.searchResult].Clear();
            Helper.result[Helper.searchResult].AddRange(iris);

            return RedirectToPage("Result", new { key = Helper.searchResult });
        }
    }
}

