# Castle demo application: .NET

This project demonstrates key Castle workflows in an ASP.NET Core app built on
the [Castle .NET SDK](https://github.com/castle/castle-dotnet) (3.0).

The repository contains two samples:

| Sample | Path | Target | Notes |
| --- | --- | --- | --- |
| ASP.NET Core | `src/CastleDemo` | `net8.0` | The main, cross-platform sample (described below). |
| .NET Framework console | `src/CastleDemo.Framework` | `net48` | Exercises the `System.Web` code path; builds and runs on Windows. |

## What's demonstrated

The app walks through a full user lifecycle. Every action mints a fresh Castle
request token in the browser (`Castle.createRequestToken()`) and forwards it to
the backend, which calls Castle and acts on the verdict. Each page shows the
exact payload sent to Castle and the verdict returned.

- **sign up** – `$registration` to `filter` (anonymous, so the email goes in `params`): `$attempted` for a new email, `$failed` (resolved via `matching_user_id`) for an email that already exists
- **login** – `$login` reusing one request token across two calls: `filter` `$attempted` first, then `risk` `$succeeded` on success or `filter` `$failed` (wrong password / unknown user)
- **account** – post-login actions: profile update (`$profile_update` to `risk`), a custom event (`Castle.custom()`), and logout (`$logout` via the non-blocking `log` endpoint)
- **password reset** – `$password_reset` via the non-blocking `log` endpoint
- **lists** – the Lists API (`CreateList`, `GetAllLists`)
- **privacy** – the Privacy API (`RequestUserData`, `DeleteUserData`)
- **webhooks** – incoming Castle webhooks are signature-verified with `Castle.Webhook.Verify` (against the `X-Castle-Signature` header) and the most recent payloads are listed

All Castle-related changes are kept in `Program.cs` (service registration and
the JSON endpoints), `Demos/` (the flow logic and the simulated user fixture)
and `Pages/Shared/_Layout.cshtml` (client-side Castle).

## Prerequisites

You'll need a Castle account. If you don't have one, start a free trial at
https://castle.io. For local development, use a **sandbox** environment so demo
traffic from `localhost` stays separate from production data — from the Castle
dashboard (Settings → API) grab the sandbox keys:

- your **publishable key** (`Castle:Pk`) – used by the browser SDK
- your **API secret** (`Castle:ApiSecret`) – used by the backend SDK

These are the only two values you need to configure.

## Running locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) and Node.js
(for the Castle browser SDK).

The Castle browser SDK is served at runtime from `node_modules`, so install the
npm dependency first:

```bash
cd src/CastleDemo
npm install
```

Provide your Castle credentials. Either via user secrets:

```bash
dotnet user-secrets set "Castle:Pk" "YOUR_PUBLISHABLE_KEY"
dotnet user-secrets set "Castle:ApiSecret" "YOUR_API_SECRET"
```

…or via environment variables (`Castle__Pk` / `Castle__ApiSecret`, or the
`castle_pk` / `castle_api_secret` names used by the other Castle examples).

Run the app:

```bash
dotnet run --project src/CastleDemo/CastleDemo.csproj
```

> **Note:** Castle.Sdk 3.0.0 is not yet on NuGet. Until it is published, build
> the SDK from `develop` and consume it locally with
> `./scripts/set-sdk-version.sh develop`.

## Running with Docker

The bundled `Dockerfile` installs the browser SDK, publishes the app and serves
it on port 8080.

```bash
docker build -t castle-dotnet-example .

docker run -d -p 8080:8080 \
  -e castle_pk=YOUR_PUBLISHABLE_KEY \
  -e castle_api_secret=YOUR_API_SECRET \
  castle-dotnet-example
```

The app will be available at http://127.0.0.1:8080. Point it at a Castle sandbox
environment when running locally.

## .NET Framework sample (`src/CastleDemo.Framework`)

A minimal `net48` console app that adapts a `System.Web` request to
`Castle.Context.FromHttpRequest(HttpRequestBase)` and sends a Risk request (with
`DoNotTrack` enabled so it runs without a real secret). It builds and runs on
Windows:

```bash
dotnet run --project src/CastleDemo.Framework/CastleDemo.Framework.csproj
```

This sample requires `Castle.Sdk` **3.0.0** or newer (the first version with a
`net48` target).

## Disclaimer

We're sharing this sample app in the hope that other developers find it
valuable. Although it is not an officially supported sample, we welcome
questions and suggestions at `support@castle.io`.
