

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

namespace CastleDemo.Areas.Identity.Pages.Filter
{
    [AllowAnonymous]
    public class FilterModel : PageModel
    {

        private readonly CastleClient _castleClient;
        public FilterModel(
            CastleClient castleClient)
        {
            _castleClient = castleClient;
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
                RequestToken = "vru9P7qwelbdOiFyGPavoPdJNX9_7mTjsOtQ-2ewrl8qFLNILm_HcXtLrQwMmDNYExujcTMqu2IhWLNYZB3OA-K45FlZjLFUdBOzI0-b_jNRdt8wSjCGchs_mwtCcdczXGyTEn8_gmwFL4h8fHbdah8kkyQdK5p8am_DME5I1j5gdsdzHiyEchgpk3RgV-cRZzOTMEJ01nxsetA3RDaTH0Nt3DFOMIpqBS-daB0ph3IaLoN8eH7VPVl2nGkYKJ1vHT_2OEwwimoFL51tGyqHch0t31RKK4Q-TyyFbFwctNcri7s5Ty_WPR150MB9Xv0bZ1qTdGJxxzlHM5MVRWvWMANNmnxjW5MbWX7DNEJ8wHwdLIN8b3bBOUhrgBgaLpMqWECGAxs_wy90KuxsBz_3b28ugnEZKZ1uGzGCbBsxhG8ZKpr4Py-CcxsunG0SKINwCy-CZhsviWwbsLtcnj526q_Us48qwjAozB9YX9xS3FgPYkxeY9ezwSupCJ8ryHTojNcN44mqEupSq8zmSaXRGm1fd1wrH7OxxvJescZ35zR_d9fmkXTYGW77Pwy1F79cLR-zfhrg",
                Context = Castle.Context.FromHttpRequest(Request)
            };
        }
    }





}