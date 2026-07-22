# create-aspnet-app

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/BuildQuickPkg.svg)](https://www.nuget.org/packages/BuildQuickPkg)

An interactive .NET CLI tool that scaffolds a complete **Clean Architecture** ASP.NET Core solution — API, Application, Domain, and (optionally) Infrastructure projects, already wired up, testable, and building — in seconds. Stop hand-rolling the same folder structure and `.csproj` references for every new API.

## What it generates

Given a project name of `MyAwesomeApi` with the 4-layer architecture and tests enabled, the tool creates:

```
MyAwesomeApi/
├── MyAwesomeApi.sln
├── .gitignore
├── README.md
├── src/
│   ├── MyAwesomeApi_API/              # Presentation layer (Minimal API, Swagger, CORS, launch profiles)
│   │   ├── Controllers/
│   │   ├── Extensions/
│   │   ├── Middlewares/
│   │   ├── Properties/launchSettings.json
│   │   └── Program.cs
│   ├── MyAwesomeApi_Application/      # Use cases / business logic
│   │   ├── Services/Implementation/
│   │   ├── Services/Interfaces/
│   │   └── Utilities/
│   ├── MyAwesomeApi_Domain/           # Entities, DTOs, enums — no dependencies on other layers
│   │   ├── Dtos/RequestDtos/
│   │   ├── Dtos/ResponseDtos/
│   │   ├── Entity/
│   │   └── Enums/
│   └── MyAwesomeApi_Infrastructure/   # EF Core, external services, persistence
│       ├── Context/
│       └── Migrations/
└── tests/
    └── MyAwesomeApi_API.Tests/        # xUnit + WebApplicationFactory integration tests
        └── HealthEndpointTests.cs
```

Project references are pre-wired according to Clean Architecture's dependency rule: `API → Application, Infrastructure`, `Infrastructure → Application, Domain`, `Application → Domain`, and `Domain` depends on nothing. The generated API project includes Swagger/OpenAPI and CORS out of the box, plus a sample `/api/health` endpoint — the solution is immediately runnable and testable.

**Two architecture options** are offered at generation time:
- **4-layer** — a dedicated Infrastructure project (shown above)
- **3-layer** — Infrastructure concerns (`Context/`, `Migrations/`) folded into `Domain/Infrastructure/` instead of a separate project, for smaller services that don't need the extra layer

Test project generation is optional; when enabled, it references the API project directly and includes a working `WebApplicationFactory<Program>`-based test for the health-check endpoint.

## Installation

```bash
dotnet tool install --global BuildQuickPkg
```

## Usage

```bash
create-aspnet-app
```

You'll be prompted interactively for:

| Prompt | Options / Default |
| --- | --- |
| Project Name | free text, default `MyAwesomeApi` |
| Target Framework | `net8.0` / `net9.0` |
| Architecture Pattern | 4-layer (with Infrastructure) / 3-layer |
| Include xUnit test project | yes / no, default yes |
| Port | default `5200` |
| HTTPS Port | default `5201` |

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
├── Program.cs                       # CLI entry point — collects prompts, invokes the scaffolder
├── Scaffolding/
│   ├── ScaffoldingConfig.cs         # Options record: naming, architecture, ports, tests
│   ├── SolutionScaffolder.cs        # Orchestrates folder creation, file writes, and `dotnet sln`
│   └── ProjectStructure.cs          # Resolves layer project names and the folder tree
├── Templates/
│   ├── CsprojTemplates.cs           # .csproj content for each layer (4-layer, 3-layer, test)
│   ├── ProgramTemplate.cs           # Generated API Program.cs
│   ├── HealthEndpointTestTemplate.cs # Generated xUnit health-check test
│   ├── LaunchSettingsTemplate.cs
│   └── GitignoreTemplate.cs
└── Utilities/
    └── ProcessRunner.cs             # Wraps `dotnet` CLI process execution
```

## Contributing

Issues and pull requests are welcome. If you're adding a new template or generation option, keep it in the matching `Templates/` file (or add a new one) rather than growing `SolutionScaffolder.cs` — it should stay a thin orchestrator.

## License

MIT — see [LICENSE](LICENSE).
