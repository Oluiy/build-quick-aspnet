# Changelog

Every notable change to BuildQuickPkg, newest first. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow
[Semantic Versioning](https://semver.org/). Each version links to its tag on GitHub.

## [Unreleased]

### Fixed
- **Documentation links now work both on the docs site and on GitHub.** Links
  between guides used a `guides/…` prefix that only worked on the site; on GitHub
  they pointed at a folder that doesn't exist. Two links were also broken *on the
  site* (Entity Framework Core → Docker, FAQ → Entity Framework Core). All 30
  links now resolve in both places (Docsify `relativePath: true`), and the site's
  home page moved to `#/guides/` so its own links resolve correctly; the old root
  URL redirects there.

## [1.1.1] - 2026-09-28

### Added
- **`BuildQuickPkg new`** creates a C# file with its namespace already filled in,
  instead of an empty file you complete by hand:
  ```bash
  BuildQuickPkg new <class|interface|struct|enum|record> <Name> [--file-scoped | --block]
  ```
  - The namespace comes from the nearest `.csproj` (its `<RootNamespace>`, or the
    project file's name) plus each folder down to where you are - the same rule
    Visual Studio and Rider use. Folder names that aren't valid identifiers are
    fixed up (`2024-reports` becomes `_2024_reports`).
  - The namespace style matches what the project already uses; with no existing
    files, it's file-scoped on .NET 6+ and block-scoped on older targets.
    `--file-scoped` or `--block` forces one.
  - `record` is refused, with a clear message, on projects older than C# 9 (.NET 5),
    rather than writing a file that won't compile. The C# version is read from
    `<LangVersion>`, or worked out from `<TargetFramework>`.
  - Never overwrites a file, and writes nothing when it refuses (no `.csproj` found,
    two `.csproj` files in one folder, an invalid name, unknown options).
  - Leaving out the type or name prompts for it; typing `Order.cs` is taken as
    `Order`; an interface named without an `I` prefix gets a naming tip.
- `DEVELOPMENT.md`: how each generated feature works, and how the tool's own code
  is organized.

### Fixed
- **The demo GIF didn't show on the NuGet package page.** nuget.org can't resolve
  relative image paths, so the README now uses the GIF's full GitHub URL.
- Dead links in the README and docs replaced with working routes.

### Changed
- CI: `actions/setup-dotnet` 4 → 6 and `softprops/action-gh-release` 2 → 3
  (via Dependabot).

## [1.1.0] - 2026-08-01

### Added
- **API style prompt:** Minimal API (top-level `Map*` endpoints) or Standard API
  (controllers backed by an interface/service pair). Both expose identical URLs;
  Standard API is what fills the `Controllers/` and `Services/` folders every
  generated project already has.
- **`BuildQuickPkg add repository`:** a generic `IRepository<T>`/`Repository<T>`
  plus `IUnitOfWork`/`UnitOfWork` with basic CRUD, written next to your
  `DbContext`. Requires `add efcore` first.
- **`BuildQuickPkg add env`:** a `.env` with dummy values matching what the
  project actually has configured (connection string, JWT settings, port), and
  `.env` added to `.gitignore`.
- **`BuildQuickPkg add caddy`:** a Caddyfile reverse proxy wired to the project's
  real port.
- A visual documentation site (Docsify, no build step) under `/docs`, published at
  https://oluiy.github.io/build-quick-aspnet/, with search and sidebar navigation.
- Dependabot for NuGet and GitHub Actions updates.
- An "about the maintainer" page in the docs.

### Changed
- `add jwt` is Controller-style aware: on a Standard API project it adds an
  `AuthController` + `IAuthService`/`AuthService` instead of top-level endpoints.

### Fixed
- Controller-style projects with JWT enabled didn't build: the generated
  `AuthService.cs` lives in the Application project, which lacked the JWT and
  `IConfiguration` packages. They're now added when Standard API and JWT are both
  selected (or JWT is retrofitted).
- The docs site's copy-code button, and a 404 on the docs site.

## [1.0.9] - 2026-07-23

### Added
- **Entity Framework Core** (PostgreSQL or SQL Server) at generation time: the
  provider package, a starter `DbContext` wired into `Program.cs`, and connection
  strings in appsettings.
- **Docker:** a multi-stage `Dockerfile` and `docker-compose.yml`, with a database
  service included automatically when EF Core is selected.
- **JWT authentication:** bearer auth wired end to end, plus sample endpoints to
  issue a token and call a protected route, so it provably works out of the box.
- `appsettings.json`, `appsettings.Development.json` and
  `appsettings.Production.json` in every generated project, layered the standard
  ASP.NET Core way.
- **`BuildQuickPkg add`:** retrofit EF Core, JWT or Docker onto an existing
  generated project, with no regeneration. It inserts at stable markers in
  `Program.cs` and aborts cleanly, writing nothing, if it can't find them.
- `--help` / `-h` and `--version` / `-v`.
- Full documentation under `/docs` (getting started, architecture, one guide per
  feature) and a demo video.

### Fixed
- Projects targeting net9.0/net10.0 failed to build (CS0234/CS7069, a conflict
  between `Microsoft.AspNetCore.OpenApi` and `Swashbuckle.AspNetCore`) and raised a
  high-severity NU1903 vulnerability warning. The conflicting package was removed.
- `Swashbuckle.AspNetCore` upgraded 6.6.2 → 10.2.3 (supports net8.0–net10.0 and
  OpenAPI 3.1); generated `Program.cs` updated for Microsoft.OpenApi 2.x.
- The generated xUnit test project didn't compile (missing `using`).
- The generated CORS policy now applies in Development only, not Production.
- Failures in `dotnet new sln` / `sln add` are reported instead of being swallowed
  behind a false "Success".
- Microservice generation can no longer produce a zero-service solution.
- A project or service name with a stray space crashed generation or produced a
  broken project; names are now trimmed and validated with a clear message.
- With JWT enabled, Swagger UI now shows an Authorize button (a Bearer security
  scheme is registered). Also fixed in `add jwt`.
- Clearer error handling, dead code removed; total time shown to 2 decimal places.

### Changed
- The root project file was renamed to `BuildQuickPkg.csproj`.

## [1.0.8] - 2026-07-22

### Added
- **Deployment style:** Monolithic or Microservice. Microservice asks how many
  services and their names, and generates one independent Clean Architecture
  solution per service plus an aggregate `.sln`.
- Serilog structured logging in every generated API.
- A timer showing the total generation time.
- `CONTRIBUTING.md`.

## [1.0.7] - 2026-07-22

### Fixed
- The generated test project's name and namespace were built from the wrong value
  (the folder path instead of the project name). The test project is now
  `<Name>.UnitTests`, with namespace `<Name>.Tests`.

## [1.0.6] - 2026-07-22

### Fixed
- The package version in the project file.

## [1.0.2] - 2026-07-22

Same code as 1.0.1, re-tagged.

## [1.0.5] - 2026-07-22

### Added
- The generated API's `Program.cs` declares a `Program` class, so integration
  tests can reference it (`WebApplicationFactory<Program>`).

## [1.0.5] - 2026-07-22

### Added
- First release: an interactive CLI that scaffolds a Clean Architecture ASP.NET
  Core solution (API, Application, Domain, and optionally Infrastructure), with
  project references wired and a test project.
- Automated release workflow: a `v*` tag builds and publishes to NuGet.


[1.1.1]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.0.8...v1.1.0
[1.0.9]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.0.8...v1.0.9
[1.0.8]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.0.7...v1.0.8
[1.0.7]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.0.6...v1.0.7
[1.0.6]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.0.1...v1.0.6
[1.0.5]: https://github.com/Oluiy/build-quick-aspnet/compare/v1.0.0...v1.0.1
[1.0.1]: https://github.com/Oluiy/build-quick-aspnet/releases/tag/v1.0.0
