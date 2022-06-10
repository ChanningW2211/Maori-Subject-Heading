using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace web
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet() { }

        [BindProperty]
        public string searchString { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");

            StringBuilder uri = new StringBuilder();
            uri.Append(Model.repository);
            string query = string.Format(
                @"SELECT distinct ?s {{
                optional {{ ?s <http://www.w3.org/2008/05/skos#prefLabel> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
                optional {{ ?s <http://www.w3.org/2008/05/skos#usedFor> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
                optional {{ ?s <http://www.w3.org/2008/05/skos#altLabel> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}}}", searchString?.ToLower());
            uri.Append(HttpUtility.UrlEncode(query));
            HttpResponseMessage response = await client.GetAsync(uri.ToString());

            var result = JsonSerializer.Deserialize<Model.AllegroGraphJsonResult>(response.Content.ReadAsStringAsync().Result);
            List<string> iris = new List<string>();
            foreach (var match in result.values)
            {
                iris.Add(match[0]);
            }

            Model.result[Model.searchResult].Clear();
            Model.result[Model.searchResult].AddRange(iris);

            if (iris.Count() == 0) return Redirect("Error");
            return RedirectToPage("Result", new { key = Model.searchResult });
        }
    }
}

