namespace create_aspnet_app.Templates;

/// <summary>
/// Produces the <c>.csproj</c> content for each Clean Architecture layer, wired up with the
/// correct project-to-project references.
/// </summary>
internal static class CsprojTemplates
{
    /// <summary>Builds the 4-layer API project file, referencing Application and Infrastructure.</summary>
    public static string Api(string applicationProject, string infrastructureProject, string targetFramework, string frameworkPackageVersion) => $$"""
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>{{targetFramework}}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="{{frameworkPackageVersion}}" />
            <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
            <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
          </ItemGroup>

          <ItemGroup>
            <ProjectReference Include="..\{{applicationProject}}\{{applicationProject}}.csproj" />
            <ProjectReference Include="..\{{infrastructureProject}}\{{infrastructureProject}}.csproj" />
          </ItemGroup>
        </Project>
        """;

    /// <summary>Builds the 3-layer API project file, referencing Application and Domain directly (no dedicated Infrastructure project).</summary>
    public static string ThreeLayerApi(string applicationProject, string domainProject, string targetFramework, string frameworkPackageVersion) => $$"""
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>{{targetFramework}}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="{{frameworkPackageVersion}}" />
            <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
            <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
          </ItemGroup>

          <ItemGroup>
            <ProjectReference Include="..\{{applicationProject}}\{{applicationProject}}.csproj" />
            <ProjectReference Include="..\{{domainProject}}\{{domainProject}}.csproj" />
          </ItemGroup>
        </Project>
        """;

    /// <summary>Builds the xUnit test project file, referencing the API project via <c>WebApplicationFactory</c>.</summary>
    public static string Test(string apiProject, string targetFramework, string frameworkPackageVersion) => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{{targetFramework}}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <IsPackable>false</IsPackable>
            <IsTestProject>true</IsTestProject>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
            <PackageReference Include="xunit" Version="2.8.1" />
            <PackageReference Include="xunit.runner.visualstudio" Version="2.8.1" />
            <PackageReference Include="coverlet.collector" Version="6.0.2" />
            <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="{{frameworkPackageVersion}}" />
          </ItemGroup>

          <ItemGroup>
            <ProjectReference Include="..\..\src\{{apiProject}}\{{apiProject}}.csproj" />
          </ItemGroup>
        </Project>
        """;

    /// <summary>Builds the Application layer project file, referencing Domain.</summary>
    public static string Application(string domainProject, string targetFramework) => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{{targetFramework}}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\{{domainProject}}\{{domainProject}}.csproj" />
          </ItemGroup>
        </Project>
        """;

    /// <summary>Builds the Infrastructure layer project file, referencing Domain and Application.</summary>
    public static string Infrastructure(string domainProject, string applicationProject, string targetFramework) => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{{targetFramework}}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\{{domainProject}}\{{domainProject}}.csproj" />
            <ProjectReference Include="..\{{applicationProject}}\{{applicationProject}}.csproj" />
          </ItemGroup>
        </Project>
        """;

    /// <summary>Builds the Domain layer project file. Has no dependencies on other layers.</summary>
    public static string Domain(string targetFramework) => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{{targetFramework}}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>
        </Project>
        """;
}
