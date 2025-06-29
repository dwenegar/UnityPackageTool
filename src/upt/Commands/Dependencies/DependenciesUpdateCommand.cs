// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UnityPackageTool.Upm;
using UnityPackageTool.Utils;
using static UnityPackageTool.Commands.Dependencies.DependenciesCommandHelpers;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace UnityPackageTool.Commands.Dependencies;

[Command("update", "up", "u", Description = "Update the version of an existing dependency")]
sealed class DependenciesUpdateCommand : DependenciesCommandBase
{
    [Option("--all|-a", Description = "Update all dependencies.")]
    bool UpdateAll { get; }

    [Argument(0, Name = "package-name", Description = "Name of the dependency to add.")]
    [PackageName]
    string? PackageName { get; }

    [Argument(1, Name = "package-version", Description = "Version of the dependency to add.")]
    [PackageVersion]
    string? PackageVersion { get; }

    protected override async ValueTask ExecuteAsync(PackageInfo package, FileManager fileManager, SimpleLogger logger)
    {
        if (package.Dependencies is null || package.Dependencies.Count == 0)
        {
            logger.Info("Nothing to do.");
            return;
        }

        var packageNames = new List<string>();
        if (UpdateAll)
            packageNames.AddRange(package.Dependencies.Select(x => x.Name));
        else
            packageNames.Add(PackageName!);

        foreach (string packageName in packageNames)
            await UpdateDependency(packageName);
        await fileManager.WritePackageJsonAsync(package);

        async ValueTask UpdateDependency(string packageName)
        {
            var upmClient = new UpmClient(logger);
            UpmPackageInfo[]? packageVersions = await upmClient.GetPackageVersionsAsync(packageName);
            if (packageVersions is null)
                return;

            string? packageVersion = PackageVersion;
            if (!TryResolvePackageVersion(packageVersions, package, ref packageVersion))
            {
                logger.Warn(packageVersion is null
                                ? $"No available versions found for the package '{packageName}' compatible with Unity {package.Unity}."
                                : $"Package '{packageName}@{packageVersion}' does not exists or is not compatible with Unity {package.Unity}.");
                return;
            }

            if (!package.Dependencies.TryFindByName(packageName, out int index, out DependencyInfo? dependency))
            {
                logger.Error($"Package '{packageName}' is not a dependency of {package.Name}.");
                return;
            }

            string previousVersion = dependency.Version;
            if (previousVersion == packageVersion)
            {
                logger.Info($"Dependency '{packageName}' is already at the requested version ({packageVersion}). No update needed.");
                return;
            }

            package.Dependencies[index] = dependency with { Version = packageVersion };
            logger.Info($"Updated dependency '{packageName}' from version {previousVersion} to version {packageVersion}");
        }
    }

    [UsedImplicitly]
    ValidationResult? OnValidate() => UpdateAll switch
    {
        true when PackageName is not null => new ValidationResult("The --all option cannot be used together with a specific package name. Please remove the package name or omit --all."),
        false when PackageName is null    => new ValidationResult("You must specify a package name or use the --all option."),
        _                                 => ValidationResult.Success
    };
}
