// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Globalization;
using System.IO;

namespace UnityPackageTool.Logging;

static class FileFormatter
{
    public static void Write<TState>(TextWriter textWriter, in LogEntry<TState> logEntry)
    {
        textWriter.Write(DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss.fff "));
        textWriter.Write(GetLogLevelString(logEntry.LogLevel));
        textWriter.Write(": ");
        textWriter.Write(logEntry.Category);
        textWriter.Write('[');
        textWriter.Write(logEntry.EventId.Id.ToString("###0", CultureInfo.InvariantCulture));
        textWriter.Write(']');

        string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (message is { Length: > 0 })
        {
            textWriter.Write(' ');
            textWriter.Write(message);
        }

        if (logEntry.Exception?.Message is { Length: > 0 })
        {
            textWriter.Write(' ');
            textWriter.Write(logEntry.Exception.Message);
        }

        if (logEntry.Exception?.StackTrace is not null)
            textWriter.Write(logEntry.Exception.StackTrace);

        textWriter.Write(Environment.NewLine);
    }

    static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace       => "trce",
        LogLevel.Debug       => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning     => "warn",
        LogLevel.Error       => "fail",
        LogLevel.Critical    => "crit",
        _                    => throw new ArgumentOutOfRangeException(nameof(logLevel))
    };
}
