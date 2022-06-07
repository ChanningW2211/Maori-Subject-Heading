using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace web
{
	public class TermModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [ViewData]
        public Term test { get; set; }

        public class Term
        {
            public string PrefLabel { get; set; }
            public string Tukutuku { get; set; }

            public string AltLabel { get; set; }
            public string UsedFor { get; set; }

            public string Whakamārama { get; set; }
            public string ScopeNote { get; set; }

            public List<Uri> RelatedTerm { get; set; }
            public List<Uri> NarrowerTerm { get; set; }
            public List<Uri> BroaderTerm { get; set; }
        }

        public TermModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public async Task<ActionResult> OnGetAsync(string term)
        {
            HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");
            StringBuilder uri = new StringBuilder();
            uri.Append("test?query=");
            string query = string.Format(@"SELECT ?p ?o {{{0} ?p ?o }}", term.Substring(1,term.Length-2).Replace("%2F", "/"));
            uri.Append(HttpUtility.UrlEncode(query));
            HttpResponseMessage response = await client.GetAsync(uri.ToString());

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result)["values"];
            string pattern = @"""(.*?)""";
            List<string> terms = new List<string>();
            foreach (Match match in Regex.Matches(result.ToString(), pattern))
            {

                terms.Add(match.Groups[0].Value);
            }

            return Page();
        }
    }
}
