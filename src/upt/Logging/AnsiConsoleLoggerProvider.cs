// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;

namespace UnityPackageTool.Logging;

sealed class AnsiConsoleLoggerProvider(LogLevel level) : ILoggerProvider
{
    public void Dispose() { }

    public ILogger CreateLogger(string name) => new AnsiConsoleLogger(name, level);
}
