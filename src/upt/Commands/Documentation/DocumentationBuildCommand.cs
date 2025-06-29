// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityPackageTool.Utils;
using static UnityPackageTool.Commands.CommandHelpers;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ReplaceAutoPropertyWithComputedProperty

namespace UnityPackageTool.Commands.Documentation;

[Command("build", Description = "Build the package documentation.")]
sealed partial class DocumentationBuildCommand : CommandBase
{
    static readonly Version s_DocFxMinimumVersion = new(2, 70, 0);
    [Option("-o|--output", Description = "The output folder.")]
    string OutputPath { get; } = "docs~";

    [DirectoryExists]
    [Option("--with-docfx", Description = "The folder of the DocFx installation to use.")]
    string? DocFxPath { get; }

    [Option("--keep-build-folder", Description = "Keep the build folder.")]
    bool KeepBuildFolder { get; } = false;

    [Option("--build", Description = "The folder used for building the documentation.")]
    string? BuildPath { get; } = null;

    [Option("--force|-f", Description = "Overwrite any existing files if they already exists.")]
    bool Force { get; }

    protected override async ValueTask ExecuteAsync(FileManager fileManager1, SimpleLogger logger)
    {
        var fileManager = new FileManager(logger);

        PackageInfo package = await ReadPackageJsonAsync(fileManager);

        string documentationPath = Path.GetFullPath("Documentation~");
        if (!Directory.Exists(documentationPath))
            throw new CommandValidationException($". Missing '{documentationPath}~' folder.");

        string docFxPath = FindDocFx();
        Version docFxVersion = await CheckDocFxVersionAsync(docFxPath);
        logger.Info($"Using DocFx version {docFxVersion} at {docFxPath}");

        string buildPath = ResolveBuildPath();
        string outputPath = Path.GetFullPath(Path.Combine(OutputPath, package.TrimmedVersion));
        var builder = new DocumentBuilder(documentationPath, buildPath, outputPath)
        {
            FileManager = fileManager,
            KeepBuildFolder = KeepBuildFolder,
            Package = package,
            DocFxPath = docFxPath,
            Logger = logger,
            Force = Force
        };
        await builder.BuildAsync();
    }

    [GeneratedRegex(@"(\d+\.\d+\.\d+)", RegexOptions.Multiline)]
    private static partial Regex GetDocFxVersionRegex();

    string ResolveBuildPath() => BuildPath is null
                                     ? Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
                                     : Path.GetFullPath(BuildPath);

    string FindDocFx()
    {
        if (DocFxPath != null)
        {
            string? docFxExecutablePath = FindDocFxExecutableInPaths([DocFxPath]);
            return docFxExecutablePath ?? throw new CommandException($"DocFx is not installed at {DocFxPath}.");
        }
        else
        {
            string[] paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
            string? docFxExecutablePath = FindDocFxExecutableInPaths(paths);
            return docFxExecutablePath ?? throw new CommandException("Could not find DocFx in the system paths.");
        }

        static string? FindDocFxExecutableInPaths(IEnumerable<string> paths)
        {
            string executableName = OperatingSystem.IsWindows() ? "docfx.exe" : "docfx";
            return paths.Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => Path.Combine(x, executableName))
                        .FirstOrDefault(x => File.Exists(x));
        }
    }

    static async ValueTask<Version> CheckDocFxVersionAsync(string docFxPath)
    {
        var docFx = new DocFx(docFxPath);
        string docFxOutput = await docFx.RunAsync("--version");
        Match match = GetDocFxVersionRegex().Match(docFxOutput);
        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out Version? version) || version < s_DocFxMinimumVersion)
            throw new CommandException($"DocFx {s_DocFxMinimumVersion} is required to build the docs.");
        return version;
    }
}
