// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityPackageTool.Logging;

sealed class FileLoggerProvider(string filePath, LogLevel level) : ILoggerProvider
{
    List<StreamWriter>? m_StreamWriters;

    public void Dispose() => m_StreamWriters?.ForEach(x => x.Dispose());

    public ILogger CreateLogger(string name)
    {
        m_StreamWriters ??= [];
        var utf8NoBom = new UTF8Encoding(false);
        var streamWriter = new StreamWriter(filePath, false, utf8NoBom);
        m_StreamWriters.Add(streamWriter);
        return new FileLogger(name, streamWriter, level);
    }
}
