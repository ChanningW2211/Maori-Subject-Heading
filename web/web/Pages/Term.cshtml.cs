using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

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
        private const string scopeNote = @"<http://www.w3.org/2008/05/skos#scopeNote>""";

        private const string related = @"<http://www.w3.org/2008/05/skos#related>";
        private const string narrower = @"<http://www.w3.org/2008/05/skos#narrower>";
        private const string broader = @"<http://www.w3.org/2008/05/skos#broader>";

        public class Term
        {
            public Term()
            {
                RelatedTerm = new List<string>();
                NarrowerTerm = new List<string>();
                BroaderTerm = new List<string>();
                UsedFor = new List<string>();
            }

            public string PrefLabel { get; set; }
            public string Tukutuku { get; set; }

            public string AltLabel { get; set; }
            public List<string> UsedFor { get; set; }

            public string Whakamārama { get; set; }
            public string ScopeNote { get; set; }

            public List<string> RelatedTerm { get; set; }
            public List<string> NarrowerTerm { get; set; }
            public List<string> BroaderTerm { get; set; }
        }

        public TermModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public async Task<ActionResult> OnGetAsync(string searchString)
        {
            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");
            StringBuilder uri = new StringBuilder();
            uri.Append("test?query=");
            string query = string.Format(@"SELECT ?p ?o {{{0} ?p ?o }}", searchString.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase));
            uri.Append(HttpUtility.UrlEncode(query));
            HttpResponseMessage response = await client.GetAsync(uri.ToString());

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result)["values"].ToString();
            string pattern = @"""(.*?)"",\n\s\s\s\s""(.*?)""(.*?"")?";
            Term term = new Term();
            foreach (Match match in Regex.Matches(result, pattern))
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value == "\\" ?
                    match.Groups[3].Value.Replace("\\", "").Replace("\"",""):
                    match.Groups[2].Value;
                switch (key)
                {
                    case prefLabel:
                        term.PrefLabel = value;
                        break;
                    case tukutuku:
                        term.Tukutuku = value;
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
