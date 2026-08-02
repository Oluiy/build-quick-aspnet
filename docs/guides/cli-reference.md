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
| 4 | API Style | Minimal API (top-level Map* endpoints) / Standard API (Controllers + Services) | (must choose) |
| 5 | Deployment Style | Monolithic / Microservice | (must choose) |
| 6 | Number of services, then a name for each | *(microservice only)* free text per service | `2` services, named `Service1`, `Service2`, ... |
| 7 | Include xUnit Integration Test project | yes / no | yes |
| 8 | Port | integer | `5200` |
| 9 | HTTPS Port | integer | `5201` |
| 10 | Add Entity Framework Core | `None` / `PostgreSQL` / `SQL Server` | `None` |
| 11 | Add Dockerfile & docker-compose.yml | yes / no | no |
| 12 | Add JWT Authentication boilerplate | yes / no | no |

Use arrow keys + Enter for the multiple-choice prompts (they're rendered by [Spectre.Console](https://spectreconsole.net/)); type-and-Enter for free text and yes/no prompts.

## What each prompt controls

- **Architecture Pattern**: see the [Architecture Guide](guides/architecture-guide.md) for exactly what each layer contains and where Entity Framework Core's `DbContext` ends up in each.
- **API Style**: Minimal API keeps every sample endpoint as a top-level `app.MapGet`/`app.MapPost` call in `Program.cs`. Standard API generates a Controller backed by an interface/service pair instead (the `Controllers/` and `Services/Interfaces`+`Services/Implementation` folders exist in every generated project either way; Standard API is what actually populates them). Both styles expose the exact same URLs, so this only changes how the code is organized, not what it does.
- **Deployment Style**: Monolithic generates one solution; Microservice generates one independent solution per named service plus an aggregate root `.sln`. See [Microservices](guides/microservices.md).
- **Port / HTTPS Port**: written to `Properties/launchSettings.json`. In microservice mode, each service after the first is offset by `+10` (service 1 gets 5200/5201, service 2 gets 5210/5211, and so on) so they don't collide when run side by side.
- **Add Entity Framework Core**: see [Entity Framework Core](guides/entity-framework-core.md).
- **Add Dockerfile & docker-compose.yml**: see [Docker](guides/docker.md).
- **Add JWT Authentication boilerplate**: see [JWT Authentication](guides/jwt-authentication.md).

## Adding a feature after generation

Said no to Entity Framework Core, Docker, or JWT the first time? You don't have to regenerate the project: `BuildQuickPkg add efcore|jwt|docker|repository|env|caddy` retrofits any of them onto a project you already have. See [Adding a Feature Later](guides/adding-features-later.md) for the full command reference.

## Exit codes / non-interactive use

BuildQuickPkg is fully interactive; it doesn't currently support answering all prompts via command-line flags or a config file. If you need to script project generation (e.g. in CI, or to generate many projects from a template), that's a good candidate for a feature request or contribution; see [CONTRIBUTING.md](https://github.com/Oluiy/build-quick-aspnet/blob/main/CONTRIBUTING.md).
