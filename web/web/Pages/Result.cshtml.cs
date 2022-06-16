using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace web
{
	public class ResultModel : PageModel
    {
        public void OnGet(string key)
        {
            ViewData["result"] = Helper.result;
            ViewData["key"] = key;
        }
    }
}
