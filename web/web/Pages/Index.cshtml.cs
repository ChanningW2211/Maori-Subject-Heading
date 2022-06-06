using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using System.Text;

namespace web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async void OnGet()
    {
        HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");
        StringBuilder uri = new StringBuilder();
        uri.Append("test?query=");
        uri.Append(HttpUtility.UrlEncode(@"SELECT distinct ?s { 
  optional {?s skos:prefLabel 'Spirituality'.}
        optional {?s skos:usedFor 'Spirituality'. }
        optional {?s skos:altLabel 'Spirituality'. }
    }"));
        HttpResponseMessage response = await client.GetAsync(uri.ToString());
        string result = response.Content.ReadAsStringAsync().Result;

    }
}