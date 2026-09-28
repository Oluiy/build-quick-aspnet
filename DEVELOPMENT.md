### appsettings.json, per environment

every generated api ships all three settings files, loaded by asp.net core's standard `appsettings.json` → `appsettings.{environment}.json` → environment variables layering:

| file | loaded when | contains |
| --- | --- | --- |
| `appsettings.json` | always (base layer) | logging defaults, `allowedhosts`, and the jwt issuer/audience/expiry when jwt is enabled |
| `appsettings.development.json` | `aspnetcore_environment=development` (default for `dotnet run`) | a working local connection string and a dev-only jwt signing key, safe to commit and never used in production |
| `appsettings.production.json` | `aspnetcore_environment=production` | connection string and jwt key left blank, meant to be supplied via environment variables (`connectionstrings__defaultconnection`, `jwt__key`) or a secret manager |

### api style

choose **minimal api** (top-level `app.mapget`/`app.mappost` calls in `program.cs`) or **standard api** (controllers backed by an interface/service pair, the service-controller pattern). every generated project has `controllers/`, `services/interfaces/`, and `services/implementation/` folders either way; standard api is what actually populates them. both styles expose the same urls, so this only changes how the code is organized.

### optional add-ons

three more prompts let you opt into common boilerplate at generation time:

- **entity framework core** (`none` / `postgresql` / `sql server`): adds the provider package plus `microsoft.entityframeworkcore.design` to the layer that owns `infrastructure/context` (the dedicated infrastructure project in 4-layer, or domain in 3-layer), generates a starter `{projectname}dbcontext`, wires up `adddbcontext` in `program.cs`, and writes matching connection strings into `appsettings.development.json`.
- **dockerfile & docker-compose.yml**: a multi-stage `dockerfile` (sdk build → asp.net runtime) and a `docker-compose.yml` with an `api` service; when ef core is also selected, a `db` service (postgres or sql server) is included and wired up via `connectionstrings__defaultconnection`.
- **jwt authentication boilerplate**: adds `microsoft.aspnetcore.authentication.jwtbearer`, registers bearer-token authentication/authorization, and wires up two sample endpoints: `post /api/auth/token` (issues a token) and `get /api/secure` (requires one), so you can see it working immediately. in standard api style, these are an `authcontroller` + `iauthservice`/`authservice` instead of top-level endpoints.

in microservice mode, each service gets its own `appsettings.*`, `dbcontext`, `dockerfile`, and `docker-compose.yml`.

said no to one of these and want it later? `buildquickpkg add efcore|jwt|docker|repository|env|caddy` retrofits it onto a project you already generated, no regeneration needed. see [adding a feature later](docs/guides/adding-features-later.md).

project references are pre-wired according to clean architecture's dependency rule: `api → application, infrastructure`, `infrastructure → application, domain`, `application → domain`, and `domain` depends on nothing. the generated api project includes swagger/openapi, cors, and serilog structured logging out of the box, plus a sample `/api/health` endpoint, so the solution is immediately runnable and testable.

**two architecture options** are offered at generation time:
- **4-layer**: a dedicated infrastructure project (shown above)
- **3-layer**: infrastructure concerns (`context/`, `migrations/`) folded into `domain/infrastructure/` instead of a separate project, for smaller services that don't need the extra layer

test project generation is optional; when enabled, it references the api project directly and includes a working `webapplicationfactory<program>`-based test for the health-check endpoint.

### monolithic vs. microservice

by default the tool generates a single solution (as above). choose **microservice** instead and it will ask how many services you need and what to name each one, then generate one fully independent clean architecture solution per service, with the same target framework, same architecture pattern, and same package versions across all of them:

```
shopsystem/
├── shopsystem.sln              # aggregate solution, builds every service at once
├── .gitignore
├── readme.md
└── services/
    ├── orderservice/
    │   ├── orderservice.sln    # each service is also independently buildable/runnable
    │   ├── src/orderservice_api/ ...
    │   └── tests/orderservice.unittests/
    ├── inventoryservice/
    │   └── ...
    └── paymentservice/
        └── ...
```

each service gets its own http/https ports, offset by 10 from your chosen base port so they don't collide when run side by side.

### structured logging (serilog)

every generated api ships with [serilog](https://serilog.net/) wired up via `serilog.aspnetcore`: a console sink, `useserilogrequestlogging()` for per-request timing, and the standard fatal-exception/flush-on-shutdown bootstrap pattern in `program.cs`.

## project source layout

the tool itself follows the same separation-of-concerns principle it generates for you:

```
buildquickpkg/
├── program.cs                       # cli entry point: routes to `add`, or runs the generation prompts
├── commands/                        # `buildquickpkg add <feature>`: retrofits a feature onto an existing project
│   ├── addfeaturecommand.cs         # parses "efcore/jwt/docker/repository/env/caddy" and dispatches to the commands below
│   ├── addefcorecommand.cs
│   ├── addjwtcommand.cs
│   ├── adddockercommand.cs
│   ├── addrepositorycommand.cs      # generic repository/unitofwork (requires efcore first)
│   ├── addenvcommand.cs             # .env with dummy values matching the project's actual setup
│   ├── addcaddycommand.cs           # caddyfile reverse proxy
│   └── helptext.cs                  # --help / -h output for the root command and `add`
├── scaffolding/
│   ├── scaffoldingconfig.cs         # options record: naming, architecture, api style, ports, tests, ef/docker/jwt
│   ├── efcoreprovider.cs            # none / postgresql / sqlserver
│   ├── apistyle.cs                  # minimal / controller
│   ├── solutionscaffolder.cs        # orchestrates folder creation, file writes, and `dotnet sln`
│   ├── projectstructure.cs          # resolves layer project names and the folder tree (new projects)
│   ├── existingproject.cs           # describes an already-generated project, resolved from disk
│   └── existingprojectlocator.cs    # locates existingproject from the current working directory
├── templates/
│   ├── csprojtemplates.cs           # .csproj content for each layer (4-layer, 3-layer, test)
│   ├── programtemplate.cs           # generated api program.cs (+ optional ef core / jwt / controller wiring)
│   ├── controllertemplate.cs        # health/auth controller + service pairs for standard api style
│   ├── repositorytemplate.cs        # generic irepository<t>/repository<t> + iunitofwork/unitofwork
│   ├── appsettingstemplate.cs       # appsettings.json / .development.json / .production.json
│   ├── efcoretemplate.cs            # generated dbcontext + provider package/connection-string helpers
│   ├── dockertemplate.cs            # dockerfile + docker-compose.yml
│   ├── envtemplate.cs               # .env content matching the project's actual setup
│   ├── caddytemplate.cs             # caddyfile reverse proxy
│   ├── healthendpointtesttemplate.cs # generated xunit health-check test
│   ├── launchsettingstemplate.cs
│   └── gitignoretemplate.cs
└── utilities/
    ├── processrunner.cs             # wraps `dotnet` cli process execution
    ├── csprojeditor.cs              # adds packagereferences to an existing .csproj (used by `add`)
    ├── programcseditor.cs           # patches an existing program.cs at its stable markers (used by `add`)
    ├── appsettingseditor.cs         # merges json sections into an existing appsettings*.json (used by `add`)
    ├── projectfeaturedetector.cs    # reads an existing project's actual ef core/jwt/api style/port (used by `add`)
    └── namevalidation.cs            # validates a project/service name is safe as a c# namespace + folder name
```