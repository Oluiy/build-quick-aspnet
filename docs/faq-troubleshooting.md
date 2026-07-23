# FAQ & Troubleshooting

## Installation

**`dotnet: command not found` / `BuildQuickPkg: command not found` after install**
Make sure the .NET SDK is installed (`dotnet --version`) and that `~/.dotnet/tools` (macOS/Linux) or `%USERPROFILE%\.dotnet\tools` (Windows) is on your `PATH`; that's where global tools install to. Restart your terminal after installing if the command still isn't found.

**"A command with this name already exists" during install**
Unlikely, but possible: some other global tool is using the `BuildQuickPkg` command name. Run `dotnet tool uninstall --global BuildQuickPkg` first, then reinstall.

## Generation

**The selection prompts (arrow-key menus) don't work / show garbled output**
Spectre.Console's interactive prompts need a real, interactive terminal (a TTY); they won't work piped through another process, in some CI runners, or in certain embedded terminals. Run BuildQuickPkg directly in a normal terminal window (Terminal.app, Windows Terminal, iTerm2, a standard VS Code integrated terminal, etc.).

**I want to regenerate with different answers**
There's no in-place "reconfigure"; delete the generated folder (or generate into a new empty directory) and run `BuildQuickPkg` again. Each run is a clean set of prompts.

## Building the generated solution

**`dotnet build` fails right after generation**
This shouldn't happen; file an issue with the exact prompts/answers you used (see below) if it does. First things to check yourself:
- `dotnet --version` is 8.0 or later.
- If you picked `net9.0` or `net10.0` as the target framework, make sure that SDK is actually installed (`dotnet --list-sdks`); BuildQuickPkg lets you pick a TFM the generator itself doesn't require, but your machine still needs the matching SDK to build it.

**NuGet restore fails / can't find a package**
Usually a network issue or a very new target framework (e.g. `net10.0` while it's still in preview) where a dependency hasn't published a matching package version yet. Try `net8.0` or `net9.0` if `net10.0` restore fails.

## Entity Framework Core

**`dotnet ef` command not found**
Install the tool once per machine: `dotnet tool install --global dotnet-ef`. See [Entity Framework Core](entity-framework-core.md).

**Migration/connection errors ("Connection refused", "server not found")**
There's no database server running yet; EF Core doesn't start one for you. Either run `docker compose up db` (if you also chose Docker), or install PostgreSQL/SQL Server locally and make sure the connection string in `appsettings.Development.json` matches your actual setup (host, port, credentials, database name).

## Docker

**`docker compose up` fails with "Cannot connect to the Docker daemon"**
Docker Desktop (or your Docker engine) isn't running. Start it, then retry.

**The API container can't reach the database**
Inside Docker, services reach each other by service name, not `localhost`; the generated `docker-compose.yml` already points the `api` service at `db` correctly. If you edited the compose file and broke that, or you're running the API outside Docker (e.g. `dotnet run` from your IDE) while the database is in a container, use the `localhost`-based connection string from `appsettings.Development.json` instead (Compose publishes the db port to the host).

## JWT

**`401 Unauthorized` on every request, even with a token**
Check that `Authorization: Bearer <token>` is set exactly (note the space after `Bearer`), and that the token hasn't expired (`Jwt:ExpiryMinutes` in `appsettings.json`, default 60). If you changed `Jwt:Issuer`/`Jwt:Audience` after generating a token, old tokens will fail validation against the new values; get a fresh token from `/api/auth/token`.

## Still stuck?

Open an issue at [github.com/Oluiy/build-quick-aspnet/issues](https://github.com/Oluiy/build-quick-aspnet/issues) with:

- The exact prompts/answers you chose (or the command, if you used an argument)
- What you expected vs. what happened, including the full error message
- Your `dotnet --version` output
