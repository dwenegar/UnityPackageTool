// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.Dependencies;

[Command("list", "l", Description = "List all dependencies in package.json")]
sealed class DependenciesListCommand : DependenciesCommandBase
{
    protected override ValueTask ExecuteAsync(PackageInfo package, FileManager fileManager, SimpleLogger logger)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine($"Dependencies for package: {package.Name}");
        AnsiConsole.WriteLine();

        DependencyList? dependencies = package.Dependencies;
        if (dependencies == null || dependencies.Count == 0)
            AnsiConsole.WriteLine("No dependencies found.");
        else
        {
            foreach (DependencyInfo dependency in dependencies)
                AnsiConsole.WriteLine($"- {dependency}");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine($"Total dependencies: {dependencies.Count}");
        }

        return ValueTask.CompletedTask;
    }
}
