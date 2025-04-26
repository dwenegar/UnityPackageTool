// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UnityPackageTool.Utils;

sealed class FileManager(ISimpleLogger logger)
{
    static readonly JsonSerializerOptions k_JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new DependencyInfoConverter() }
    };

    public bool IsTestFolder(string path)
        => path.Contains("Tests", StringComparison.Ordinal);

    public bool IsPathIgnoredByUnity(string path)
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

    public async Task WriteJsonAsync<T>(string path, T value)
    {
        logger.Debug($"Writing '{path}'.");
        var info = new FileInfo(path);
        CreateDirectory(Path.GetDirectoryName(path));
        await using FileStream stream = info.OpenWrite();
        await JsonSerializer.SerializeAsync(stream, value, k_JsonSerializerOptions);
    }

    public async ValueTask<T?> ReadJsonAsync<T>(string path)
    {
        var info = new FileInfo(path);
        if (info.Exists)
        {
            logger.Debug($"Reading '{path}'.");
            await using FileStream stream = info.OpenRead();
            return await JsonSerializer.DeserializeAsync<T>(stream, k_JsonSerializerOptions);
        }

        return default;
    }

    public async Task WriteTextAsync(string path, string text)
    {
        logger.Debug($"Writing '{path}'.");
        CreateDirectory(Path.GetDirectoryName(path));
        await File.WriteAllTextAsync(path, text, Encoding.UTF8);
    }

    public async Task<string> ReadTextAsync(string path)
        => await File.ReadAllTextAsync(path, Encoding.UTF8);

    public void MoveFile(string filename, string srcDir, string dstDir)
        => MoveFile(Path.Combine(srcDir, filename), Path.Combine(dstDir, filename));

    public void MoveFile(string src, string dst)
    {
        if (File.Exists(src))
        {
            DeleteFile(dst);
            File.Move(src, dst);
        }

        void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    public async Task TryCopyFileAsync(string filename, string srcDir, string dstDir)
        => await TryCopyFileAsync(Path.Combine(srcDir, filename), Path.Combine(dstDir, filename));

    public async Task<bool> TryCopyFileAsync(string src, string dst)
    {
        if (IsPathIgnoredByUnity(src))
            return false;

        var srcInfo = new FileInfo(src);
        if (!srcInfo.Exists)
            return false;

        var dstInfo = new FileInfo(dst);
        logger.Debug($"Copying '{srcInfo.FullName}' to '{dstInfo.FullName}'.");

        if (dstInfo.Directory is { } dstDirectory)
            dstDirectory.Create();

        await using FileStream srcStream = srcInfo.OpenRead();
        await using FileStream dstStream = dstInfo.OpenWrite();
        await srcStream.CopyToAsync(dstStream);
        return true;
    }

    public async Task<int> CopyDirectoryAsync(string src, string dst, string filter)
        => await CopyDirectoryAsync(src, dst, filter, null);

    public async Task<int> CopyDirectoryAsync(string src, string dst, string filter, List<string>? files)
    {
        int count = 0;
        if (Directory.Exists(src))
        {
            foreach (string file in Directory.EnumerateFiles(src, filter, SearchOption.AllDirectories))
            {
                if (await TryCopyFileAsync(file, file.Replace(src, dst, StringComparison.Ordinal)))
                {
                    files?.Add(file);
                    count++;
                }
            }
        }

        return count;
    }

    public void RemoveIgnoredPaths(string rootPath)
    {
        foreach (string path in Directory.GetDirectories(rootPath, ".*", SearchOption.AllDirectories))
            DeleteDirectory(path);

        foreach (string path in Directory.GetDirectories(rootPath, "*~", SearchOption.AllDirectories))
            DeleteDirectory(path);
    }

    public void CreateDirectory(string? path)
    {
        logger.Debug($"Creating directory '{path}'.");
        if (path != null && !Directory.Exists(path))
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(path);
        }
    }

    public void DeleteDirectory(string path)
    {
        var info = new DirectoryInfo(path);
        if (info.Exists)
        {
            logger.Debug($"Removing directory '{path}'.");
            info.Delete(true);
        }
    }
}
