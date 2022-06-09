using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace web
{
    public class TermModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string prefLabel = @"<http://www.w3.org/2008/05/skos#prefLabel>";
        private const string tukutuku = @"<http://www.w3.org/2008/05/skos#tukutuku>";

        private const string altLabel = @"<http://www.w3.org/2008/05/skos#altLabel>";
        private const string usedFor = @"<http://www.w3.org/2008/05/skos#usedFor>";

        private const string whakamārama = @"<http://www.w3.org/2008/05/skos#whakamārama>";
        private const string scopeNote = @"<http://www.w3.org/2008/05/skos#scopeNote>";

        private const string related = @"<http://www.w3.org/2008/05/skos#related>";
        private const string narrower = @"<http://www.w3.org/2008/05/skos#narrower>";
        private const string broader = @"<http://www.w3.org/2008/05/skos#broader>";


        public TermModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public async Task<ActionResult> OnGetAsync(string searchString)
        {
            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");
            StringBuilder uri = new StringBuilder();
            uri.Append("test?query=");
            string query = string.Format(@"SELECT ?p ?o {{{0} ?p ?o }}", searchString.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase));
            uri.Append(HttpUtility.UrlEncode(query));
            HttpResponseMessage response = await client.GetAsync(uri.ToString());

            var result = JsonSerializer.Deserialize<Model.AllegroGraphJsonResult>(response.Content.ReadAsStringAsync().Result);
            var term = new Model.Term();
            foreach (var match in result.values)
            {
                var key = match[0];
                var value = match[1].Replace("\"", "");

                switch (key)
                {
                    case prefLabel:
                        term.PrefLabel = value;
                        break;
                    case tukutuku:
                        term.Tukutuku.Add(value);
                        break;
                    case altLabel:
                        term.AltLabel = value;
                        break;
                    case usedFor:
                        term.UsedFor.Add(value);
                        break;
                    case whakamārama:
                        term.Whakamārama = value;
                        break;
                    case scopeNote:
                        term.ScopeNote = value;
                        break;
                    case related:
                        term.RelatedTerm.Add(value);
                        break;
                    case narrower:
                        term.NarrowerTerm.Add(value);
                        break;
                    case broader:
                        term.BroaderTerm.Add(value);
                        break;
                    default:
                        break;
                }
            }

            ViewData["term"] = term;

            return Page();
        }
    }
}
