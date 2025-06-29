// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

#pragma warning disable CA2254 // Template should be a static expression

using Microsoft.Extensions.Logging;
using System;
using System.Threading;

sealed class SimpleLogger(ILogger logger)
{
    int m_EventId;

    public bool HasErrors { get; private set; }

    public static SimpleLogger CreateLogger(ILoggerFactory factory, string appName)
        => new(factory.CreateLogger(appName));

    public void Log(LogLevel level, Exception? exception, string message)
    {
        HasErrors |= level is LogLevel.Error or LogLevel.Critical;
        int id = Interlocked.Increment(ref m_EventId);
        logger.Log(level, id, exception, message);
    }
}
