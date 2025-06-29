// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace UnityPackageTool.Commands.Dependencies;

[Command("remove", "rm", "r", Description = "Remove a dependency from package.json")]
sealed class DependenciesRemoveCommand : DependenciesCommandBase
{
    [Option("--all|-a", Description = "Remove all dependencies.")]
    bool RemoveAll { get; }

    [Argument(0, Name = "package-name", Description = "Name of the dependency to remove.")]
    [PackageName]
    string? PackageName { get; }

    protected override async ValueTask ExecuteAsync(PackageInfo package, FileManager fileManager, SimpleLogger logger)
    {
        if (package.Dependencies is not null)
        {
            if (RemoveAll)
            {
                package.Dependencies.Clear();
                await fileManager.WritePackageJsonAsync(package);
                logger.Info($"Removed all dependencies from package '{package.Name}'.");
                return;
            }

            if (package.Dependencies.TryRemove(PackageName!, out DependencyInfo? dependency))
            {
                await fileManager.WritePackageJsonAsync(package);
                logger.Info($"Removed dependency '{dependency}' from package '{package.Name}'.");
                return;
            }
        }

        logger.Info($"No dependency named '{PackageName}' found in package '{package.Name}'.");
    }

    [UsedImplicitly]
    ValidationResult? OnValidate() => RemoveAll switch
    {
        true when PackageName is not null => new ValidationResult("The --all option cannot be used together with a specific package name. Please remove the package name or omit --all."),
        false when PackageName is null    => new ValidationResult("You must specify a package name or use the --all option."),
        _                                 => ValidationResult.Success
    };
}
