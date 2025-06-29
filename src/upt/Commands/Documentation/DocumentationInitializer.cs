// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.Documentation;

sealed class DocumentationInitializer(string packageName, string version)
{
    public required SimpleLogger Logger { get; init; }
    public required FileManager FileManager { get; init; }
    public required string DisplayName { get; init; }
    public bool Force { get; init; }

    public async ValueTask InitializeAsync()
    {
        string documentationPath = Path.GetFullPath("Documentation~");
        if (Directory.Exists(documentationPath))
        {
            if (!Force)
                throw new CommandException($"Cannot create the directory '{documentationPath}'. The directory already exists. Use option --force to overwrite it.");
            FileManager.DeleteDirectory(documentationPath);
        }

        FileManager.CreateDirectory(documentationPath);

        var config = new DocumentationConfiguration();
        string configJsonPath = Path.Combine(documentationPath, "config.json");
        await FileManager.WriteJsonAsync(configJsonPath, config);

        string indexPath = Path.Combine(documentationPath, "index.md");
        string indexContent = $"""
                               #{packageName} {version}

                               This is the documentation for the package {DisplayName}.
                               """;
        await FileManager.WriteTextAsync(indexPath, indexContent);
    }
}
