// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable ReplaceAutoPropertyWithComputedProperty

using JetBrains.Annotations;
using Lunet.Extensions.Logging.SpectreConsole;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Logging;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands;

abstract class CommandBase
{
    [Option("--log-level", Description = "Set the log verbosity.")]
    LogLevel LogLevel { get; } = LogLevel.Information;

    [Option("--log-file", Description = "Set the log file.")]
    string? LogFile { get; } = null;

    [UsedImplicitly]
    public async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        using var factory = new LoggerFactory();
        factory.AddProvider(new SpectreConsoleLoggerProvider(new SpectreConsoleLoggerOptions
        {
            ConsoleSettings = new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(Console.Out)
            },
            LogLevel = LogLevel,
            IndentAfterNewLine = false,
            IncludeTimestamp = true,
            IncludeNewLineBeforeMessage = false,
            IncludeCategory = false
        }));

        if (LogFile is not null)
        {
            string logFilePath = Path.GetFullPath(LogFile);
            factory.AddProvider(new FileLoggerProvider(new FileLoggerOptions
            {
                FilePath = logFilePath,
                LogLevel = LogLevel
            }));
        }

        ISimpleLogger logger = SimpleLogger.CreateLogger(factory, AssemblyHelpers.GetAssemblyName());
        return await ExecuteAsync(logger) ? 0 : 1;
    }

    protected abstract Task<bool> ExecuteAsync(ISimpleLogger logger);
}
