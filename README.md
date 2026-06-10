# Castle .NET Example

Examples of integrating Castle into .NET applications. The repository contains two samples:

| Sample | Path | Target | Notes |
| --- | --- | --- | --- |
| ASP.NET Core Razor Pages | `src/CastleDemo` | `net8.0` | The main, cross-platform sample. |
| .NET Framework console | `src/CastleDemo.Framework` | `net48` | Exercises the `System.Web` code path; builds and runs on Windows. |

## ASP.NET Core sample (`src/CastleDemo`)

A Razor Pages app based on the default template with _Individual user accounts_ for
authentication. The home page lists the available demos (see `Demos/DemoCatalog.cs`),
each of which triggers a Castle API call.

### Highlights

- Targets `net8.0` and uses minimal hosting (`Program.cs`); there is no `Startup.cs`.
- The database runs in-memory:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("CastleDemo"));
```

- Fire-and-forget Castle calls (e.g. **Track** / **Authenticate** in monitor mode) use a
  discard (`_ = client.Track(...)`) rather than an external dependency.
- Client-side fingerprinting and secure mode are wired up in `Pages/Shared/_Layout.cshtml`.

All Castle-related changes are marked with comments containing the word _Castle_ for easy
searching, and primarily affect:

- `Program.cs` — service registration
- `Areas/Identity/Pages/Account/Login.cshtml.cs` and the `Risk` / `Filter` / `Log` pages — SDK calls
- `Pages/Shared/_Layout.cshtml` — client-side Castle
- `appsettings.json` — your Castle API secret and App ID

### Run

```bash
dotnet run --project src/CastleDemo/CastleDemo.csproj
```

Set your credentials in `appsettings.json` (or via environment / user secrets):

```json
"Castle": {
  "ApiSecret": "YOUR API SECRET",
  "AppId": "YOUR APP ID"
}
```

### Docker

```bash
docker build -t castle-dotnet-example .
docker run -p 8080:8080 -e Castle__ApiSecret=YOUR_API_SECRET castle-dotnet-example
```

## .NET Framework sample (`src/CastleDemo.Framework`)

A minimal `net48` console app that adapts a `System.Web` request to
`Castle.Context.FromHttpRequest(HttpRequestBase)` and sends a Risk request (with
`DoNotTrack` enabled so it runs without a real secret). It builds and runs on Windows:

```bash
dotnet run --project src/CastleDemo.Framework/CastleDemo.Framework.csproj
```

This sample requires `Castle.Sdk` **2.4.0** or newer (the first version with a `net48` target).
