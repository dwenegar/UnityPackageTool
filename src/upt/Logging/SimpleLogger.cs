// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

#pragma warning disable CA2254 // Template should be a static expression

using Microsoft.Extensions.Logging;
using System;
using System.Threading;

sealed class SimpleLogger : ISimpleLogger
{
    readonly ILogger m_Logger;
    int m_EventId;

    SimpleLogger(ILogger logger) => m_Logger = logger;

    public static ISimpleLogger CreateLogger(ILoggerFactory factory, string appName)
        => new SimpleLogger(factory.CreateLogger(appName));

    public void Log(LogLevel level, Exception? exception, string? message, params object?[] args)
    {
        int id = Interlocked.Increment(ref m_EventId);
        m_Logger.Log(level, id, exception, message, args);
    }
}
