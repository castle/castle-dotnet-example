using System;
using System.Security.Cryptography;
using System.Text;
using Castle.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace CastleDemo.Areas.Identity.Pages.Webhook
{
    [AllowAnonymous]
    public class WebhookModel : PageModel
    {
        private readonly string _apiSecret;

        public WebhookModel(IConfiguration configuration)
        {
            _apiSecret = configuration["Castle:ApiSecret"];
        }

        public string Status { get; private set; }

        public void OnGet()
        {
        }

        // Demonstrates verifying an incoming webhook against the X-Castle-Signature
        // header. We sign a sample payload ourselves to mimic Castle's signature.
        public IActionResult OnPost()
        {
            const string body = "{\"type\":\"$review.opened\",\"data\":{}}";

            var signature = Sign(_apiSecret, body);

            try
            {
                Castle.Webhook.Verify(body, signature, _apiSecret);

                var tampered = false;
                try
                {
                    Castle.Webhook.Verify(body, "invalid-signature", _apiSecret);
                }
                catch (CastleWebhookVerificationException)
                {
                    tampered = true;
                }

                Status = tampered
                    ? "Valid signature accepted; tampered signature rejected."
                    : "Valid signature accepted.";
            }
            catch (CastleWebhookVerificationException e)
            {
                Status = $"Verification failed: {e.Message}";
            }

            return Page();
        }

        private static string Sign(string secret, string body)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? "")))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));
            }
        }
    }
}
