using System.Collections.Generic;

namespace CastleDemo.Demos
{
    /// <summary>A single workflow surfaced in the nav and on the home page.</summary>
    public class DemoInfo
    {
        /// <summary>URL slug, e.g. <c>signup</c> -&gt; <c>/signup</c>.</summary>
        public string Slug { get; set; }

        public string FriendlyName { get; set; }

        public string Blurb { get; set; }
    }

    /// <summary>
    /// Central registry of the workflows shipped with this sample so they can be
    /// listed in one place (nav + home grid).
    /// </summary>
    public static class DemoCatalog
    {
        public static readonly IReadOnlyList<DemoInfo> Demos = new List<DemoInfo>
        {
            new DemoInfo
            {
                Slug = "signup",
                FriendlyName = "sign up",
                Blurb = "Filter a registration ($registration) before the account exists."
            },
            new DemoInfo
            {
                Slug = "login",
                FriendlyName = "login",
                Blurb = "Filter the attempt, then assess a successful login with Risk."
            },
            new DemoInfo
            {
                Slug = "account",
                FriendlyName = "account",
                Blurb = "Update your profile, send a custom event, and log out."
            },
            new DemoInfo
            {
                Slug = "password_reset",
                FriendlyName = "password reset",
                Blurb = "Record a password-reset event with the non-blocking log endpoint."
            },
            new DemoInfo
            {
                Slug = "lists",
                FriendlyName = "lists",
                Blurb = "Create and fetch lists with the Lists API."
            },
            new DemoInfo
            {
                Slug = "privacy",
                FriendlyName = "privacy",
                Blurb = "Request or delete a user's data with the Privacy API."
            },
            new DemoInfo
            {
                Slug = "webhooks",
                FriendlyName = "webhooks",
                Blurb = "Verify and inspect incoming Castle webhooks."
            }
        };
    }
}
