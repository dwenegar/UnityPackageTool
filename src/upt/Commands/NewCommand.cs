// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable ReplaceAutoPropertyWithComputedProperty

using System;
using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.Init;

sealed partial class NewCommand : CommandBase
{
    protected override async Task<bool> ExecuteAsync(ISimpleLogger logger)
    {
        if (EditorOnly && RuntimeOnly)
            throw new CommandException("The options --editor-only and --runtime-only cannot be used at the same time.");

        string packageName = PackageName ?? throw new InvalidOperationException("Internal error");
        string version = Version ?? throw new InvalidOperationException("Internal error");
        string outputPath = Path.GetFullPath(Output);

        var fileManager = new FileManager(logger);
        InitializationMode mode = InitializationMode.Default;
        if (EditorOnly)
            mode &= InitializationMode.Editor;
        if (RuntimeOnly)
            mode &= InitializationMode.Runtime;

        var initializer = new PackageInitializer(packageName, version, mode, outputPath)
        {
            Logger = logger,
            FileManager = fileManager,
            DisplayName = DisplayName,
            UnityVersion = UnityVersion,
            RuntimeAssemblyName = AssemblyName ?? GetAssemblyNameFromPackageName(packageName),
            Author = Author is null ? null : new AuthorInfo(Author, AuthorEmail, AuthorUrl)
        };

        await initializer.InitializePackageAsync(Force);

        if (WithTests)
            await initializer.InitializeTestsPackage(Force);

        if (!WithoutDocumentation)
        {
            var documentationInitializer = new DocumentationInitializer(packageName, outputPath)
            {
                Logger = logger,
                FileManager = fileManager
            };

            await documentationInitializer.InitializeAsync(Force);
        }

        return true;
    }

    static string GetAssemblyNameFromPackageName(string packageName)
    {
        Span<char> buffer = stackalloc char[packageName.Length];

        int j = 0;
        bool needCapitalization = true;
        int startIndex = packageName.IndexOf('.', StringComparison.Ordinal); // skip "com", "net", etc.
        for (int i = startIndex + 1; i < packageName.Length; i++)
        {
            char c = packageName[i];
            if (char.IsLetterOrDigit(c) || c == '.')
                buffer[j++] = needCapitalization ? char.ToUpperInvariant(c) : c;
            needCapitalization = !char.IsLetterOrDigit(c);
        }

        return buffer[..j].ToString();
    }
}
