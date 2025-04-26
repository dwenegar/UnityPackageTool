// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands;

sealed class DocumentationInitializer(string packagePath)
{
    public DocumentationInitializer(string packageName, string outputPath)
        : this(Path.Combine(outputPath, packageName)) { }

    public required FileManager FileManager { get; init; }
    public required ISimpleLogger Logger { get; init; }

    public async Task InitializeAsync(bool force)
    {
        string documentationPath = Path.Combine(packagePath, "Documentation~");
        if (Directory.Exists(documentationPath))
        {
            if (!force)
                throw new CommandException($"Cannot create a directory `{documentationPath}`. The directory already exists. Use option --force to overwrite it.");
            FileManager.DeleteDirectory(documentationPath);
        }

        FileManager.CreateDirectory(documentationPath);

        PackageInfo package = await LoadPackageInfo();

        var config = new DocumentationConfig();
        string configPath = Path.Combine(documentationPath, "config.json");
        await FileManager.WriteJsonAsync(configPath, config);

        string indexPath = Path.Combine(documentationPath, "index.md");
        string titleDisplayName = package.DisplayName ?? package.Name;
        string displayName = package.DisplayName ?? $"`{package.Name}`";
        string indexContent = $"""
                               # {titleDisplayName} {package.Version}

                               This is the documentation for the package {displayName}.
                               """;
        await FileManager.WriteTextAsync(indexPath, indexContent);
    }

    async Task<PackageInfo> LoadPackageInfo()
    {
        string packageJsonPath = Path.Combine(packagePath, "package.json");
        PackageInfo? package = await FileManager.ReadJsonAsync<PackageInfo>(packageJsonPath);
        if (package is not { IsValid: true })
            throw new CommandException($"Invalid package at {packagePath}. The file `package.json` does not exist or is invalid.");
        return package;
    }
}
