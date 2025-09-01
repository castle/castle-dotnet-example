

using Castle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Castle.Messages.Requests;
using System.Collections.Generic;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;
using System;
using Castle.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;

namespace CastleDemo.Areas.Identity.Pages.Filter
{
    [AllowAnonymous]
    public class FilterModel : PageModel
    {

        private readonly _configuration;
        private readonly CastleClient _castleClient;
        public FilterModel(
            CastleClient castleClient, IConfiguration configuration
            )
        {

            _castleClient = castleClient;
            _configuration = configuration;
        }

        private string SubmitBtn { get; set; }


        public async Task<IActionResult> SendFilter()
        {
            try
            {
                var res = await _castleClient.Filter(CreateCastleActionRequest("$custom"));

                if (res.Failover)
                {
                    //  Allow attempt. Data missing or invalid. See FailoverReason
                }

            }
            catch (Exception e)
            {
                if (e is CastleInvalidTokenException)
                {
                    // Deny attempt. Likely a bad actor bypassing fingerprinting
                }

            }
            return null;

        }
        private ActionRequest CreateCastleActionRequest(string castleEvent)
        {
            var user = new Dictionary<string, object>();
            user.Add("id", "12346-123142341-42314-4214123412");
            user.Add("email", "someuser123@some-email.com");
            user.Add("name", "Some filter");

            var properties = new Dictionary<string, object>();
            properties.Add("some prop", 1234);

            var changeset = new Dictionary<string, object>();
            changeset.Add("password", new Dictionary<string, object>() {
                {"changed", true }
            });
            changeset.Add("email", new Dictionary<string, object>() {
                {"from", "original.email@example.com" }
            });
            changeset.Add("authentication_method.type", new Dictionary<string, object>() {
                {"from", null },
                {"to", "$authenticator" }
            });

            return new ActionRequest()
            {
                Type = castleEvent,
                Status = "$attempted",
                Name = "custom event",
                User = user,
                Changeset = changeset,
                Properties = properties,
                RequestToken = _configuration["Castle:TestRequestToken"],
                Context = Castle.Context.FromHttpRequest(Request)
            };
        }
    }





}