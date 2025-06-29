// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.IO;

namespace UnityPackageTool.Utils;

static class PathHelpers
{
    public static int ComparePaths(ReadOnlySpan<char> p0, ReadOnlySpan<char> p1)
    {
        int i = 0;
        while (i < p0.Length && i < p1.Length)
        {
            char c0 = p0[i], c1 = p1[i];
            if (c0 != c1 && (!IsDirSep(c0) || !IsDirSep(c1)))
            {
                if (IsDirSep(c0))
                    return -1;
                if (IsDirSep(c1))
                    return 1;
                break;
            }

            i++;
        }


        while (i < p0.Length && i < p1.Length)
        {
            char c0 = p0[i], c1 = p1[i];
            if (IsDirSep(c0))
                return -1;
            if (IsDirSep(c1))
                return 1;
            i++;
        }

        return 0;

        static bool IsDirSep(char c)
        {
            return c is '\\' or '/';
        }
    }

    public static bool IsTestFolder(string path)
    {
        int index = path.IndexOf("Tests", StringComparison.OrdinalIgnoreCase);
        return index >= 1
               && path[index - 1] == Path.DirectorySeparatorChar
               && (index + 5 == path.Length || path[index + 5] == Path.DirectorySeparatorChar);
    }

    public static bool IsPathIgnoredByUnity(string path)
    {
        if (path[0] == '.' || path[^1] == '~')
            return true;

        for (int i = 1, n = path.Length - 1; i < n; i++)
        {
            if (path[i] is '/' or '\\' && (path[i - 1] == '~' || path[i + 1] == '.'))
                return true;
        }

        return false;
    }
}
