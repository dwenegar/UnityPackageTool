// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityPackageTool.Logging;

sealed class FileLoggerProvider(FileLoggerOptions options) : ILoggerProvider
{
    static readonly Encoding k_Utf8NoBom = new UTF8Encoding(false);

    List<StreamWriter>? m_StreamWriters;

    public void Dispose() => m_StreamWriters?.ForEach(x => x.Dispose());

    public ILogger CreateLogger(string name)
    {
        m_StreamWriters ??= [];
        var streamWriter = new StreamWriter(options.FilePath, false, k_Utf8NoBom);
        m_StreamWriters.Add(streamWriter);
        return new FileLogger(name, streamWriter, options);
    }
}
