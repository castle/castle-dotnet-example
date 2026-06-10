using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using Castle;
using Castle.Config;
using Castle.Messages.Requests;

namespace CastleDemo.Framework
{
    /// <summary>
    /// Minimal .NET Framework 4.8 sample. It exercises the System.Web code path of the
    /// SDK — <see cref="Context.FromHttpRequest(HttpRequestBase)"/> — which only builds and
    /// runs on Windows. The Castle call uses DoNotTrack so the sample runs without a
    /// real API secret or network access.
    /// </summary>
    internal static class Program
    {
        private static async Task Main()
        {
            var apiSecret = Environment.GetEnvironmentVariable("CASTLE_API_SECRET") ?? "YOUR API SECRET";

            var client = new CastleClient(new CastleConfiguration(apiSecret)
            {
                DoNotTrack = true
            });

            var headers = new NameValueCollection
            {
                ["X-Castle-Client-ID"] = "the-client-id-from-castle-js",
                ["X-Forwarded-For"] = "1.2.3.4"
            };

            // Adapt a System.Web request to the SDK's .NET Framework context overload.
            HttpRequestBase request = new DemoHttpRequest(headers, new HttpCookieCollection(), "1.2.3.4");
            var context = Context.FromHttpRequest(request);

            var actionRequest = new ActionRequest
            {
                Event = "$login.succeeded",
                UserId = "user-123",
                UserTraits = new Dictionary<string, string>
                {
                    ["email"] = "user@example.com"
                },
                Context = context
            };

            var verdict = await client.Risk(actionRequest);

            Console.WriteLine("Castle .NET Framework sample");
            Console.WriteLine($"  Resolved client id: {context.ClientId}");
            Console.WriteLine($"  Resolved ip:        {context.Ip}");
            Console.WriteLine($"  Verdict action:     {verdict?.Action}");
            Console.WriteLine($"  Failover:           {verdict?.Failover}");
        }
    }
}
