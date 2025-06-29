// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using GlobExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPackageTool.Commands.Dependencies;
using static UnityPackageTool.Utils.PathHelpers;

namespace UnityPackageTool.Commands.Documentation;

sealed partial class DocumentBuilder
{
    static async ValueTask<Dictionary<string, string>?> ReadResponseFileAsync(string asmdefFolder)
    {
        string responseFilePath = Path.Combine(asmdefFolder, "csc.rsp");
        if (!File.Exists(responseFilePath))
            return null;

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await foreach (string line in File.ReadLinesAsync(responseFilePath))
        {
            if (line[0] is not '-')
                continue;

            int i = line.IndexOf(':', StringComparison.Ordinal);
            if (i < 0)
                continue;

            string key = line.Substring(1, i - 1);
            string value = line[(i + 1)..];
            result[key] = value;
        }

        return result;
    }

    async ValueTask CreateDocFxProject()
    {
        FileManager.CreateDirectory(m_BuildSourcesPath);

        DocumentationConfiguration documentationConfig = await ReadDocumentationConfiguration();

        TableOfContentsOptions tableOfContentsOptions = TableOfContentsOptions.None;

        await CreateDefaultApiIndexFileAsync();
        await CreateDefaultFilterFileAsync();
        await CreateDefaultIndexFileAsync();
        await CopySourceFilesAsync(documentationConfig);
        if (await CopyLicenses())
            tableOfContentsOptions |= TableOfContentsOptions.License;
        if (await CopyChangelog())
            tableOfContentsOptions |= TableOfContentsOptions.Changelog;
        await CopyManualFiles();
        string? manualHomePage = await CreateManualTableOfContentsAsync();
        await CreateTableOfContentsAsync(tableOfContentsOptions, manualHomePage);

        var docFxConfiguration = DocFxConfiguration.CreateConfig(Package);
        await foreach (string projectFile in CreateCSharpProjects(Package, documentationConfig))
        {
            docFxConfiguration.Metadata[0].Src[0].Files.Add(projectFile);
            docFxConfiguration.Metadata[0].Src[0].Src = m_BuildSourcesPath;
        }

        await WriteDocFxJsonAsync(docFxConfiguration);
    }

    async ValueTask<DocumentationConfiguration> ReadDocumentationConfiguration()
    {
        string configJsonPath = Path.Combine(m_SourcePath, "config.json");
        return await FileManager.ReadJsonAsync<DocumentationConfiguration>(configJsonPath)
               ?? new DocumentationConfiguration();
    }

    async ValueTask CreateDefaultIndexFileAsync()
    {
        const string Content = """
                               # Home Page

                               This is the home page for this package.
                               """;
        string path = Path.Combine(m_BuildPath, "index.md");
        await FileManager.WriteTextAsync(path, Content);
    }

    async ValueTask CreateDefaultApiIndexFileAsync()
    {
        const string Content = """
                               # API Documentation

                               This is the documentation for the Scripting APIs of this package
                               """;
        string path = Path.Combine(m_BuildPath, "api", "index.md");
        await FileManager.WriteTextAsync(path, Content);
    }

    async ValueTask CreateDefaultFilterFileAsync()
    {
        const string Content = """
                               apiRules:
                                 - exclude:
                                    # inherited Object methods
                                    uidRegex: ^System\.Object\..*$
                                    type: Method
                                 - exclude:
                                    # mentioning types from System.* namespace
                                    uidRegex: ^System\..*$
                                    type: Type
                                 - exclude:
                                    hasAttribute:
                                       uid: System.ObsoleteAttribute
                                    type: Member
                                 - exclude:
                                    hasAttribute:
                                       uid: System.ObsoleteAttribute
                                    type: Type
                                 - exclude:
                                    hasAttribute:
                                       uid: System.ComponentModel.EditorBrowsableAttribute
                                       ctorArguments:
                                          - System.ComponentModel.EditorBrowsableState.Never
                                 - exclude:
                                    hasAttribute:
                                       uid: NUnit.Framework.TestAttribute
                                 - exclude:
                                    hasAttribute:
                                       uid: NUnit.Framework.TestCaseAttribute
                                 - exclude:
                                    hasAttribute:
                                       uid: NUnit.Framework.TestCaseAttribute
                                 - exclude:
                                    uidRegex: ..*\.Tests$
                                    type: Namespace
                               """;
        string path = Path.Combine(m_BuildPath, "filter.yml");
        await FileManager.WriteTextAsync(path, Content);
    }

    async IAsyncEnumerable<string> CreateCSharpProjects(PackageInfo packageInfo, DocumentationConfiguration documentationConfig)
    {
        string defineConstants = GetDefineConstants();
        string targetFrameWork = GetTargetFramework();
        await foreach (ProjectInfo projectInfo in GetProjectInfos())
        {
            if (projectInfo.FileManager.Length > 0)
                yield return await CreateCSharpProject(projectInfo);
        }

        async ValueTask<string> CreateCSharpProject(ProjectInfo projectInfo)
        {
            Logger.Debug($"Creating the C# project {projectInfo.Name}");

            var builder = new CsProjBuilder();
            using (CsProjBuilder.Group properties = builder.BeginGroup("PropertyGroup"))
            {
                properties.AddProperty("DefineConstants", defineConstants);
                properties.AddProperty("TargetFramework", targetFrameWork);
                properties.AddProperty("AllowUnsafeBlocks", projectInfo.AllowUnsafeBlocks);
                properties.AddProperty("LangVersion", projectInfo.LangVersion ?? "10");
                properties.AddProperty("EnableDefaultCompileItems", false);
                if (projectInfo.Nullable)
                    properties.AddProperty("Nullable", "enable");

                if (projectInfo.NoWarn is { } noWarn)
                    properties.AddProperty("NoWarn", noWarn);
            }

            using (CsProjBuilder.Group items = builder.BeginGroup("ItemGroup"))
            {
                items.AddItem("Reference", "System");
                foreach (string file in projectInfo.FileManager)
                    items.AddItem("Compile", file);
            }

            string csprojPath = $"{projectInfo.Name}.csproj";
            await FileManager.WriteTextAsync(Path.Combine(m_BuildSourcesPath, csprojPath), builder.ToString());
            return csprojPath;
        }

        string GetDefineConstants()
        {
            var sb = new StringBuilder();
            sb.Append(k_PackageDocsGenerationDefine);
            foreach (string defineConstant in documentationConfig.DefineConstants)
                sb.Append(';').Append(defineConstant);

            return sb.ToString();
        }

        string GetTargetFramework()
        {
            Version.TryParse(packageInfo.Unity, out Version? version);
            return version switch
            {
                { Major: > 2021 } or { Major: 2021, Minor: >= 2 } => "netstandard2.1",
                { Major: >= 2018 }                                => "netstandard2.0",
                _                                                 => "net48"
            };
        }
    }

    async IAsyncEnumerable<ProjectInfo> GetProjectInfos()
    {
        string[] asmdefFiles = Directory.GetFiles(m_BuildSourcesPath, "*.asmdef", SearchOption.AllDirectories);
        Array.Sort(asmdefFiles, CompareAsmDefFilenames);

        var allCompilableFiles = new HashSet<string>(StringComparer.Ordinal);

        foreach (string asmdefFile in asmdefFiles)
        {
            string asmdefFolder = Path.GetDirectoryName(asmdefFile)
                                  ?? throw new Exception($"Invalid path {asmdefFile}");

            AsmDefInfo asmdef = await FileManager.ReadJsonAsync<AsmDefInfo>(asmdefFile)
                                ?? throw new Exception($"Failed to load {asmdefFile}.");

            string[] files = Directory.GetFiles(asmdefFolder, "*.cs", SearchOption.AllDirectories)
                                      .Where(x => allCompilableFiles.Add(x))
                                      .ToArray();

            var projectInfo = new ProjectInfo
            {
                Name = asmdef.Name,
                FileManager = files,
                AllowUnsafeBlocks = asmdef.AllowUnsafeCode
            };

            if (await ReadResponseFileAsync(asmdefFolder) is { } rspInfo)
            {
                if (rspInfo.TryGetValue("nowarn", out string? noWarn))
                    projectInfo.NoWarn = noWarn;

                if (rspInfo.TryGetValue("nullable", out string? nullable))
                    projectInfo.Nullable = nullable == "enable";

                if (rspInfo.TryGetValue("langVersion", out string? langVersion))
                    projectInfo.LangVersion = langVersion;
            }

            yield return projectInfo;
        }

        static int CompareAsmDefFilenames(string x, string y)
        {
            int nx = x.Length, ny = y.Length;
            int i = 0;
            for (; i < nx && i < ny; i++)
            {
                char cx = x[i], cy = y[i];
                if (cx == cy)
                    continue;
                if (cx == '.' && cy != '.')
                    return 1;
                if (cx != '.' && cy == '.')
                    return -1;
                return cx < cy ? 1 : -1;
            }

            return nx == ny ? 0 : nx < ny ? 1 : -1;
        }
    }

    async ValueTask WriteDocFxJsonAsync(DocFxConfiguration docFxConfiguration)
    {
        string dstPath = Path.Combine(m_BuildPath, "docfx.json");
        await FileManager.WriteJsonAsync(dstPath, docFxConfiguration);
    }

    async ValueTask<bool> CopySourceFilesAsync(DocumentationConfiguration documentationConfig)
    {
        int totalSourceFileCount = 0;
        foreach (string sourcePath in ResolveSourceDirectories().Where(x => !IsPathIgnoredByUnity(x) && !IsTestFolder(x)))
        {
            string relativePath = Path.GetRelativePath(m_PackagePath, sourcePath);
            string destinationPath = Path.Combine(m_BuildSourcesPath, relativePath);
            await FileManager.CopyDirectoryAsync(sourcePath, destinationPath, "*.asmdef");
            await FileManager.CopyDirectoryAsync(sourcePath, destinationPath, "*.rsp");
            totalSourceFileCount += await FileManager.CopyDirectoryAsync(sourcePath, destinationPath, "*.cs");
        }

        return totalSourceFileCount > 0;

        IEnumerable<string> ResolveSourceDirectories()
        {
            return documentationConfig.Sources.Select(x => Glob.Directories(m_PackagePath, x))
                                      .SelectMany(x => x)
                                      .Distinct()
                                      .OrderBy(x => x);
        }
    }

    async ValueTask CopyManualFiles()
    {
        var copiedFiles = new List<string>();
        await FileManager.CopyDirectoryAsync(m_SourcePath, m_BuildManualPath, "*.*", copiedFiles);
        FileManager.MoveFile("favicon.ico", m_BuildManualPath, m_BuildPath);
        FileManager.MoveFile("logo.svg", m_BuildManualPath, m_BuildPath);
        FileManager.MoveFile("filter.yml", m_BuildManualPath, m_BuildPath);
        FileManager.MoveFile("projectMetadata.yml", m_BuildManualPath, m_BuildPath);
    }

    async ValueTask<bool> CopyLicenses()
    {
        var toc = new TableOfContents();

        string? licenseIndexFileName = null;

        string[] licenseFiles = ["LICENSE.md", "LICENSE.txt", "LICENSE", "License.md", "License.txt", "License"];
        if (await FileManager.CopyAnyAsync(licenseFiles, Path.Combine(m_BuildPath, "licenses", "LICENSE.md")))
        {
            toc.AddItem("License", "LICENSE.md");
            licenseIndexFileName = "LICENSE.html";
        }

        string[] thirdPartyLicenseFiles = ["Third Party Notices.md", "ThirdPartyNotices.md", "Third Party Notices.txt", "ThirdPartyNotices.txt", "Third Party Notices", "ThirdPartyNotices"];
        if (await FileManager.CopyAnyAsync(thirdPartyLicenseFiles, Path.Combine(m_BuildPath, "licenses", "ThirdPartyNotices.md")))
        {
            toc.AddItem("Third Party Notices", "ThirdPartyNotices.md");
            licenseIndexFileName ??= "ThirdPartyNotices.html";
        }

        if (toc.Count == 0)
        {
            Logger.Warn("No license found.");
            return false;
        }

        string destinationFolderPath = Path.Combine(m_BuildPath, "license");
        Directory.CreateDirectory(destinationFolderPath);

        string indexFile = Path.Combine(destinationFolderPath, "index.md");
        await FileManager.WriteTextAsync(indexFile, $"<script>window.location.replace('{licenseIndexFileName!}')</script>");

        string tocFilePath = Path.Combine(destinationFolderPath, "toc.yml");
        await FileManager.WriteTextAsync(tocFilePath, toc.ToString());
        return true;
    }

    async ValueTask<bool> CopyChangelog()
    {
        string[] changelogFileNames = ["CHANGELOG.md", "CHANGELOG.txt", "CHANGELOG", "Changelog.md", "Changelog.txt", "Changelog", "ChangeLog.md", "ChangeLog.txt", "ChangeLog"];
        if (!await FileManager.CopyAnyAsync(changelogFileNames, Path.Combine(m_BuildPath, "changelog", "CHANGELOG.md")))
        {
            Logger.Warn("No changelog found.");
            return false;
        }

        string indexFile = Path.Combine(m_BuildPath, "changelog", "index.md");
        await FileManager.WriteTextAsync(indexFile, "<script>window.location.replace('CHANGELOG.html')</script>");

        const string TocContent = """
                                  - name: Changelog
                                    href: CHANGELOG.md
                                  """;
        string tocFile = Path.Combine(m_BuildPath, "changelog", "toc.yml");
        await FileManager.WriteTextAsync(tocFile, TocContent);
        return true;
    }

    struct ProjectInfo
    {
        public string Name;
        public bool AllowUnsafeBlocks;
        public string? NoWarn;
        public bool Nullable;
        public string? LangVersion;
        public string[] FileManager;
    }

    sealed class CsProjBuilder
    {
        readonly StringBuilder m_Buffer = new(512);
        int m_IndentLength;

        public CsProjBuilder()
        {
            m_Buffer.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            m_IndentLength += 2;
        }

        public Group BeginGroup(string name)
            => new(name, this);

        public override string ToString()
        {
            m_Buffer.AppendLine("</Project>");
            return m_Buffer.ToString();
        }

        void AppendIndented(string text)
            => m_Buffer.Append(' ', m_IndentLength).AppendLine(text);

        internal class Group : IDisposable
        {
            readonly CsProjBuilder m_Builder;
            readonly string m_Name;

            public Group(string name, CsProjBuilder builder)
            {
                m_Name = name;
                m_Builder = builder;
                m_Builder.AppendIndented($"<{name}>");
                m_Builder.m_IndentLength += 2;
            }

            public void Dispose()
            {
                m_Builder.m_IndentLength -= 2;
                m_Builder.AppendIndented($"</{m_Name}>");
            }

            public void AddProperty(string name, object value)
                => m_Builder.AppendIndented($"<{name}>{value}</{name}>");

            public void AddItem(string name, string value)
                => m_Builder.AppendIndented($"<{name} Include=\"{value}\" />");
        }
    }
}
