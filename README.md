# buildquickpkg

[![license: mit](https://img.shields.io/badge/license-mit-yellow.svg)](LICENSE)
[![nuget](https://img.shields.io/nuget/v/buildquickpkg.svg)](https://www.nuget.org/packages/buildquickpkg)
[![downloads](https://img.shields.io/nuget/dt/buildquickpkg.svg)](https://www.nuget.org/packages/buildquickpkg)
[![github](https://img.shields.io/github/stars/oluiy/build-quick-aspnet.svg?style=social)](https://github.com/oluiy/build-quick-aspnet)

an interactive .net cli tool that scaffolds a complete **clean architecture** asp.net core solution: api, application, domain, and (optionally) infrastructure projects, already wired up, testable, and building in seconds. stop hand-rolling the same folder structure and `.csproj` references for every new api.

![buildquickpkg demo: running the cli to generate a clean architecture solution with ef core, docker, and jwt boilerplate](https://raw.githubusercontent.com/Oluiy/build-quick-aspnet/main/docs/assets/demo.gif)

📖 **[full documentation](https://oluiy.github.io/build-quick-aspnet/)** (or [browse in-repo](docs/guides/readme.md)): getting started, cli reference, architecture guide, ef core, docker, jwt, microservices, and troubleshooting.

## what it generates

given a project name of `myawesomeapi` with the 4-layer architecture and tests enabled, the tool creates:

```
myawesomeapi/
├── myawesomeapi.sln
├── .gitignore
├── readme.md
├── dockerfile                         # (optional) multi-stage build → publish → run
├── docker-compose.yml                 # (optional) api + db services
├── src/
│   ├── myawesomeapi_api/              # presentation layer (minimal api, swagger, cors, launch profiles)
│   │   ├── controllers/
│   │   ├── extensions/
│   │   ├── middlewares/
│   │   ├── properties/launchsettings.json
│   │   ├── appsettings.json           # shared settings (logging, jwt issuer/audience, ...)
│   │   ├── appsettings.development.json # local connection string + dev jwt signing key
│   │   ├── appsettings.production.json  # secrets left blank, supplied via env vars
│   │   └── program.cs
│   ├── myawesomeapi_application/      # use cases / business logic
│   │   ├── services/implementation/
│   │   ├── services/interfaces/
│   │   └── utilities/
│   ├── myawesomeapi_domain/           # entities, dtos, enums, no dependencies on other layers
│   │   ├── dtos/requestdtos/
│   │   ├── dtos/responsedtos/
│   │   ├── entity/
│   │   └── enums/
│   └── myawesomeapi_infrastructure/   # ef core, external services, persistence
│       ├── context/                   # (optional) generated dbcontext when ef core is selected
│       └── migrations/
└── tests/
    └── myawesomeapi_api.tests/        # xunit + webapplicationfactory integration tests
        └── healthendpointtests.cs
```


## installation

```bash
dotnet tool install --global buildquickpkg
```

upgrading, downgrading to a specific version, and uninstalling are covered in [managing your install](docs/guides/getting-started.md#managing-your-install).

## usage

```bash
buildquickpkg
# or, to skip the project-name prompt:
buildquickpkg myawesomeapi
```

you'll be prompted interactively for:

| prompt | options / default |
| --- | --- |
| project name | free text, default `myawesomeapi` (skipped if passed as an argument) |
| target framework | `net8.0` / `net9.0` / `net10.0` |
| architecture pattern | 4-layer (with infrastructure) / 3-layer |
| [api style](DEVELOPMENT.md##api-style) | minimal api / standard api (controllers + services) |
| [deployment style](DEVELOPMENT.md##monolithic-vs.-microservice) | monolithic / microservice |
| number of services + a name for each | *(microservice only)* |
| include xunit test project | yes / no, default yes |
| port | default `5200` |
| https port | default `5201` |
| add entity framework core | `none` / `postgresql` / `sql server` |
| add dockerfile & docker-compose.yml | yes / no, default no |
| add jwt authentication boilerplate | yes / no, default no |

then run the generated api:

```bash
cd myawesomeapi/src/myawesomeapi_api
dotnet run
```

### clear explanation of how to use the tool
Visit the [DEVELOPMENT.md](DEVELOPMENT.md) file for a full explanation of how to use the tool. What changed in each release: [CHANGELOG.md](CHANGELOG.md).

### adding a new type to any c# project

```bash
cd src/myawesomeapi_application/services
buildquickpkg new interface iorderservice      # class | interface | struct | enum | record
```

creates `iorderservice.cs` with the namespace already filled in (`myawesomeapi_application.services`), worked out from the nearest `.csproj` and the folders below it. the namespace style matches what the project already uses; force one with `--file-scoped` or `--block`. works in any c# project, not just generated ones, and never overwrites an existing file.

## requirements

- [.net 8 sdk](https://dotnet.microsoft.com/download) or later


## contributing

issues and pull requests are welcome. see [CONTRIBUTING.md](CONTRIBUTING.md) for how to get set up, add a new generation option, and test your change. to find your way around the code first, read [how the tool's own code is organized](DEVELOPMENT.md#project-source-layout).

## license

see [LICENSE](LICENSE).
