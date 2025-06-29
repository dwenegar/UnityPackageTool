// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;
using System;
using System.Text;

namespace UnityPackageTool.Logging;

class AnsiConsoleLogger(string name, LogLevel level) : ILogger
{
    readonly StringBuilder m_Buffer = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            var logEntry = new LogEntry<TState>(logLevel, name, eventId, state, exception, formatter);
            AnsiConsoleFormatter.Format(logEntry, m_Buffer);
            AnsiConsole.MarkupLine(m_Buffer.ToString());
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= level;

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
        => NullScope.Instance;

    sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}
