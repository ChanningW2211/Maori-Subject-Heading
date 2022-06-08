﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;


namespace web;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    public static List<string> result;

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
        uri.Append("test?query=");
        string query = string.Format(
            @"SELECT distinct ?s {{
            optional {{ ?s <http://www.w3.org/2008/05/skos#prefLabel> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
            optional {{ ?s <http://www.w3.org/2008/05/skos#usedFor> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}
            optional {{ ?s <http://www.w3.org/2008/05/skos#altLabel> ?o. FILTER (contains(lcase(str(?o)),'{0}')) }}}}", searchString?.ToLower());
        uri.Append(HttpUtility.UrlEncode(query));
        HttpResponseMessage response = await client.GetAsync(uri.ToString());

        string result = JObject.Parse(response.Content.ReadAsStringAsync().Result)["values"].ToString();
        string pattern = @"""(.*?)""";
        List<string> uris = new List<string>();
        foreach (Match match in Regex.Matches(result, pattern))
        {
            uris.Add(match.Groups[1].Value);
        }

        IndexModel.result = uris;
        if (uris.Count() == 0) return Redirect("Error");
        return RedirectToPage ("Result");
    }
}