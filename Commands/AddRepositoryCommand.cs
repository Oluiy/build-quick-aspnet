using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Retrofits a generic Repository/UnitOfWork onto an already-generated project via <c>BuildQuickPkg add repository</c>.</summary>
internal static class AddRepositoryCommand
{
    public static void Run(ExistingProject project)
    {
        var dbContextName = $"{project.ProjectName}DbContext";
        var dbContextPath = Path.Combine(project.ContextOwnerDirectory, "Context", $"{dbContextName}.cs");

        if (!File.Exists(dbContextPath))
        {
            AnsiConsole.MarkupLine("[red]Entity Framework Core isn't set up yet.[/] Run [bold cyan]BuildQuickPkg add efcore[/] first: Repository<T> needs a real DbContext to work against.");
            return;
        }

        var repositoriesDirectory = Path.Combine(project.ContextOwnerDirectory, "Repositories");
        var unitOfWorkPath = Path.Combine(repositoriesDirectory, "IUnitOfWork.cs");

        if (File.Exists(unitOfWorkPath))
        {
            AnsiConsole.MarkupLine("[yellow]Repository/UnitOfWork is already set up[/] (IUnitOfWork.cs already exists). Nothing to do.");
            return;
        }

        var repositoriesNamespace = project.IsFourLayer
            ? $"{project.ProjectName}_Infrastructure.Repositories"
            : $"{project.ProjectName}_Domain.Infrastructure.Repositories";

        Directory.CreateDirectory(repositoriesDirectory);
        File.WriteAllText(Path.Combine(repositoriesDirectory, "IRepository.cs"), RepositoryTemplate.IRepository(repositoriesNamespace));
        File.WriteAllText(Path.Combine(repositoriesDirectory, "Repository.cs"), RepositoryTemplate.Repository(repositoriesNamespace));
        File.WriteAllText(unitOfWorkPath, RepositoryTemplate.IUnitOfWork(repositoriesNamespace));
        File.WriteAllText(Path.Combine(repositoriesDirectory, "UnitOfWork.cs"), RepositoryTemplate.UnitOfWork(repositoriesNamespace, dbContextName, project.DbContextNamespace));

        var programCsPath = Path.Combine(project.ApiProjectDirectory, "Program.cs");
        ProgramCsEditor.ApplyInsertions(programCsPath,
        [
            (ProgramCsEditor.UsingsMarker, $"using {repositoriesNamespace};\n"),
            (ProgramCsEditor.ServicesMarker, "\n                    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();\n"),
        ]);

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] Generic Repository/UnitOfWork added to [bold yellow]{project.ProjectName}[/].");
        AnsiConsole.MarkupLine("Inject [bold cyan]IUnitOfWork[/], call [bold cyan]unitOfWork.Repository<YourEntity>()[/] for CRUD, then [bold cyan]await unitOfWork.SaveChangesAsync()[/].");
    }
}
