// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using System;

interface ISimpleLogger
{
    void Log(LogLevel level, Exception? exception, string? message, params object?[] args);
}
