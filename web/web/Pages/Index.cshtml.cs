using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;

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

    public void OnGet() { }

    [BindProperty]
    public string? Term { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        HttpClient client = _httpClientFactory.CreateClient("AllegroGraph");
        StringBuilder uri = new StringBuilder();
        uri.Append("test?query=");
        uri.Append(HttpUtility.UrlEncode(string.Format(
            @"SELECT distinct ?s {{
            optional {{?s skos:prefLabel ""{0}"".}}
            optional {{?s skos:usedFor ""{0}"". }}
            optional {{?s skos:altLabel ""{0}"". }}}}", Term)));
        HttpResponseMessage response = await client.GetAsync(uri.ToString());
        string result = response.Content.ReadAsStringAsync().Result;
        return Page();
    }
}