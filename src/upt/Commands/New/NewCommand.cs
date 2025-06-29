// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ReplaceAutoPropertyWithComputedProperty

using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.New;

[Command("new", Description = "Create a new Unity package in the specified directory.")]
sealed class NewCommand : CommandBase
{
    [Option("--version|-v", Description = "Set the initial semantic version of the package.")]
    [PackageVersion]
    string Version { get; } = "0.0.1";

    [Option("--display-name|-n", Description = "Set human-readable name shown in Unity Package Manager.")]
    string? DisplayName { get; }

    [Option("--assembly|-a", Description = "Set the name of the runtime assembly.")]
    string? AssemblyName { get; }

    [Option("--author|-u", Description = "Set the name of the package author.")]
    string? Author { get; }

    [Option("--author-email|-m", Description = "Set the package author's email address.")]
    string? AuthorEmail { get; }

    [Option("--author-url|-w", Description = "Set the URL of the package author's website.")]
    string? AuthorUrl { get; }

    [Option("--unity|-y", Description = "Set the version of Unity supported by the package.")]
    [RegexMatch(@"^\d{4}\.\d+$", "Invalid version '{0}'. See https://docs.unity3d.com/Manual/upm-semver.html for how to version your package.")]
    string? UnityVersion { get; }

    [Option("--name-space|-N", Description = "Set the root name space of the runtime assembly.")]
    string RootNamespace { get; } = string.Empty;

    [Option("--editor-only|-e", Description = "Initialize an editor-only package.")]
    bool EditorOnly { get; }

    [Option("--runtime-only|-r", Description = "Initialize an runtime-only package.")]
    bool RuntimeOnly { get; }

    [Option("--tests|-t", Description = "Initialize a companion package containing tests.")]
    bool WithTests { get; }

    [Argument(0, "package-name", Description = "The name of the package to initialize. Ignored if --test-only is specified.")]
    [PackageName]
    [Required]
    string PackageName { get; } = "com.example.package";

    [Option("--force|-f", Description = "Overwrite any existing files if they already exists.")]
    bool Force { get; }

    protected override void ValidateCommand()
    {
        if (EditorOnly && RuntimeOnly)
            throw new CommandException("The options --editor-only and --runtime-only cannot be used at the same time.");
    }

    protected override async ValueTask ExecuteAsync(FileManager fileManager1, SimpleLogger logger)
    {
        var fileManager = new FileManager(logger);
        InitializationMode mode = InitializationMode.Default;
        if (EditorOnly)
            mode &= InitializationMode.Editor;
        if (RuntimeOnly)
            mode &= InitializationMode.Runtime;

        var initializer = new PackageInitializer(PackageName, Version)
        {
            Force = Force,
            Mode = mode,
            Logger = logger,
            FileManager = fileManager,
            DisplayName = DisplayName,
            UnityVersion = UnityVersion,
            RootNameSpace = RootNamespace,
            RuntimeAssemblyName = AssemblyName ?? GetAssemblyNameFromPackageName(PackageName),
            Author = Author is null ? null : new AuthorInfo(Author, AuthorEmail, AuthorUrl)
        };

        await initializer.InitializePackageAsync();
        if (WithTests)
            await initializer.InitializeTestsPackage();
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
