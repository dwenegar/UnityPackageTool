// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UnityPackageTool.Upm;
using UnityPackageTool.Utils;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ReplaceAutoPropertyWithComputedProperty

namespace UnityPackageTool.Commands.Dependencies;

[Command("add", "a", Description = "Add a new dependency to package.json")]
sealed class DependenciesAddCommand : DependenciesCommandBase
{
    [Argument(0, Name = "package-name", Description = "Name of the dependency to add.")]
    [PackageName]
    [Required]
    string PackageName { get; } = string.Empty;

    [Argument(1, Name = "package-version", Description = "Version of the dependency to add.")]
    [PackageVersion]
    string? PackageVersion { get; }

    protected override async ValueTask ExecuteAsync(PackageInfo package, FileManager fileManager, SimpleLogger logger)
    {
        package.Dependencies ??= [];
        if (package.Dependencies.Any(x => x.Name == PackageName))
            throw new CommandException($"The dependency '{PackageName}' has been already added. Use `update` to update it.");

        var upmClient = new UpmClient(logger);
        UpmPackageInfo[]? packageVersions = await upmClient.GetPackageVersionsAsync(PackageName);
        if (packageVersions is null)
            return;

        string? packageVersion = PackageVersion;
        if (!DependenciesCommandHelpers.TryResolvePackageVersion(packageVersions, package, ref packageVersion))
        {
            logger.Warn(packageVersion is null
                            ? $"No available versions found for the package '{PackageName}' compatible with Unity {package.Unity}."
                            : $"Package '{PackageName}@{packageVersion}' does not exists or is not compatible with Unity {package.Unity}.");
            return;
        }

        package.Dependencies.Add(new DependencyInfo(PackageName, packageVersion));
        await fileManager.WritePackageJsonAsync(package);
        logger.Info($"Added dependency '{PackageName}@{packageVersion}' to package '{package.Name}'.");
    }
}
