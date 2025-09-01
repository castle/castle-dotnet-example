

using Castle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Castle.Messages.Requests;
using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CastleDemo.Areas.Identity.Pages.Risk
{
    [AllowAnonymous]
    public class RiskModel : PageModel
    {

        private readonly CastleClient _castleClient;
        private readonly IConfiguration _configuration;
        public RiskModel(
            CastleClient castleClient,
            IConfiguration configuration)
        {
            _castleClient = castleClient;
            _configuration = configuration;
        }

        public async Task<ActionResult> SendRisk()
        {
            try
            {
                var request = CreateCastleActionRequest("$profile_update");
                var res = await _castleClient.Risk(request);
                return Page();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return Page();
            }
        }

        private ActionRequest CreateCastleActionRequest(string castleEvent)
        {

            var traits = new Dictionary<string, object>();
            traits.Add("account_type", "pro");

            var user = new Dictionary<string, object>();
            user.Add("id", "12346-123142341-42314-4214123412");
            user.Add("email", "someuser@some-email.com");
            user.Add("name", "some random  name");
            user.Add("traits", traits);

            var product = new Dictionary<string, string>();
            product.Add("id", "12345");

            var properties = new Dictionary<string, object>();
            properties.Add("some prop", 1234);


            // Create a custom context with a public IP address
            var context = Castle.Context.FromHttpRequest(Request);
            // Override the IP address for testing purposes


            return new ActionRequest()
            {
                Event = castleEvent,
                Type = castleEvent,
                Status = "$succeeded",
                Product = product,
                User = user,
                Properties = properties,
                // https://docs.castle.io/docs/test-tokens for more info
                RequestToken = _configuration["Castle:TestRequestToken"],
                Context = context
            };
        }
    }





}