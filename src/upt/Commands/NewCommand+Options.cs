// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable ReplaceAutoPropertyWithComputedProperty

using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.IO;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.Init;

[Command("new", Description = "Create a new Unity package in the specified directory.")]
sealed partial class NewCommand
{
    [Option("--version|-v", Description = "Set the initial semantic version of the package.")]
    [RegexMatch(@"^\d+\.\d+\.\d+$", "Invalid version '{0}'. See https://docs.unity3d.com/Manual/upm-semver.html for how to version your package.")]
    string Version { get; } = "0.0.1";

    [Option("--display-name|-n", Description = "Set human-readable name shown in Unity Package Manager.")]
    string? DisplayName { get; } = null;

    [Option("--assembly|-a", Description = "Set the name of the runtime assembly.")]
    string? AssemblyName { get; } = null;

    [DirectoryExists]
    [Option("--output|-o", Description = "Set the directory where the package will be created.")]
    string Output { get; } = Directory.GetCurrentDirectory();

    [Option("--author|-u", Description = "Set the name of the package author.")]
    string? Author { get; } = null;

    [Option("--author-email|-m", Description = "Set the package author's email address.")]
    string? AuthorEmail { get; } = null;

    [Option("--author-url|-w", Description = "Set the URL of the package author's website.")]
    string? AuthorUrl { get; } = null;

    [Option("--unity|-y", Description = "Set the version of Unity supported by the package.")]
    [RegexMatch(@"^\d{4}\.\d+$", "Invalid version '{0}'. See https://docs.unity3d.com/Manual/upm-semver.html for how to version your package.")]
    string? UnityVersion { get; } = null;

    [Option("--editor-only|-e", Description = "Initialize an editor-only package.")]
    bool EditorOnly { get; } = false;

    [Option("--runtime-only|-r", Description = "Initialize an runtime-only package.")]
    bool RuntimeOnly { get; } = false;

    [Option("--tests|-t", Description = "Initialize a companion package containing tests.")]
    bool WithTests { get; } = false;

    [Option("--no-docs|-D", Description = "Initialize the package's documentation folder.")]
    bool WithoutDocumentation { get; } = true;

    [Argument(0, "package-name", Description = "The name of the package to initialize. Ignored if --test-only is specified.")]
    [RegexMatch(@"^(?:[a-z0-9][a-z0-9\-_]*\.)+[a-z0-9][a-z0-9\-_]*$", "Invalid package name '{0}'. See https://docs.unity3d.com/Manual/cus-naming.html for how to name your package.")]
    [Required]
    string? PackageName { get; } = null;

    [Option("--force|-f", Description = "Overwrite any existing files if they already exists.")]
    bool Force { get; } = false;
}
