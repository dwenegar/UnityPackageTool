// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.IO;
using UnityPackageTool.Commands.Dependencies;
using UnityPackageTool.Commands.Documentation;
using UnityPackageTool.Commands.New;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ReplaceAutoPropertyWithComputedProperty

namespace UnityPackageTool;

[Subcommand(typeof(NewCommand), typeof(DependenciesCommand), typeof(DocumentationCommand))]
sealed class UnityPackageToolApp : RootCommandBase
{
    [Option("-C", Description = "Path to the working directory")]
    [DirectoryExists]
    public string WorkingDirectory { get; } = Directory.GetCurrentDirectory();

    [Option("--log-level", Description = "Set the log verbosity.")]
    public LogLevel LogLevel { get; } = LogLevel.Information;

    [Option("--log-file", Description = "Set the log file.")]
    public string? LogFile { get; } = null;
}
