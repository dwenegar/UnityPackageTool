// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ReplaceAutoPropertyWithComputedProperty

using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;
using UnityPackageTool.Utils;
using static UnityPackageTool.Commands.CommandHelpers;

namespace UnityPackageTool.Commands.Documentation;

[Command("init", Description = "Initialize the package documentation.")]
sealed class DocumentationInitCommand : CommandBase
{
    [Option("--force|-f", Description = "Overwrite any existing files if they already exists.")]
    bool Force { get; }

    protected override async ValueTask ExecuteAsync(FileManager fileManager1, SimpleLogger logger)
    {
        var fileManager = new FileManager(logger);

        PackageInfo package = await ReadPackageJsonAsync(fileManager);
        var initializer = new DocumentationInitializer(package.Name, package.Version)
        {
            Force = Force,
            Logger = logger,
            FileManager = fileManager,
            DisplayName = package.DisplayName ?? package.Name
        };

        await initializer.InitializeAsync();
    }
}
