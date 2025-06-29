// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable ReplaceAutoPropertyWithComputedProperty
// ReSharper disable UnassignedGetOnlyAutoProperty

using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Logging;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands;

abstract class CommandBase
{
    [UsedImplicitly]
    public async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        UnityPackageToolApp globalOptions = GetGlobalOptions(app);

        string workingDirectory = globalOptions.WorkingDirectory;
        LogLevel logLevel = globalOptions.LogLevel;
        string? logFile = globalOptions.LogFile;

        using var _ = new DirectoryScope(workingDirectory);

        using var factory = new LoggerFactory();
        factory.AddProvider(new AnsiConsoleLoggerProvider(logLevel));
        if (logFile is not null)
        {
            string logFilePath = Path.GetFullPath(logFile);
            factory.AddProvider(new FileLoggerProvider(logFilePath, logLevel));
        }

        var logger = SimpleLogger.CreateLogger(factory, AssemblyHelpers.GetAssemblyName());
        var fileManager = new FileManager(logger);

        try
        {
            ValidateCommand();
            await ExecuteAsync(fileManager, logger);
        }
        catch (CommandException e)
        {
            logger.Error(e.Message);
        }
        catch (Exception e)
        {
            logger.Error("Internal error", e);
        }

        return logger.HasErrors ? 1 : 0;
    }

    protected virtual void ValidateCommand() { }

    protected abstract ValueTask ExecuteAsync(FileManager fileManager, SimpleLogger logger);

    static UnityPackageToolApp GetGlobalOptions(CommandLineApplication app)
    {
        while (app.Parent is not null)
        {
            if (app.Parent is CommandLineApplication<UnityPackageToolApp> parent)
                return parent.Model;
            app = app.Parent;
        }

        throw new Exception("Unexpected error.");
    }
}
