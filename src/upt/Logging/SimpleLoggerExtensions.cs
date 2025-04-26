// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;

static class SimpleLoggerExtensions
{
    public static void Info(this ISimpleLogger log, string message) =>
        log.Log(LogLevel.Information, null, message, false);

    public static void Warn(this ISimpleLogger log, string message) =>
        log.Log(LogLevel.Warning, null, message, false);

    public static void Error(this ISimpleLogger log, string message) =>
        log.Log(LogLevel.Error, null, message, false);

    public static void Debug(this ISimpleLogger log, string message) =>
        log.Log(LogLevel.Debug, null, message, false);
}
