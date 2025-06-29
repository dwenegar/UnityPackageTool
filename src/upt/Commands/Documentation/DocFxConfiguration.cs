// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityPackageTool.Commands.Dependencies;

using FileMetadataPairs = Dictionary<string, object?>;

static class FileMetadataKeys
{
    public const string DisableToc = "_disableToc";
    public const string NoIndex = "_noindex";
}

static class GlobalMetadataKeys
{
    public const string AppLogoPath = "_appLogoPath";
    public const string AppTitle = "_appTitle";
    public const string CurrentYear = "_currentYear";
    public const string CustomCopyrightNotice = "_customCopyrightNotice";
    public const string DisableToc = "_disableToc";
    public const string EnableSearch = "_enableSearch";
    public const string GeneratedOn = "_generatedOn";
    public const string ImageZoomThreshold = "_imageZoomThreshold";
    public const string NoIndex = "_noIndex";
    public const string PackageName = "_packageName";
    public const string PackageVersion = "_packageVersion";
    public const string ValueLabel = "_valueLabel";

    public const string EnableNewTab = "enableNewTab";
    public const string EnableTocForManual = "enableTocForManual";
    public const string ShowScriptRef = "showScriptRef";
    public const string RenameSamplesFolder = "renameSamplesFolder";
    public const string HideGlobalNamespace = "hideGlobalNamespace";
    public const string MemberLayout = "memberLayout";
    public const string XRef = "xref";

    public static bool IsProtectedKey(string key) => key switch
    {
        EnableSearch   => true,
        AppLogoPath    => true,
        DisableToc     => true,
        PackageVersion => true,
        PackageName    => true,
        _              => false
    };
}

[Serializable]
sealed class DocFxConfiguration
{
    public List<MetadataConfig> Metadata { get; set; } = [];
    public BuildConfig Build { get; set; } = new();

    public static DocFxConfiguration CreateConfig(PackageInfo package) => new()
    {
        Metadata =
        [
            new MetadataConfig
            {
                Src = [new FileMapping([], ["**/obj/**", "**/bin/**", "_site/**"])],
                Dest = "api",
                Filter = "filter.yml",
                Force = true
            }
        ],
        Build = new BuildConfig
        {
            Content =
            [
                new FileMapping(["api/**.yml", "api/index.md"]),
                new FileMapping(["manual/**.md", "manual/**/toc.yml"]),
                new FileMapping(["changelog/**.md", "changelog/**/toc.yml"]),
                new FileMapping(["license/**.md", "license/**/toc.yml"]),
                new FileMapping(["*.md", "toc.yml"], ["obj/**", "_site/**"])
            ],
            Resource =
            {
                new FileMapping(["images/**", "logo.svg"], ["obj/**", "_site/**"])
            },
            Overwrite =
            {
                new FileMapping(["apidoc/**.md"], ["obj/**", "_site/**"])
            },
            Template = ["default", "modern"],
            NoLangKeyword = false,
            KeepFileLink = false,
            Dest = "_site",
            GlobalMetadata = new Dictionary<string, object>
            {
                { GlobalMetadataKeys.AppTitle, package.DisplayName ?? package.Name },
                { GlobalMetadataKeys.PackageName, package.Name },
                { GlobalMetadataKeys.PackageVersion, package.Version },
                { GlobalMetadataKeys.AppLogoPath, "logo.svg" },
                { GlobalMetadataKeys.CurrentYear, DateTime.Today.Year.ToString("D", CultureInfo.InvariantCulture) },
                { GlobalMetadataKeys.CustomCopyrightNotice, string.Empty },
                { GlobalMetadataKeys.DisableToc, false },
                { GlobalMetadataKeys.EnableNewTab, true },
                { GlobalMetadataKeys.EnableSearch, true },
                { GlobalMetadataKeys.GeneratedOn, DateTime.Today.ToString("D", CultureInfo.InvariantCulture) },
                { GlobalMetadataKeys.ImageZoomThreshold, 1200 },
                { GlobalMetadataKeys.NoIndex, false },
                { GlobalMetadataKeys.ValueLabel, "Value" },
                { GlobalMetadataKeys.EnableTocForManual, false },
                { GlobalMetadataKeys.RenameSamplesFolder, true },
                { GlobalMetadataKeys.ShowScriptRef, true }
            },
            FileMetadata = new Dictionary<string, FileMetadataPairs>
            {
                {
                    FileMetadataKeys.DisableToc, new FileMetadataPairs
                    {
                        ["changelog/*.md"] = false,
                        ["license/*.md"] = false
                    }
                },
                {
                    FileMetadataKeys.NoIndex, new FileMetadataPairs
                    {
                        ["changelog/*.md"] = false,
                        ["license/*.md"] = false
                    }
                }
            }
        }
    };

    [Serializable]
    internal class MetadataConfig
    {
        public List<FileMapping> Src { get; set; } = [];
        public string Dest { get; set; } = string.Empty;
        public bool DisableGitFeatures { get; set; }
        public bool DisableDefaultFilter { get; set; }
        public bool AllowCompilationErrors { get; set; } = true;
        public string Filter { get; set; } = string.Empty;
        public bool Force { get; set; }
    }

    [Serializable]
    internal class BuildConfig
    {
        public List<FileMapping> Content { get; set; } = [];
        public List<FileMapping> Resource { get; } = [];
        public List<FileMapping> Overwrite { get; set; } = [];
        public List<string> Xref { get; set; } = [];
        public string Dest { get; set; } = string.Empty;
        public Dictionary<string, object> GlobalMetadata { get; set; } = new();
        public Dictionary<string, FileMetadataPairs> FileMetadata { get; set; } = new();
        public List<string> Template { get; set; } = [];
        public List<string> PostProcessors { get; set; } = [];
        public string MarkdownEngineName { get; set; } = "markdig";
        public bool DisableGitFeatures { get; set; }
        public bool NoLangKeyword { get; set; }
        public bool KeepFileLink { get; set; }
    }

    [Serializable]
    internal class FileMapping(List<string> files, List<string> excludes)
    {
        public FileMapping(List<string> files)
            : this(files, []) { }

        public List<string> Files { get; set; } = files;
        public List<string> Excludes { get; init; } = excludes;
        public string? Src { get; set; }
        public string? Dest { get; set; }
    }

    [Serializable]
    internal class FileMetadataPair(string glob, object? value)
    {
        public string Glob { get; set; } = glob;
        public object? Value { get; set; } = value;
    }
}
