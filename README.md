# BuildQuickPkg

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/BuildQuickPkg.svg)](https://www.nuget.org/packages/BuildQuickPkg)

An interactive .NET CLI tool that scaffolds a complete **Clean Architecture** ASP.NET Core solution: API, Application, Domain, and (optionally) Infrastructure projects, already wired up, testable, and building in seconds. Stop hand-rolling the same folder structure and `.csproj` references for every new API.

![BuildQuickPkg demo: running the CLI to generate a Clean Architecture solution with EF Core, Docker, and JWT boilerplate](docs/assets/demo.gif)

📖 **[Full documentation](docs/README.md)**: getting started, CLI reference, architecture guide, EF Core, Docker, JWT, microservices, and troubleshooting.

## What it generates

Given a project name of `MyAwesomeApi` with the 4-layer architecture and tests enabled, the tool creates:

```
MyAwesomeApi/
├── MyAwesomeApi.sln
├── .gitignore
├── README.md
├── Dockerfile                         # (optional) multi-stage build → publish → run
├── docker-compose.yml                 # (optional) api + db services
├── src/
│   ├── MyAwesomeApi_API/              # Presentation layer (Minimal API, Swagger, CORS, launch profiles)
│   │   ├── Controllers/
│   │   ├── Extensions/
│   │   ├── Middlewares/
│   │   ├── Properties/launchSettings.json
│   │   ├── appsettings.json           # shared settings (logging, JWT issuer/audience, ...)
│   │   ├── appsettings.Development.json # local connection string + dev JWT signing key
│   │   ├── appsettings.Production.json  # secrets left blank, supplied via env vars
│   │   └── Program.cs
│   ├── MyAwesomeApi_Application/      # Use cases / business logic
│   │   ├── Services/Implementation/
│   │   ├── Services/Interfaces/
│   │   └── Utilities/
│   ├── MyAwesomeApi_Domain/           # Entities, DTOs, enums, no dependencies on other layers
│   │   ├── Dtos/RequestDtos/
│   │   ├── Dtos/ResponseDtos/
│   │   ├── Entity/
│   │   └── Enums/
│   └── MyAwesomeApi_Infrastructure/   # EF Core, external services, persistence
│       ├── Context/                   # (optional) generated DbContext when EF Core is selected
│       └── Migrations/
└── tests/
    └── MyAwesomeApi_API.Tests/        # xUnit + WebApplicationFactory integration tests
        └── HealthEndpointTests.cs
```

### appsettings.json, per environment

Every generated API ships all three settings files, loaded by ASP.NET Core's standard `appsettings.json` → `appsettings.{Environment}.json` → environment variables layering:

| File | Loaded when | Contains |
| --- | --- | --- |
| `appsettings.json` | Always (base layer) | Logging defaults, `AllowedHosts`, and the JWT issuer/audience/expiry when JWT is enabled |
| `appsettings.Development.json` | `ASPNETCORE_ENVIRONMENT=Development` (default for `dotnet run`) | A working local connection string and a dev-only JWT signing key, safe to commit and never used in production |
| `appsettings.Production.json` | `ASPNETCORE_ENVIRONMENT=Production` | Connection string and JWT key left blank, meant to be supplied via environment variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`) or a secret manager |

### Optional add-ons

Three more prompts let you opt into common boilerplate at generation time:

- **Entity Framework Core** (`None` / `PostgreSQL` / `SQL Server`): adds the provider package plus `Microsoft.EntityFrameworkCore.Design` to the layer that owns `Infrastructure/Context` (the dedicated Infrastructure project in 4-layer, or Domain in 3-layer), generates a starter `{ProjectName}DbContext`, wires up `AddDbContext` in `Program.cs`, and writes matching connection strings into `appsettings.Development.json`.
- **Dockerfile & docker-compose.yml**: a multi-stage `Dockerfile` (SDK build → ASP.NET runtime) and a `docker-compose.yml` with an `api` service; when EF Core is also selected, a `db` service (Postgres or SQL Server) is included and wired up via `ConnectionStrings__DefaultConnection`.
- **JWT Authentication boilerplate**: adds `Microsoft.AspNetCore.Authentication.JwtBearer`, registers bearer-token authentication/authorization, and wires up two sample endpoints: `POST /api/auth/token` (issues a token) and `GET /api/secure` (requires one), so you can see it working immediately.

In microservice mode, each service gets its own `appsettings.*`, `DbContext`, `Dockerfile`, and `docker-compose.yml`.

Project references are pre-wired according to Clean Architecture's dependency rule: `API → Application, Infrastructure`, `Infrastructure → Application, Domain`, `Application → Domain`, and `Domain` depends on nothing. The generated API project includes Swagger/OpenAPI, CORS, and Serilog structured logging out of the box, plus a sample `/api/health` endpoint, so the solution is immediately runnable and testable.

**Two architecture options** are offered at generation time:
- **4-layer**: a dedicated Infrastructure project (shown above)
- **3-layer**: Infrastructure concerns (`Context/`, `Migrations/`) folded into `Domain/Infrastructure/` instead of a separate project, for smaller services that don't need the extra layer

Test project generation is optional; when enabled, it references the API project directly and includes a working `WebApplicationFactory<Program>`-based test for the health-check endpoint.

### Monolithic vs. microservice

By default the tool generates a single solution (as above). Choose **Microservice** instead and it will ask how many services you need and what to name each one, then generate one fully independent Clean Architecture solution per service, with the same target framework, same architecture pattern, and same package versions across all of them:

```
ShopSystem/
├── ShopSystem.sln              # aggregate solution, builds every service at once
├── .gitignore
├── README.md
└── services/
    ├── OrderService/
    │   ├── OrderService.sln    # each service is also independently buildable/runnable
    │   ├── src/OrderService_API/ ...
    │   └── tests/OrderService.UnitTests/
    ├── InventoryService/
    │   └── ...
    └── PaymentService/
        └── ...
```

Each service gets its own HTTP/HTTPS ports, offset by 10 from your chosen base port so they don't collide when run side by side.

### Structured logging (Serilog)

Every generated API ships with [Serilog](https://serilog.net/) wired up via `Serilog.AspNetCore`: a console sink, `UseSerilogRequestLogging()` for per-request timing, and the standard fatal-exception/flush-on-shutdown bootstrap pattern in `Program.cs`.

## Installation

```bash
dotnet tool install --global BuildQuickPkg
```

## Usage

```bash
BuildQuickPkg
# or, to skip the project-name prompt:
BuildQuickPkg MyAwesomeApi
```

You'll be prompted interactively for:

| Prompt | Options / Default |
| --- | --- |
| Project Name | free text, default `MyAwesomeApi` (skipped if passed as an argument) |
| Target Framework | `net8.0` / `net9.0` / `net10.0` |
| Architecture Pattern | 4-layer (with Infrastructure) / 3-layer |
| Deployment Style | Monolithic / Microservice |
| Number of services + a name for each | *(microservice only)* |
| Include xUnit test project | yes / no, default yes |
| Port | default `5200` |
| HTTPS Port | default `5201` |
| Add Entity Framework Core | `None` / `PostgreSQL` / `SQL Server` |
| Add Dockerfile & docker-compose.yml | yes / no, default no |
| Add JWT Authentication boilerplate | yes / no, default no |

Then run the generated API:

```bash
cd MyAwesomeApi/src/MyAwesomeApi_API
dotnet run
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later

## Project source layout

The tool itself follows the same separation-of-concerns principle it generates for you:

```
create-aspnet-app/
├── Program.cs                       # CLI entry point collects prompts, invokes the scaffolder
├── Scaffolding/
│   ├── ScaffoldingConfig.cs         # Options record: naming, architecture, ports, tests, EF/Docker/JWT
│   ├── EfCoreProvider.cs            # None / PostgreSql / SqlServer
│   ├── SolutionScaffolder.cs        # Orchestrates folder creation, file writes, and `dotnet sln`
│   └── ProjectStructure.cs          # Resolves layer project names and the folder tree
├── Templates/
│   ├── CsprojTemplates.cs           # .csproj content for each layer (4-layer, 3-layer, test)
│   ├── ProgramTemplate.cs           # Generated API Program.cs (+ optional EF Core / JWT wiring)
│   ├── AppSettingsTemplate.cs       # appsettings.json / .Development.json / .Production.json
│   ├── EfCoreTemplate.cs            # Generated DbContext + provider package/connection-string helpers
│   ├── DockerTemplate.cs            # Dockerfile + docker-compose.yml
│   ├── HealthEndpointTestTemplate.cs # Generated xUnit health-check test
│   ├── LaunchSettingsTemplate.cs
│   └── GitignoreTemplate.cs
└── Utilities/
    └── ProcessRunner.cs             # Wraps `dotnet` CLI process execution
```

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for how to get set up, add a new generation option, and test your change.

## License

MIT. See [LICENSE](LICENSE).
