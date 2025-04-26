// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;

namespace UnityPackageTool.Logging;

class FileLoggerOptions
{
    public required string FilePath { get; init; }
    public LogLevel LogLevel { get; init; } = LogLevel.Information;
}
