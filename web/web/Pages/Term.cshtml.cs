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

        public async Task<ActionResult> OnGet(string searchString)
        {
            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");
            searchString = searchString.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase);


            StringBuilder termUri = new StringBuilder();
            termUri.Append(_configuration["repository"]);
            string termQuery = string.Format(@"SELECT distinct ?p ?o {{{0} ?p ?o }}", searchString);
            termUri.Append(HttpUtility.UrlEncode(termQuery));
            HttpResponseMessage termResponse = await client.GetAsync(termUri.ToString());
            var termResult = JsonSerializer.Deserialize<Model.AllegroGraphJsonResult>(termResponse.Content.ReadAsStringAsync().Result);
            var term = new Model.Term();
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


            StringBuilder recordUri = new StringBuilder();
            recordUri.Append(_configuration["repository"]);
            string recordQuery = string.Format(
                @"SELECT distinct ?s ?p ?o {{ 
                ?s ?p ?o.
                ?s <http://national.library.records/#tag> {0}.}}", searchString);
            recordUri.Append(HttpUtility.UrlEncode(recordQuery));
            HttpResponseMessage recordResponse = await client.GetAsync(recordUri.ToString());
            var recordResult = JsonSerializer.Deserialize<Model.AllegroGraphJsonResult>(recordResponse.Content.ReadAsStringAsync().Result);
            var records = new Dictionary<string, Model.Record>();
            foreach (var match in recordResult.values)
            {
                var s = match[0];
                var p = match[1];
                var o = match[2].Replace("\"", "");

                if (!records.Keys.Any(key => key == s))
                {
                    records.Add(s, new Model.Record());
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


            StringBuilder broaderUri = new StringBuilder();
            broaderUri.Append(_configuration["repository"]);
            string broaderQuery = string.Format(@"SELECT distinct ?o {{{0} <http://www.w3.org/2008/05/skos#broaderTransitive> ?o }}", searchString);
            broaderUri.Append(HttpUtility.UrlEncode(broaderQuery));
            HttpResponseMessage broaderResponse = await client.GetAsync(broaderUri.ToString());
            var broaderResult = JsonSerializer.Deserialize<Model.AllegroGraphJsonResult>(broaderResponse.Content.ReadAsStringAsync().Result);
            List<string> broaderIris = new List<string>();
            foreach (var iri in broaderResult.values) { broaderIris.Add(iri[0]); }
            Model.result[Model.broaderResult].Clear();
            Model.result[Model.broaderResult].AddRange(broaderIris);


            StringBuilder narrowerUri = new StringBuilder();
            narrowerUri.Append(_configuration["repository"]);
            string narrowerQuery = string.Format(@"SELECT distinct ?o {{{0} <http://www.w3.org/2008/05/skos#narrowerTransitive> ?o }}", searchString);
            narrowerUri.Append(HttpUtility.UrlEncode(narrowerQuery));
            HttpResponseMessage narrowerResponse = await client.GetAsync(narrowerUri.ToString());
            var narrowerResult = JsonSerializer.Deserialize<Model.AllegroGraphJsonResult>(narrowerResponse.Content.ReadAsStringAsync().Result);
            List<string> narrowerIris = new List<string>();
            foreach (var iri in narrowerResult.values) { narrowerIris.Add(iri[0]); }
            Model.result[Model.narrowerResult].Clear();
            Model.result[Model.narrowerResult].AddRange(narrowerIris);

            return Page();
        }
    }
}
