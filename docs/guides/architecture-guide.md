# Architecture Guide

BuildQuickPkg scaffolds [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html): dependencies point inward, toward the Domain, and never the other way around.

## The dependency rule

```
API → Application, Infrastructure
Infrastructure → Application, Domain
Application → Domain
Domain → (nothing)
```

`Domain` has zero project references: it's pure entities, DTOs, and enums. Everything else depends on it, directly or indirectly, never vice versa. This is what makes the Domain layer trivially testable and reusable, and it's enforced structurally: the generated `.csproj` files simply don't have the reference that would let you violate it.

## 4-layer vs. 3-layer

You choose this once, at generation time; it changes the folder layout and which project owns `Infrastructure/Context`.

### 4-layer (default recommendation for anything non-trivial)

```
src/
├── MyApp_API/              # Controllers, Program.cs, Swagger, CORS, launch profiles
├── MyApp_Application/      # Services/Implementation, Services/Interfaces, Utilities
├── MyApp_Domain/           # Dtos, Entity, Enums, no dependencies
└── MyApp_Infrastructure/   # Context/ (EF Core DbContext lives here), Migrations/
```

A dedicated Infrastructure project holds persistence concerns (`DbContext`, migrations, any external service clients you add later) separately from Domain. This is the standard shape for anything that's going to grow: multiple data sources, external API clients, background jobs, etc.

### 3-layer (smaller services)

```
src/
├── MyApp_API/
├── MyApp_Application/
└── MyApp_Domain/
    └── Infrastructure/
        ├── Context/         # EF Core DbContext lives here instead
        └── Migrations/
```

No dedicated Infrastructure project; those concerns are folded into `Domain/Infrastructure/` instead. One fewer project to open, build, and reference, which is a reasonable trade for a small service or a prototype. The dependency rule is unaffected: Domain still has no external references, it just now contains its own persistence folder.

**Rule of thumb:** if you're not sure, pick 4-layer. It costs you one extra project and buys you a clean home for persistence code as the service grows. Reach for 3-layer when you know the service is going to stay small (a handful of endpoints, one table or two).

## Where Entity Framework Core fits

If you say yes to EF Core during generation, the `DbContext` and its NuGet packages go wherever `Infrastructure/Context` lives for the architecture you picked: the dedicated Infrastructure project in 4-layer, or `Domain/Infrastructure/Context` in 3-layer. See [Entity Framework Core](entity-framework-core.md) for the full picture.

## Folder reference

| Folder | Layer | Purpose |
| --- | --- | --- |
| `Controllers/` | API | (Empty by default; the sample health check is a minimal-API endpoint in `Program.cs`, not a controller. Add controllers here if you prefer that style.) |
| `Extensions/` | API | `IServiceCollection` / `WebApplicationBuilder` extension methods you add |
| `Middlewares/` | API | Custom middleware |
| `Services/Implementation/`, `Services/Interfaces/` | Application | Your use cases, split into interface + implementation |
| `Utilities/` | Application | Shared helpers for the Application layer |
| `Dtos/RequestDtos/`, `Dtos/ResponseDtos/` | Domain | Request/response contracts |
| `Entity/` | Domain | Your domain entities |
| `Enums/` | Domain | Shared enums |
| `Context/`, `Migrations/` | Infrastructure (or `Domain/Infrastructure/` in 3-layer) | `DbContext` and EF Core migrations |

## Monolithic vs. microservice

Architecture pattern (4-layer/3-layer) and deployment style (monolithic/microservice) are independent choices: you can generate a 3-layer microservice split, or a 4-layer monolith, or any other combination. See [Microservices](microservices.md) for how the microservice option changes the output tree.
