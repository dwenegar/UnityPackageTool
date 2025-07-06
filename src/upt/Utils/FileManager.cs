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
using static UnityPackageTool.Utils.PathHelpers;

namespace UnityPackageTool.Utils;

sealed class FileManager(SimpleLogger logger)
{
    static readonly JsonSerializerOptions k_JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        NewLine = "\n",
        Converters = { new DependencyListConverter() }
    };

    public async ValueTask<PackageInfo?> ReadPackageJsonAsync()
        => await ReadJsonAsync<PackageInfo>("package.json");

    public async ValueTask WritePackageJsonAsync(PackageInfo package)
        => await WriteJsonAsync("package.json", package);

    public async ValueTask<T?> ReadJsonAsync<T>(string path)
    {
        path = Path.GetFullPath(path);

        logger.Debug($"Reading '{path}'.");
        if (File.Exists(path))
        {
            await using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<T>(stream, k_JsonSerializerOptions);
        }

        return default;
    }

    public async ValueTask WriteJsonAsync<T>(string path, T value)
    {
        path = Path.GetFullPath(path);

        logger.Debug($"Writing '{path}'.");
        EnsureDirectoryExists(path);
        await using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        await JsonSerializer.SerializeAsync(stream, value, k_JsonSerializerOptions);
    }

    public async ValueTask<string> ReadTextAsync(string path)
    {
        path = Path.GetFullPath(path);

        logger.Debug($"Reading '{path}'.");
        return await File.ReadAllTextAsync(path);
    }

    public async ValueTask WriteTextAsync(string path, string text)
    {
        path = Path.GetFullPath(path);

        logger.Debug($"Writing '{path}'.");
        EnsureDirectoryExists(path);
        await File.WriteAllTextAsync(path, text.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    public void CreateDirectory(string? path)
    {
        if (path is not null)
        {
            path = Path.GetFullPath(path);

            logger.Debug($"Creating directory '{path}'.");
            Directory.CreateDirectory(path);
        }
    }

    public void DeleteDirectory(string path)
    {
        path = Path.GetFullPath(path);

        if (Directory.Exists(path))
        {
            logger.Debug($"Removing directory '{path}'.");
            Directory.Delete(path, true);
        }
    }

    public async Task<int> CopyDirectoryAsync(string src, string dst, string filter)
        => await CopyDirectoryAsync(src, dst, filter, null);

    public async Task<int> CopyDirectoryAsync(string src, string dst, string filter, List<string>? files)
    {
        src = Path.GetFullPath(src);
        dst = Path.GetFullPath(dst);

        if (!Directory.Exists(src))
            return 0;

        int count = 0;
        foreach (string file in Directory.EnumerateFiles(src, filter, SearchOption.AllDirectories))
        {
            if (await TryCopyFileAsync(file, file.Replace(src, dst, StringComparison.Ordinal)))
            {
                files?.Add(file);
                count++;
            }
        }

        return count;
    }

    public void MoveFile(string filename, string srcDir, string dstDir)
        => MoveFile(Path.Combine(srcDir, filename), Path.Combine(dstDir, filename));

    public async Task TryCopyFileToDirectoryAsync(string path, string dst)
    {
        string filename = Path.GetFileName(path);
        await TryCopyFileAsync(path, Path.Combine(dst, filename));
    }

    public async ValueTask<bool> CopyAnyAsync(IEnumerable<string> filenames, string destination)
    {
        foreach (string filename in filenames)
        {
            if (await TryCopyFileAsync(filename, destination))
                return true;
        }

        return false;
    }

    public async ValueTask<bool> TryCopyFileAsync(string src, string dst)
    {
        src = Path.GetFullPath(src);
        dst = Path.GetFullPath(dst);

        logger.Debug($"Copying '{src}' to '{dst}'.");

        if (IsPathIgnoredByUnity(src))
            return false;

        if (!File.Exists(src))
            return false;

        EnsureDirectoryExists(dst);

        await using FileStream srcStream = File.OpenRead(src);
        await using FileStream dstStream = File.OpenWrite(dst);
        await srcStream.CopyToAsync(dstStream);
        return true;
    }

    void EnsureDirectoryExists(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (directory is not null)
            CreateDirectory(directory);
    }

    void MoveFile(string src, string dst)
    {
        src = Path.GetFullPath(src);
        dst = Path.GetFullPath(dst);

        logger.Debug($"Moving '{src}' to '{dst}'.");

        if (File.Exists(src))
            File.Move(src, dst, true);
    }
}
