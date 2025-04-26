// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using static UnityPackageTool.Logging.FileFormatter;

namespace UnityPackageTool.Logging;

class FileLogger(string name, StreamWriter streamWriter, FileLoggerOptions options) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            var logEntry = new LogEntry<TState>(logLevel, name, eventId, state, exception, formatter);
            Write(streamWriter, logEntry);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= options.LogLevel;

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
        => NullScope.Instance;

    sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}
