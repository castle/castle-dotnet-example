using System.Collections.Generic;

namespace CastleDemo.Demos
{
    /// <summary>
    /// A single Castle demo scenario surfaced on the home page.
    /// </summary>
    public class DemoInfo
    {
        public string Title { get; set; }

        /// <summary>The Castle API used by the demo (Risk, Filter, Log, ...).</summary>
        public string Api { get; set; }

        /// <summary>The Castle event/type the demo sends.</summary>
        public string Event { get; set; }

        public string Blurb { get; set; }

        /// <summary>Razor Page path used with <c>asp-page</c>.</summary>
        public string Page { get; set; }

        /// <summary>Razor Page area used with <c>asp-area</c> (empty for the root area).</summary>
        public string Area { get; set; }
    }

    /// <summary>
    /// Central registry of the demos shipped with this sample so they can be
    /// listed in one place and linked from the home page.
    /// </summary>
    public static class DemoCatalog
    {
        public static readonly IReadOnlyList<DemoInfo> Demos = new List<DemoInfo>
        {
            new DemoInfo
            {
                Title = "Login",
                Api = "Risk / Log",
                Event = "$login.succeeded / $login.failed",
                Blurb = "Sign in to send a Risk request on success and a Log request on failure.",
                Area = "Identity",
                Page = "/Account/Login"
            },
            new DemoInfo
            {
                Title = "Risk",
                Api = "Risk",
                Event = "$profile_update",
                Blurb = "Send a synchronous Risk request and inspect the verdict.",
                Area = "Identity",
                Page = "/Risk/Risk"
            },
            new DemoInfo
            {
                Title = "Filter",
                Api = "Filter",
                Event = "$custom",
                Blurb = "Evaluate a pre-authentication event such as a custom abuse signal.",
                Area = "Identity",
                Page = "/Filter/Filter"
            },
            new DemoInfo
            {
                Title = "Log",
                Api = "Log",
                Event = "$challenge",
                Blurb = "Record an event for monitoring without affecting a verdict.",
                Area = "Identity",
                Page = "/Log/Log"
            },
            new DemoInfo
            {
                Title = "Lists",
                Api = "Lists",
                Event = "n/a",
                Blurb = "Create a list, add an item, query it and clean up via the Lists API.",
                Area = "Identity",
                Page = "/Lists/Lists"
            },
            new DemoInfo
            {
                Title = "Privacy",
                Api = "Privacy",
                Event = "n/a",
                Blurb = "Request and delete the data Castle stores for a user.",
                Area = "Identity",
                Page = "/Privacy/Privacy"
            },
            new DemoInfo
            {
                Title = "Events",
                Api = "Events",
                Event = "n/a",
                Blurb = "Fetch the events schema and run a query against event data.",
                Area = "Identity",
                Page = "/Events/Events"
            },
            new DemoInfo
            {
                Title = "Webhooks",
                Api = "Webhook",
                Event = "n/a",
                Blurb = "Verify an incoming webhook against the X-Castle-Signature header.",
                Area = "Identity",
                Page = "/Webhook/Webhook"
            }
        };
    }
}
