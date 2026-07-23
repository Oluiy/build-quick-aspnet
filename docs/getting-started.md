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
