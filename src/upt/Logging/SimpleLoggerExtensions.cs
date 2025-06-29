// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using System;

static class SimpleLoggerExtensions
{
    public static void Debug(this SimpleLogger log, string message) =>
        log.Log(LogLevel.Debug, null, message);

    public static void Info(this SimpleLogger log, string message) =>
        log.Log(LogLevel.Information, null, message);

    public static void Warn(this SimpleLogger log, string message) =>
        log.Log(LogLevel.Warning, null, message);

    public static void Error(this SimpleLogger log, string message) =>
        log.Log(LogLevel.Error, null, message);

    public static void Error(this SimpleLogger log, string message, Exception exception) =>
        log.Log(LogLevel.Error, exception, message);
}
