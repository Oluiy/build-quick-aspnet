# Docker

Answering yes to "Add Dockerfile & docker-compose.yml?" writes both files to the root of the generated project (or, in microservice mode, to the root of *each* service; see [Microservices](microservices.md)).

## What's generated

**`Dockerfile`**: a multi-stage build:

1. `mcr.microsoft.com/dotnet/sdk:<version>` restores and publishes the API project (and everything it references, via the solution's project references).
2. `mcr.microsoft.com/dotnet/aspnet:<version>` runs the published output. This keeps the final image to just the ASP.NET runtime, not the full SDK.

The image listens on port `8080` inside the container (`ASPNETCORE_URLS=http://+:8080`).

**`docker-compose.yml`**: an `api` service that builds from that Dockerfile, plus:

- If you also chose Entity Framework Core, a `db` service (`postgres:16` or `mcr.microsoft.com/mssql/server:2022-latest`, matching your provider choice), with the `api` service's connection string pointed at it by hostname (`db`) instead of `localhost`, and a named volume so data survives container restarts.
- If you chose "None" for EF Core, just the `api` service, no database container.

## Running it

From the project root (next to the `Dockerfile`):

```bash
docker compose up --build
```

The API is reachable at `http://localhost:<your chosen HTTP port>` on the host, mapped through to `8080` in the container. If a `db` service was generated, Compose starts it first and the API waits on it via `depends_on`.

To run only the database (useful if you're running the API from your IDE instead of the container, e.g. for debugging):

```bash
docker compose up db
```

Then run the API normally with `dotnet run`; it'll connect using the `localhost`-based connection string in `appsettings.Development.json`, which points at the same database, published to the host.

## Building the image standalone

```bash
docker build -t myapp-api .
docker run -p 5200:8080 myapp-api
```

## Customizing

Both files are generated once and then yours to edit; BuildQuickPkg doesn't manage them afterward. Common things to change before deploying for real:

- Swap the dev database credentials in `docker-compose.yml` for something pulled from a `.env` file or secret store, rather than the plaintext defaults.
- Add a `HEALTHCHECK` instruction to the `Dockerfile` pointing at `/api/health`.
- If you're deploying behind a reverse proxy or load balancer, adjust `ASPNETCORE_URLS`/exposed port to match your platform's expectations.
