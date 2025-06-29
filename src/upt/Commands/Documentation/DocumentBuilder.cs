// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.Documentation;

sealed partial class DocumentBuilder
{
    const string k_PackageDocsGenerationDefine = "PACKAGE_DOCS_GENERATION";

    readonly string m_PackagePath = Directory.GetCurrentDirectory();

    readonly string m_SourcePath;
    readonly string m_BuildPath;
    readonly string m_OutputPath;
    readonly string m_BuildManualPath;
    readonly string m_BuildSourcesPath;

    public DocumentBuilder(string sourcePath, string buildPath, string outputPath)
    {
        m_SourcePath = sourcePath;
        m_BuildPath = buildPath;
        m_OutputPath = outputPath;
        m_BuildManualPath = Path.Combine(m_BuildPath, "manual");
        m_BuildSourcesPath = Path.Combine(m_BuildPath, "sources");
    }

    public required PackageInfo Package { get; init; }

    public required string DocFxPath { get; init; }

    public required bool KeepBuildFolder { get; init; }
    public required FileManager FileManager { get; init; }
    public required SimpleLogger Logger { get; init; }
    public bool Force { get; set; }

    public async ValueTask BuildAsync()
    {
        CleanBuildPath();
        InitializeOutputPath();

        await CreateDocFxProject();
        await RunDocFxAsync();
        await CopyFilesToOutputFolderAsync();

        if (!KeepBuildFolder)
            FileManager.DeleteDirectory(m_BuildPath);
    }

    void CleanBuildPath()
    {
        FileManager.DeleteDirectory(m_BuildPath);
        FileManager.CreateDirectory(m_BuildPath);
    }

    void InitializeOutputPath()
    {
        if (Directory.Exists(m_OutputPath))
        {
            if (!Force)
                throw new CommandException($"Cannot create the directory '{m_OutputPath}'. The directory already exists. Use option --force to overwrite it.");
            FileManager.DeleteDirectory(m_OutputPath);
        }

        FileManager.CreateDirectory(m_OutputPath);
    }

    async ValueTask CopyFilesToOutputFolderAsync()
    {
        string sourcePath = Path.Combine(m_BuildPath, "_site");
        await FileManager.CopyDirectoryAsync(sourcePath, m_OutputPath, "*.*");
        string destination = Path.Combine(m_OutputPath, "package.json");
        await FileManager.TryCopyFileAsync("package.json", destination);
    }

    async ValueTask RunDocFxAsync()
    {
        var docFx = new DocFx(DocFxPath)
        {
            WorkingDirectory = m_BuildPath
        };

        string docFxJsonFile = Path.Combine(m_BuildPath, "docfx.json");
        await docFx.RunAsync(docFxJsonFile, Logger);
    }
}
