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
        private readonly IConfiguration _configuration;
        private HttpClient client;
        private string searchString;

        private const string prefLabel = @"<http://www.w3.org/2008/05/skos#prefLabel>";
        private const string tukutuku = @"<http://www.w3.org/2008/05/skos#tukutuku>";
        private const string altLabel = @"<http://www.w3.org/2008/05/skos#altLabel>";
        private const string usedFor = @"<http://www.w3.org/2008/05/skos#usedFor>";
        private const string whakamārama = @"<http://www.w3.org/2008/05/skos#whakamārama>";
        private const string scopeNote = @"<http://www.w3.org/2008/05/skos#scopeNote>";
        private const string related = @"<http://www.w3.org/2008/05/skos#related>";
        private const string narrower = @"<http://www.w3.org/2008/05/skos#narrower>";
        private const string broader = @"<http://www.w3.org/2008/05/skos#broader>";
        private const string title = @"<http://national.library.records/#title>";
        private const string isbn = @"<http://national.library.records/#isbn>";
        private const string link = @"<http://national.library.records/#link>";


        public TermModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public ActionResult OnGet(string searchString)
        {
            client = _httpClientFactory.CreateClient("AllegroGraph");
            searchString = searchString.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase);

            var termResult = GetResult(string.Format(@"SELECT distinct ?p ?o {{{0} ?p ?o }}", searchString)).Result;
            var term = new Helper.Term();
            foreach (var match in termResult.values)
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


            var recordResult = GetResult(string.Format(
                @"SELECT distinct ?s ?p ?o {{ 
                ?s ?p ?o.
                ?s <http://national.library.records/#tag> {0}.}}", searchString)).Result;
            var records = new Dictionary<string, Helper.Record>();
            foreach (var match in recordResult.values)
            {
                var s = match[0];
                var p = match[1];
                var o = match[2].Replace("\"", "");

                if (!records.Keys.Any(key => key == s))
                {
                    records.Add(s, new Helper.Record());
                    records[s].RecordId = s;
                }

                switch (p)
                {

                    case title:
                        records[s].Title = o;
                        break;
                    case isbn:
                        records[s].ISBN = o;
                        break;
                    case link:
                        records[s].Link.Add(o);
                        break;
                    default:
                        break;
                }
            }
            ViewData["records"] = records.Values.ToList();


            var broaderResult = GetResult(string.Format(@"SELECT distinct ?o {{{0} <http://www.w3.org/2008/05/skos#broaderTransitive> ?o }}", searchString)).Result;
            List<string> broaderIris = new List<string>();
            foreach (var iri in broaderResult.values) { broaderIris.Add(iri[0]); }
            Helper.result[Helper.broaderResult].Clear();
            Helper.result[Helper.broaderResult].AddRange(broaderIris);


            var narrowerResult = GetResult(string.Format(@"SELECT distinct ?o {{{0} <http://www.w3.org/2008/05/skos#narrowerTransitive> ?o }}", searchString)).Result;
            List<string> narrowerIris = new List<string>();
            foreach (var iri in narrowerResult.values) { narrowerIris.Add(iri[0]); }
            Helper.result[Helper.narrowerResult].Clear();
            Helper.result[Helper.narrowerResult].AddRange(narrowerIris);

            return Page();
        }

        private async Task<Helper.AllegroGraphJsonResult> GetResult(String query)
        {
            StringBuilder uri = new StringBuilder();
            uri.Append(_configuration["repository"]);
            uri.Append(HttpUtility.UrlEncode(query));
            HttpResponseMessage response = await client.GetAsync(uri.ToString());
            return JsonSerializer.Deserialize<Helper.AllegroGraphJsonResult>(response.Content.ReadAsStringAsync().Result);
        }
    }
}
