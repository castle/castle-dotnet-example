using System;
using System.Threading.Tasks;
using Castle;
using Castle.Messages.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CastleDemo.Areas.Identity.Pages.Privacy
{
    [AllowAnonymous]
    public class PrivacyModel : PageModel
    {
        private readonly CastleClient _castleClient;

        public PrivacyModel(CastleClient castleClient)
        {
            _castleClient = castleClient;
        }

        public string Status { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var request = new PrivacyRequest
            {
                Identifier = "demo@example.com",
                IdentifierType = "$email"
            };

            try
            {
                await _castleClient.RequestUserData(request);
                await _castleClient.DeleteUserData(request);

                Status = "Submitted a data request and a data deletion for demo@example.com.";
            }
            catch (Exception e)
            {
                Status = $"Request failed: {e.Message}";
            }

            return Page();
        }
    }
}
