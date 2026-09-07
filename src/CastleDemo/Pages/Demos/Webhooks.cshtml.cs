using System.Collections.Generic;
using CastleDemo.Demos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CastleDemo.Pages.Demos
{
    public class WebhooksModel : PageModel
    {
        private readonly WebhookStore _store;

        public WebhooksModel(WebhookStore store)
        {
            _store = store;
        }

        public string WebhookEndpoint { get; private set; }

        public IReadOnlyList<ReceivedWebhook> Received { get; private set; }

        public void OnGet()
        {
            WebhookEndpoint = $"{Request.Scheme}://{Request.Host}/webhooks/castle";
            Received = _store.All();
        }

        public static string Pretty(ReceivedWebhook webhook)
        {
            return webhook.Body == null
                ? "null"
                : webhook.Body.ToString(Formatting.Indented);
        }
    }
}
