# Adding a Feature Later

Said no to Entity Framework Core, Docker, or JWT when you first generated a project, and want it now? You don't need to regenerate anything: `BuildQuickPkg add` retrofits it onto the project you already have, in place.

```bash
BuildQuickPkg add efcore postgres    # or: sqlserver
BuildQuickPkg add jwt
BuildQuickPkg add docker
BuildQuickPkg add repository         # requires efcore first
BuildQuickPkg add env
BuildQuickPkg add caddy
```

Leave off the option and it'll prompt for what it needs, the same way the interactive generator does:

```bash
BuildQuickPkg add efcore
# Which Entity Framework Core provider?
# > PostgreSQL
#   SQL Server
```

Leave off the feature entirely and it'll ask which one:

```bash
BuildQuickPkg add
# What would you like to add to MyAwesomeApi?
# > efcore
#   jwt
#   docker
#   repository
#   env
#   caddy
```

## Where to run it

From the project root, the API project's own folder, or (in microservice mode) a specific service's root; `add` figures out which project you mean from wherever you run it:

```bash
cd MyAwesomeApi              # or MyAwesomeApi/src/MyAwesomeApi_API, or services/OrderService
BuildQuickPkg add efcore postgres
```

If you run it from a microservice aggregate root with more than one service underneath, it'll ask you to `cd` into the specific service first rather than guessing which one you meant.

## What each one adds

Exactly what you'd have gotten by answering "yes" at generation time; see the dedicated guide for the full picture of each:

- [Entity Framework Core](guides/entity-framework-core.md): the provider package, a starter `DbContext`, `AddDbContext` wired into `Program.cs`, and a connection string in `appsettings.Development.json`.
- [JWT Authentication](guides/jwt-authentication.md): the `JwtBearer` package, auth services/middleware, the sample token-issuing and protected endpoints, and the `Jwt` config section. If the project was generated with the Standard API style, this adds an `AuthController` + `IAuthService`/`AuthService` instead of the minimal endpoints; either way the URLs are identical.
- [Docker](guides/docker.md): the `Dockerfile` and `docker-compose.yml`.
- **repository**: a generic `IRepository<T>`/`Repository<T>` plus `IUnitOfWork`/`UnitOfWork` with basic CRUD (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Remove`, `SaveChangesAsync`), written next to the DbContext (Infrastructure in 4-layer, `Domain/Infrastructure` in 3-layer). Requires `add efcore` to have been run first; it works against any entity, since it doesn't know what entities you have.
- **env**: a `.env` file at the project root, with dummy values that reflect what's actually configured (`ConnectionStrings__DefaultConnection` if EF Core is set up, `Jwt__*` if JWT is set up, always `ASPNETCORE_ENVIRONMENT`/`ASPNETCORE_URLS`). Automatically added to `.gitignore`.
- **caddy**: a `Caddyfile` at the project root, reverse-proxying to the project's actual configured port. Replace `localhost` with your real domain before deploying.

## It won't run twice

Each command checks for what it would add first: an existing `DbContext`, `JwtBearerDefaults` already in `Program.cs`, a `Dockerfile` already at the project root, an existing `IUnitOfWork.cs`, `.env`, or `Caddyfile`, and just tells you it's already there instead of duplicating anything.

## If you've heavily edited Program.cs

`add efcore`, `add jwt`, and `add repository` insert their code at stable marker comments (`// BuildQuickPkg:usings`, `// BuildQuickPkg:services`, and so on) that every generated `Program.cs` contains. As long as those markers are still there, it doesn't matter what else you've changed around them: your own endpoints, controllers, and middleware are left alone.

If you've removed a marker (or rewritten `Program.cs` from scratch), `add` won't guess where the new code should go; it stops with an error naming the missing marker, and nothing is written. Add the required lines by hand in that case; the relevant guide (linked above) shows exactly what's needed.
