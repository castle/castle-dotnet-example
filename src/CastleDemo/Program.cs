using System.IO;
using Castle;
using Castle.Config;
using Castle.Infrastructure.Exceptions;
using CastleDemo.Demos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Castle credentials. Only the publishable key (used by the browser SDK) and
// the API secret (used by the server SDK) need to be configured; the simulated
// demo user values are baked into DemoConfig. Both Castle:* settings and the
// castle_pk / castle_api_secret environment variables are honoured.
var demoConfig = new DemoConfig
{
    Pk = builder.Configuration["Castle:Pk"] ?? builder.Configuration["castle_pk"] ?? "",
    ApiSecret = builder.Configuration["Castle:ApiSecret"] ?? builder.Configuration["castle_api_secret"] ?? ""
};

builder.Services.AddSingleton(demoConfig);

// The SDK requires a non-empty secret; fall back to a placeholder so the app
// still boots when none is configured (the API calls then surface an error).
var castleConfiguration = new CastleConfiguration(
    string.IsNullOrEmpty(demoConfig.ApiSecret) ? "no-secret-configured" : demoConfig.ApiSecret)
{
    Timeout = 5000
};

builder.Services.AddSingleton(new CastleClient(castleConfiguration));
builder.Services.AddSingleton<CastleFlows>();
builder.Services.AddSingleton<WebhookStore>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseStaticFiles();

// Serve the Castle browser SDK straight from the npm install (node_modules)
// instead of vendoring it into the repo.
var castleJsDist = Path.Combine(app.Environment.ContentRootPath, "node_modules", "@castleio", "castle-js", "dist");
if (Directory.Exists(castleJsDist))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(castleJsDist),
        RequestPath = "/vendor/castle-js"
    });
}

app.UseRouting();

app.MapRazorPages();

// 2.x ships castle.browser.js; 3.x ships castle.umd.js. The HTML always requests castle.umd.js.
app.MapGet("/vendor/castle-js/{filename}", (string filename) =>
{
    if (!Directory.Exists(castleJsDist))
    {
        return Results.NotFound();
    }

    var path = ResolveCastleJs(castleJsDist, filename);
    return path is null ? Results.NotFound() : Results.File(path, "application/javascript");
});

// --- JSON evaluation endpoints (called by the browser demo pages) ----------

app.MapPost("/evaluate_signup", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.EvaluateSignup(Str(input, "email"), Str(input, "request_token"), req);
    return Json(result);
});

app.MapPost("/evaluate_login", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.EvaluateLogin(Str(input, "email"), Str(input, "password"), Str(input, "request_token"), req);
    return Json(result);
});

app.MapPost("/evaluate_profile_update", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.EvaluateProfileUpdate(Str(input, "name"), Str(input, "email"), Str(input, "request_token"), req);
    return Json(result);
});

app.MapPost("/evaluate_logout", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.EvaluateLogout(Str(input, "request_token"), req);
    return Json(result);
});

app.MapPost("/evaluate_new_password", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.EvaluatePasswordReset(Str(input, "password"), Str(input, "request_token"), req);
    return Json(result);
});

app.MapPost("/create_list", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.CreateListDemo(Str(input, "name"), Str(input, "color"), Str(input, "primary_field"));
    return Json(result);
});

app.MapPost("/privacy_user_data", async (HttpRequest req, CastleFlows flows) =>
{
    var input = await ReadJson(req);
    var result = await flows.PrivacyUserData(Str(input, "action"), Str(input, "identifier"), Str(input, "identifier_type"));
    return Json(result);
});

// Incoming Castle webhook receiver. Verifies the signature before storing the
// payload; anything that fails verification gets a 404 so the endpoint isn't
// revealed to unauthenticated callers.
app.MapPost("/webhooks/castle", async (HttpRequest req, DemoConfig cfg, WebhookStore store) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    var signature = req.Headers["X-Castle-Signature"].ToString();

    if (string.IsNullOrEmpty(cfg.ApiSecret))
    {
        return Results.StatusCode(404);
    }

    try
    {
        Castle.Webhook.Verify(body, signature, cfg.ApiSecret);
    }
    catch (CastleWebhookVerificationException)
    {
        return Results.StatusCode(404);
    }

    JToken parsed;
    try
    {
        parsed = JToken.Parse(body);
    }
    catch (JsonException)
    {
        parsed = JValue.CreateString(body);
    }

    store.Add(parsed);
    return Results.StatusCode(204);
});

app.Run();

static async Task<JObject> ReadJson(HttpRequest req)
{
    using var reader = new StreamReader(req.Body);
    var raw = await reader.ReadToEndAsync();
    if (string.IsNullOrWhiteSpace(raw))
    {
        return new JObject();
    }

    try
    {
        return JObject.Parse(raw);
    }
    catch (JsonException)
    {
        return new JObject();
    }
}

static string Str(JObject input, string key)
{
    return input.TryGetValue(key, out var token) ? token.ToString() : "";
}

static IResult Json(JObject body)
{
    return Results.Text(body.ToString(Formatting.None), "application/json");
}

static string? ResolveCastleJs(string dist, string filename)
{
    var names = filename switch
    {
        "castle.umd.js" => new[] { "castle.umd.js", "castle.browser.js" },
        "castle.browser.js" => new[] { "castle.browser.js", "castle.umd.js" },
        _ => new[] { filename }
    };
    var root = Path.GetFullPath(dist);
    foreach (var name in names)
    {
        var candidate = Path.GetFullPath(Path.Combine(root, name));
        if (!candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            continue;
        }

        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    return null;
}
