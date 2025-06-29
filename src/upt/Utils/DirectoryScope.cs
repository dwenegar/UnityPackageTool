// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.IO;

namespace UnityPackageTool.Utils;

sealed class DirectoryScope : IDisposable
{
    readonly string m_PreviousDirectory;

    public DirectoryScope(string directory)
    {
        m_PreviousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(directory);
    }

    public void Dispose() => Directory.SetCurrentDirectory(m_PreviousDirectory);
}
