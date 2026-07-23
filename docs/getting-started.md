# Getting Started

## 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later installed (`dotnet --version` should print `8.0.x` or higher)
- A terminal

That's it: no other tooling is required to generate and run a plain API. Docker and a database are only needed if you opt into those prompts.

## 2. Install

```bash
dotnet tool install --global BuildQuickPkg
```

This installs the `BuildQuickPkg` command globally, available from any directory. To upgrade later:

```bash
dotnet tool update --global BuildQuickPkg
```

## 3. Generate a project

Run it from the directory you want your new solution created in:

```bash
BuildQuickPkg
```

You'll be walked through a series of prompts (project name, target framework, architecture, ports, and a few optional add-ons). Full details on each one are in the [CLI Reference](cli-reference.md); for a first run, the defaults are fine, just press Enter through them.

You can also skip the project-name prompt by passing it as an argument:

```bash
BuildQuickPkg MyAwesomeApi
```

## 4. Run it

Once generation finishes, `cd` into the API project and run it:

```bash
cd MyAwesomeApi/src/MyAwesomeApi_API
dotnet run
```

Open the URL printed in the terminal (defaults to `http://localhost:5200`); you'll land on the Swagger UI, and `GET /api/health` will return a 200 with a healthy status. That confirms the whole stack (build, project references, launch profile) is wired up correctly.

## 5. What you just got

- A `.sln` with four projects (or three, if you picked 3-layer) already referencing each other correctly
- Swagger, CORS, and Serilog request logging configured in `Program.cs`
- `appsettings.json` / `appsettings.Development.json` / `appsettings.Production.json`, layered per environment
- An xUnit test project (if you kept the default "yes") that boots the API in-memory and asserts the health check

If you also said yes to Entity Framework Core, Docker, or JWT, see their dedicated guides:

- [Entity Framework Core](entity-framework-core.md)
- [Docker](docker.md)
- [JWT Authentication](jwt-authentication.md)

## 6. Next run

BuildQuickPkg doesn't remember your previous answers between runs; each invocation is a fresh set of prompts, so you can generate very different projects back to back (a monolith, then a 3-service microservice split, then a Docker+Postgres+JWT API) without reconfiguring anything.

## Managing your install

BuildQuickPkg is installed as a [.NET global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools), so all of this is standard `dotnet tool` plumbing, nothing BuildQuickPkg-specific.

**Check what you have installed:**

```bash
dotnet tool list --global
```

Look for the `BuildQuickPkg` row; the second column is your currently installed version.

**Upgrade to the latest version:**

```bash
dotnet tool update --global BuildQuickPkg
```

**Upgrade or downgrade to a specific version**: `dotnet tool update` takes a `--version`, and it works in both directions (it doesn't have to be newer than what you have):

```bash
dotnet tool update --global BuildQuickPkg --version 1.0.6
```

See every published version on the [NuGet package page](https://www.nuget.org/packages/BuildQuickPkg) or the [GitHub Releases page](https://github.com/Oluiy/build-quick-aspnet/releases); release notes are generated automatically for each tagged version, so that's the fastest way to see what changed before deciding whether to move.

**Uninstall:**

```bash
dotnet tool uninstall --global BuildQuickPkg
```

This only removes the CLI itself; any project you've already generated with it is a normal, independent ASP.NET Core solution and keeps working exactly as before. Uninstalling BuildQuickPkg doesn't touch anything it previously generated.
