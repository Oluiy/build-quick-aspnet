# Contributing to BuildQuickPkg

Thanks for considering a contribution! This project scaffolds Clean Architecture ASP.NET Core solutions, monolithic or microservice, via an interactive CLI, and keeping the generator itself simple and well-organized is part of the point.

## Getting set up

```bash
git clone https://github.com/Oluiy/build-quick-aspnet.git
cd build-quick-aspnet
dotnet build
```

Run it locally without installing the tool:

```bash
dotnet run
# or, to skip the project-name prompt:
dotnet run -- MyTestApp
```

## A note on editing docs via GitHub's web UI

The `docs/` site (rendered by Docsify at https://oluiy.github.io/build-quick-aspnet/) is just the `docs/` folder on `main`; there's no separate build or publish step. That means edits made with GitHub's web editor (the pencil icon on github.com) commit directly to `main`, the same branch your local clones push to.

## Project layout

See the [Project source layout](README.md#project-source-layout) section of the README for how `Program.cs`, `Scaffolding/`, `Templates/`, and `Utilities/` fit together.

## Adding a new generation option

- **New template content** (a new file type in generated projects) → add a method to the matching file in `Templates/`, or create a new `Templates/XyzTemplate.cs` if it doesn't fit an existing one.
- **New prompt / config flag** → add the field to `Scaffolding/ScaffoldingConfig.cs`, wire the prompt into `Program.cs`, and thread it through `Scaffolding/SolutionScaffolder.cs`.
- Keep `SolutionScaffolder.cs` a thin orchestrator; it should call into `Templates/` and `ProjectStructure`, not contain template text or business logic itself.
- If the option affects both monolithic and microservice generation, make sure `GenerateProjectSet` (the shared per-service/per-project path) handles it rather than special-casing one mode.

## Testing your change

There's no automated test suite for the generator itself yet (contributions welcome!). Before opening a PR, verify by hand:

1. `dotnet build` the tool itself: must be warning/error free.
2. Run it against a scratch directory and confirm the generated tree looks right:
   ```bash
   mkdir /tmp/scaffold-check && cd /tmp/scaffold-check
   dotnet run --project /path/to/build-quick-aspnet
   ```
3. `dotnet build` (and, if you generated tests, `dotnet test`) the **generated** solution; this is the real acceptance test, since it proves the templates actually compile and wire up correctly.
4. If you touched microservice generation, verify both a per-service `.sln` and the aggregate root `.sln` build.

## Pull requests

- Keep PRs focused on one change; unrelated cleanups make review harder.
- Update `README.md`, and the relevant guide under [`docs/guides/`](docs/guides/README.md), if you're adding or changing a prompt, generated file, or folder.
- Describe what you tested (see above) in the PR description.

## Reporting bugs / requesting features

Open an issue with:
- What you ran (the exact prompts/answers, or the command if using an argument)
- What you expected vs. what you got
- Your .NET SDK version (`dotnet --version`)
