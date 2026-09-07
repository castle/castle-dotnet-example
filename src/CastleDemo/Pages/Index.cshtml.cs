using System.Collections.Generic;
using CastleDemo.Demos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CastleDemo.Pages
{
    public class IndexModel : PageModel
    {
        public IReadOnlyList<DemoInfo> Demos => DemoCatalog.Demos;

        public void OnGet()
        {
        }
    }
}
