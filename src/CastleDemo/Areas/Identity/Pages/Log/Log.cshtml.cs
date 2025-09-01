

using Castle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Castle.Messages.Requests;
using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace CastleDemo.Areas.Identity.Pages.Log
{
    [AllowAnonymous]
    public class LogModel : PageModel
    {

        private readonly CastleClient _castleClient;
        private readonly IConfiguration _configuration;
        public LogModel(
            CastleClient castleClient,
            IConfiguration configuration
            )
        {
            _castleClient = castleClient;
            _configuration = configuration;
        }

        public async Task<ActionResult> SendLog()
        {
            await _castleClient.Log(CreateCastleActionRequest("$challenge"));

            return Page();
        }

        private ActionRequest CreateCastleActionRequest(string castleEvent)
        {

            var traits = new Dictionary<string, object>();
            traits.Add("account_type", "pro123");

            var user = new Dictionary<string, object>();
            user.Add("id", "12346-123142341-42314-4214123412");
            user.Add("email", "loureiroandre47@hotmail.com");
            user.Add("name", "andre");
            user.Add("traits", traits);


            var authMethod = new Dictionary<string, string>();
            authMethod.Add("type", "$social");
            authMethod.Add("variant", "$facebook");

            var product = new Dictionary<string, string>();
            product.Add("id", "12345");

            var properties = new Dictionary<string, object>();
            properties.Add("some prop", 1234);
            properties.Add("some prop12", product);

            return new ActionRequest()
            {
                Event = castleEvent,
                Type = castleEvent,
                Status = "$requested",
                Product = product,
                CreatedAt = new DateTime(),
                User = user,
                Properties = properties,
                AuthenticationMethod = authMethod,
                RequestToken = _configuration["Castle:TestRequestToken"],
                Context = Castle.Context.FromHttpRequest(Request)
            };
        }
    }





}