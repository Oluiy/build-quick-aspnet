# BuildQuickPkg Docs

Everything you need to install, run, and extend BuildQuickPkg: an interactive .NET CLI that scaffolds a Clean Architecture ASP.NET Core solution in seconds.

If you just want to get a project running, start with **Getting Started**. Everything else is reference material you can come back to.

| Guide | What's in it |
| --- | --- |
| [Getting Started](guides/getting-started.md) | Install, upgrade, downgrade, or uninstall the tool; generate your first project, run it |
| [CLI Reference](guides/cli-reference.md) | Every prompt, its options, and its default |
| [Adding a Feature Later](guides/adding-features-later.md) | `BuildQuickPkg add efcore/jwt/docker`: retrofit an optional feature onto a project you already generated |
| [Architecture Guide](guides/architecture-guide.md) | 4-layer vs. 3-layer, the dependency rule, where things live |
| [Entity Framework Core](guides/entity-framework-core.md) | PostgreSQL / SQL Server setup, migrations, connection strings |
| [Docker](guides/docker.md) | The generated Dockerfile and docker-compose.yml, running the stack |
| [JWT Authentication](guides/jwt-authentication.md) | Getting a token, calling protected endpoints, going to production |
| [Microservices](guides/microservices.md) | Generating multiple independent services from one run |
| [FAQ & Troubleshooting](guides/faq-troubleshooting.md) | Common errors and how to fix them |

New to ASP.NET Core entirely? Getting Started assumes you have the .NET SDK installed and can run a terminal command, nothing else.

## Where to go for help

- Bugs or feature requests: [open an issue](https://github.com/Oluiy/build-quick-aspnet/issues)
- Want to contribute to the tool itself: see [CONTRIBUTING.md](https://github.com/Oluiy/build-quick-aspnet/blob/main/CONTRIBUTING.md)
- Everything else: the main [README.md](https://github.com/Oluiy/build-quick-aspnet/blob/main/README.md)
