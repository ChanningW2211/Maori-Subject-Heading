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
        private const string title = @"<http://national.library.records/#title>";
        private const string isbn = @"<http://national.library.records/#isbn>";
        private const string link = @"<http://national.library.records/#link>";


        public TermModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public async Task<ActionResult> OnGetAsync(string searchString)
        {
            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");


            StringBuilder termUri = new StringBuilder();
            termUri.Append("test?query=");
            string termQuery = string.Format(@"SELECT ?p ?o {{{0} ?p ?o }}", searchString.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase));
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
            recordUri.Append("test?query=");
            string recordQuery = string.Format(
                @"SELECT ?s ?p ?o {{ 
                ?s ?p ?o.
                ?s <http://national.library.records/#tag> {0}.}}", searchString.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase));
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


            return Page();
        }
    }
}
