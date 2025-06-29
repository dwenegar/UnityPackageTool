// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Text;

namespace UnityPackageTool.Logging;

static class AnsiConsoleFormatter
{
    public static void Format<TState>(in LogEntry<TState> logEntry, StringBuilder buffer)
    {
        buffer.Clear();
        buffer.Append(GetLogLevelString(logEntry.LogLevel));
        buffer.Append($": [[{logEntry.EventId.Id:0}]]");

        string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (message is { Length: > 0 })
            buffer.Append(' ').Append(EscapeMarkup(message));

        if (logEntry.Exception?.Message is { Length: > 0 })
            buffer.Append(' ').Append(EscapeMarkup(logEntry.Exception.Message));

        if (logEntry.Exception?.StackTrace is { Length: > 0 })
            buffer.AppendLine().Append(EscapeMarkup(logEntry.Exception?.StackTrace));

        static string? EscapeMarkup(string? text)
        {
            return string.IsNullOrEmpty(text)
                       ? text
                       : text.Replace("[", "[[", StringComparison.Ordinal).Replace("]", "]]", StringComparison.Ordinal);
        }
    }

    static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace       => "[grey]TRCE[/]",
        LogLevel.Debug       => "[white]DBUG[/]",
        LogLevel.Information => "[green]INFO[/]",
        LogLevel.Warning     => "[yellow]WARN[/]",
        LogLevel.Error       => "[red]FAIL[/]",
        LogLevel.Critical    => "[red]CRIT[/]",
        _                    => throw new ArgumentOutOfRangeException(nameof(logLevel))
    };
}
