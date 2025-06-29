// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using CliWrap;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace UnityPackageTool.Commands.Documentation;

sealed class DocFx(string path)
{
    public string? WorkingDirectory { get; init; }

    public async Task<string> RunAsync(string arguments, SimpleLogger? logger = null)
    {
        var buffer = new StringBuilder();
        var handleOutput = PipeTarget.ToDelegate(x => HandleOutput(x, buffer, logger));

        string workingDirectory = WorkingDirectory ?? Environment.CurrentDirectory;
        CommandResult result = await Cli.Wrap(path)
                                        .WithArguments(arguments)
                                        .WithWorkingDirectory(workingDirectory)
                                        .WithStandardOutputPipe(handleOutput)
                                        .ExecuteAsync();

        if (result.ExitCode != 0)
            throw new Exception($"Failed to run {path} {arguments}");

        return buffer.ToString();

        static void HandleOutput(string line, StringBuilder buffer, SimpleLogger? logger)
        {
            if (line.Contains("CS0246", StringComparison.Ordinal)    // The type or namespace name could not be found
                || line.Contains("CS0103", StringComparison.Ordinal) // The name 'identifier' does not exist in the current context
                || line.Contains("InvalidCref", StringComparison.Ordinal))
                return;

            buffer.AppendLine(line);
            if (logger != null)
            {
                LogLevel level = LogLevel.Debug;
                if (line.StartsWith("warning:", StringComparison.Ordinal))
                {
                    level = LogLevel.Warning;
                    line = line[9..];
                }
                else if (line.StartsWith("error:", StringComparison.Ordinal))
                {
                    level = LogLevel.Error;
                    line = line[9..];
                }

                logger.Log(level, null, line);
            }
        }
    }
}
