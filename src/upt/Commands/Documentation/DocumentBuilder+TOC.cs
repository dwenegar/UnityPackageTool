// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnityPackageTool.Commands.Documentation;

sealed partial class DocumentBuilder
{
    static async ValueTask<string?> GetTitleFromFileContentAsync(string path)
    {
        if (await FindFirstHeaderAsync(path) is { } header)
        {
            string title = header.TrimStart('#').TrimStart();
            if (!string.IsNullOrEmpty(title))
                return title;
        }

        return null;

        static async ValueTask<string?> FindFirstHeaderAsync(string path)
        {
            using StreamReader reader = File.OpenText(path);
            while (await reader.ReadLineAsync() is { } line)
            {
                ReadOnlySpan<char> span = line.AsSpan().TrimEnd();
                if (span.IsEmpty)
                    continue;

                if (span.StartsWith("# ", StringComparison.Ordinal))
                    return line;

                break;
            }

            return null;
        }
    }

    [Flags]
    enum TableOfContentsOptions
    {
        None = 0,
        Api = 1,
        Changelog = 2,
        License = 4
    }

    async ValueTask CreateTableOfContentsAsync(TableOfContentsOptions options, string? manualHomePage)
    {
        Logger.Debug("Creating the main table of contents");

        var toc = new TableOfContents();
        if (manualHomePage is not null)
            toc.AddItem("Manual", "manual/", manualHomePage);

        if ((options & TableOfContentsOptions.Api) != 0)
            toc.AddItem("API Documentation", "api/", "api/index.md");

        if ((options & TableOfContentsOptions.Changelog) != 0)
            toc.AddItem("Changelog", "changelog/", "changelog/CHANGELOG.md");

        if ((options & TableOfContentsOptions.License) != 0)
            toc.AddItem("License", "license/", "license/LICENSE.md");

        if (toc.Count > 0)
        {
            string dstPath = Path.Combine(m_BuildPath, "toc.yml");
            await FileManager.WriteTextAsync(dstPath, toc.ToString());
        }
    }

    async ValueTask<string?> CreateManualTableOfContentsAsync()
    {
        TopicInfo? topicInfo = await CreateTableOfContentsAsync(m_BuildManualPath);
        return topicInfo is null ? null : Path.GetRelativePath(m_BuildPath, topicInfo.TopicFilePath).Replace('\\', '/');
    }

    async ValueTask<TopicInfo?> CreateTableOfContentsAsync(string directoryPath)
    {
        string[] topicFileNames = ["overview.md", "introduction.md", "index.md"];
        string? topicFilePath = topicFileNames.Select(x => Path.Combine(directoryPath, x))
                                              .FirstOrDefault(x => File.Exists(x));
        if (topicFilePath is null)
        {
            Logger.Warn($"No valid topic file was found in the directory '{directoryPath}'.");
            return null;
        }

        if (await GetTitleFromFileContentAsync(topicFilePath) is { } topicTitle)
        {
            string tocPath = Path.Combine(directoryPath, "toc.yml");
            if (!File.Exists(tocPath))
            {
                TableOfContents toc = await BuildTableOfContents(directoryPath);
                if (toc.Count == 0)
                    return null;

                await FileManager.WriteTextAsync(tocPath, toc.ToString());
            }

            return new TopicInfo(topicTitle, tocPath, topicFilePath);
        }

        Logger.Warn($"Title could not be extracted from the file at '{topicFilePath}'.");
        return null;
    }

    async ValueTask<TableOfContents> BuildTableOfContents(string directoryPath)
    {
        var toc = new TableOfContents();
        foreach (string path in Directory.EnumerateFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly))
        {
            if (await GetTitleFromFileContentAsync(path) is { } title)
            {
                int index = GetIndexFromFilePath(path);
                string href = Path.GetRelativePath(directoryPath, path);
                toc.AddItem(index, title, href);
                continue;
            }

            Logger.Warn($"Title could not be extracted from the file at {path}. Please ensure the file contains a valid title.");
        }

        foreach (string path in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
        {
            TopicInfo? topicInfo = await CreateTableOfContentsAsync(path);
            if (topicInfo is not null)
            {
                string topicHref = Path.GetRelativePath(directoryPath, topicInfo.TopicFilePath).Replace('\\', '/');
                string tocHref = Path.GetRelativePath(directoryPath, topicInfo.TocPath).Replace('\\', '/');
                toc.AddToc(topicInfo.Title, tocHref, topicHref);
            }
        }

        return toc;

        static int GetIndexFromFilePath(ReadOnlySpan<char> path)
        {
            int p = path.IndexOf('-');
            if (p != -1 && int.TryParse(path[..p], out int result))
                return result;
            return -1;
        }
    }

    record TopicInfo(string Title, string TocPath, string TopicFilePath);
}
