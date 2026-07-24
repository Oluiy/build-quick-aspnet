# CLI Reference

## Invocation

```bash
BuildQuickPkg
```

```bash
BuildQuickPkg <ProjectName>   # skips the Project Name prompt
```

```bash
BuildQuickPkg --help, -h      # usage and every command, right in the terminal
BuildQuickPkg add --help      # usage for the add subcommand specifically
BuildQuickPkg --version, -v   # installed version
```

Everything else is asked interactively, in this order:

| # | Prompt | Options | Default |
| --- | --- | --- | --- |
| 1 | Project Name | free text | `MyAwesomeApi` (skipped if passed as an argument) |
| 2 | .NET Target Framework | `net8.0` / `net9.0` / `net10.0` | (must choose) |
| 3 | Architecture Pattern | 4-layer (API, Application, Domain, Infrastructure) / 3-layer (API, Application, Domain) | (must choose) |
| 4 | Deployment Style | Monolithic / Microservice | (must choose) |
| 5 | Number of services, then a name for each | *(microservice only)* free text per service | `2` services, named `Service1`, `Service2`, ... |
| 6 | Include xUnit Integration Test project | yes / no | yes |
| 7 | Port | integer | `5200` |
| 8 | HTTPS Port | integer | `5201` |
| 9 | Add Entity Framework Core | `None` / `PostgreSQL` / `SQL Server` | `None` |
| 10 | Add Dockerfile & docker-compose.yml | yes / no | no |
| 11 | Add JWT Authentication boilerplate | yes / no | no |

Use arrow keys + Enter for the multiple-choice prompts (they're rendered by [Spectre.Console](https://spectreconsole.net/)); type-and-Enter for free text and yes/no prompts.

## What each prompt controls

- **Architecture Pattern**: see the [Architecture Guide](architecture-guide.md) for exactly what each layer contains and where Entity Framework Core's `DbContext` ends up in each.
- **Deployment Style**: Monolithic generates one solution; Microservice generates one independent solution per named service plus an aggregate root `.sln`. See [Microservices](microservices.md).
- **Port / HTTPS Port**: written to `Properties/launchSettings.json`. In microservice mode, each service after the first is offset by `+10` (service 1 gets 5200/5201, service 2 gets 5210/5211, and so on) so they don't collide when run side by side.
- **Add Entity Framework Core**: see [Entity Framework Core](entity-framework-core.md).
- **Add Dockerfile & docker-compose.yml**: see [Docker](docker.md).
- **Add JWT Authentication boilerplate**: see [JWT Authentication](jwt-authentication.md).

## Adding a feature after generation

Said no to Entity Framework Core, Docker, or JWT the first time? You don't have to regenerate the project: `BuildQuickPkg add efcore|jwt|docker` retrofits any of them onto a project you already have. See [Adding a Feature Later](adding-features-later.md) for the full command reference.

## Exit codes / non-interactive use

BuildQuickPkg is fully interactive; it doesn't currently support answering all prompts via command-line flags or a config file. If you need to script project generation (e.g. in CI, or to generate many projects from a template), that's a good candidate for a feature request or contribution; see [CONTRIBUTING.md](https://github.com/Oluiy/build-quick-aspnet/blob/main/CONTRIBUTING.md).
