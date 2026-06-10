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
                Api = "Authenticate / Track",
                Event = "$login.succeeded / $login.failed",
                Blurb = "Sign in to send an Authenticate request on success and a Track request on failure.",
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
            }
        };
    }
}
