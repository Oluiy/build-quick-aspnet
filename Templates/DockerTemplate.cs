using create_aspnet_app.Scaffolding;

namespace create_aspnet_app.Templates;

/// <summary>
/// Produces the <c>Dockerfile</c> and <c>docker-compose.yml</c> written to the root of each
/// generated project.
/// </summary>
internal static class DockerTemplate
{
    /// <summary>Builds a multi-stage <c>Dockerfile</c> that restores, publishes, and runs the API project.</summary>
    public static string Dockerfile(string apiProject, string targetFramework)
    {
        var runtimeVersion = targetFramework.Replace("net", "");

        return $$"""
            FROM mcr.microsoft.com/dotnet/sdk:{{runtimeVersion}} AS build
            WORKDIR /src

            COPY . .
            RUN dotnet restore "src/{{apiProject}}/{{apiProject}}.csproj"
            RUN dotnet publish "src/{{apiProject}}/{{apiProject}}.csproj" -c Release -o /app/publish /p:UseAppHost=false

            FROM mcr.microsoft.com/dotnet/aspnet:{{runtimeVersion}} AS final
            WORKDIR /app
            COPY --from=build /app/publish .
            EXPOSE 8080
            ENV ASPNETCORE_URLS=http://+:8080
            ENTRYPOINT ["dotnet", "{{apiProject}}.dll"]
            """;
    }

    /// <summary>Builds a <c>docker-compose.yml</c> running the API on <paramref name="httpPort"/>, plus a database service when <paramref name="efProvider"/> is set.</summary>
    public static string DockerCompose(string projectName, int httpPort, EfCoreProvider efProvider)
    {
        if (efProvider == EfCoreProvider.None)
        {
            return $$"""
                services:
                  api:
                    build:
                      context: .
                      dockerfile: Dockerfile
                    ports:
                      - "{{httpPort}}:8080"
                    environment:
                      - ASPNETCORE_ENVIRONMENT=Development
                """;
        }

        var connectionString = EfCoreTemplate.ComposeConnectionString(projectName, efProvider);
        var dbService = efProvider switch
        {
            EfCoreProvider.PostgreSql => $$"""
                  db:
                    image: postgres:16
                    environment:
                      - POSTGRES_DB={{projectName}}_Dev
                      - POSTGRES_USER=postgres
                      - POSTGRES_PASSWORD=postgres
                    ports:
                      - "5432:5432"
                    volumes:
                      - db-data:/var/lib/postgresql/data
                """,
            EfCoreProvider.SqlServer => $$"""
                  db:
                    image: mcr.microsoft.com/mssql/server:2022-latest
                    environment:
                      - ACCEPT_EULA=Y
                      - MSSQL_SA_PASSWORD=Your_password123
                    ports:
                      - "1433:1433"
                    volumes:
                      - db-data:/var/opt/mssql
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(efProvider), efProvider, null)
        };

        return $$"""
            services:
              api:
                build:
                  context: .
                  dockerfile: Dockerfile
                ports:
                  - "{{httpPort}}:8080"
                environment:
                  - ASPNETCORE_ENVIRONMENT=Development
                  - ConnectionStrings__DefaultConnection={{connectionString}}
                depends_on:
                  - db

            {{dbService}}

            volumes:
              db-data:
            """;
    }
}
