# Microservices

Choosing **Microservice** for "Deployment Style" generates one fully independent Clean Architecture solution per named service, plus an aggregate root solution that references all of them.

## What you get

```
ShopSystem/
├── ShopSystem.sln              # aggregate solution, builds every service at once
├── .gitignore
├── README.md
└── services/
    ├── OrderService/
    │   ├── OrderService.sln    # independently buildable/runnable on its own
    │   ├── Dockerfile           # (if Docker was selected)
    │   ├── docker-compose.yml   # (if Docker was selected)
    │   ├── src/OrderService_API/ ...
    │   └── tests/OrderService.UnitTests/
    ├── InventoryService/
    │   └── ...
    └── PaymentService/
        └── ...
```

Every service:

- Gets the full project set for your chosen architecture (4-layer or 3-layer), the same shape as a monolithic generation, just rooted under `services/<Name>/`.
- Shares the same target framework, architecture pattern, and add-on choices (EF Core provider, Docker, JWT); these are asked once, up front, and applied identically to every service.
- Gets its own `appsettings.*`, `DbContext` (if EF Core was selected), and `Dockerfile`/`docker-compose.yml` (if Docker was selected); nothing is shared between services at the file level, by design. Microservices are meant to be independently deployable.

## Ports

You're asked for one base HTTP/HTTPS port pair. Each service after the first is offset by `+10`:

| Service | HTTP | HTTPS |
| --- | --- | --- |
| Service 1 | 5200 | 5201 |
| Service 2 | 5210 | 5211 |
| Service 3 | 5220 | 5221 |

This means all of them can run side by side on one machine without port collisions, useful for local integration testing across services.

## Running everything at once

Each service is independently runnable:

```bash
cd ShopSystem/services/OrderService/src/OrderService_API && dotnet run
cd ShopSystem/services/InventoryService/src/InventoryService_API && dotnet run
```

Or open the aggregate `ShopSystem.sln` in your IDE to build (and, with a bit of launch-profile configuration, run) all of them together.

If you also chose Docker, each service has its own `docker-compose.yml`; run them individually (`docker compose up` from inside each service's folder), or write a top-level compose file that includes all of them if you want single-command startup across the whole system (not generated automatically, since cross-service networking/naming is a decision specific to your setup).

## What's *not* shared

BuildQuickPkg intentionally does not generate:

- A shared Domain/contracts library between services; each service's `Domain` project is its own, per microservice principles. If you need shared contracts, that's a deliberate architectural decision to make yourself (e.g. a separate NuGet package or shared repo), not something to bolt on by default.
- API gateway, service discovery, or inter-service messaging: out of scope for a scaffolding tool. What you get is N independently buildable, independently runnable, correctly-wired Clean Architecture solutions; how they talk to each other is up to you.
