// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.New;

sealed class PackageInitializer(string packageName, string version)
{
    public required InitializationMode Mode { get; init; }
    public required SimpleLogger Logger { get; init; }
    public required FileManager FileManager { get; init; }
    public string? DisplayName { get; init; }
    public required string RootNameSpace { get; init; }
    public required string RuntimeAssemblyName { get; init; }
    public AuthorInfo? Author { get; init; }
    public string? UnityVersion { get; init; }
    string EditorAssemblyName => $"{RuntimeAssemblyName}.Editor";
    public bool Force { get; init; }

    public async Task InitializePackageAsync()
    {
        Logger.Info($"Initializing package '{packageName}', version {version}.");

        string packagePath = Path.GetFullPath(packageName);
        if (Directory.Exists(packagePath))
        {
            if (!Force)
                throw new CommandException($"Cannot create a new package at '{packagePath}'. The directory already exists. Use option --force to overwrite it.");
            FileManager.DeleteDirectory(packagePath);
        }

        FileManager.CreateDirectory(packagePath);

        var package = new PackageInfo
        {
            Name = packageName,
            Version = version,
            DisplayName = DisplayName,
            Author = Author,
            Unity = UnityVersion
        };

        string packageJsonPath = Path.Combine(packagePath, "package.json");
        await FileManager.WriteJsonAsync(packageJsonPath, package);

        bool hasRuntimeAssembly = (Mode & InitializationMode.Runtime) != 0;
        if (hasRuntimeAssembly)
        {
            var runtimeAsmdef = new AsmDefInfo
            {
                Name = RuntimeAssemblyName,
                RootNameSpace = RootNameSpace,
                AutoReferenced = false
            };

            string runtimeFolderPath = Path.Combine(packagePath, "Runtime");
            FileManager.CreateDirectory(runtimeFolderPath);

            string runtimeAsmdefPath = Path.Combine(runtimeFolderPath, $"{RuntimeAssemblyName}.asmdef");
            await FileManager.WriteJsonAsync(runtimeAsmdefPath, runtimeAsmdef);
        }

        bool hasEditorAssembly = (Mode & InitializationMode.Editor) != 0;
        if (hasEditorAssembly)
        {
            string editorAssemblyName = EditorAssemblyName;
            var editorAsmdef = new AsmDefInfo
            {
                Name = editorAssemblyName,
                RootNameSpace = RootNameSpace,
                IncludePlatforms = ["Editor"],
                AutoReferenced = false,
                References = hasRuntimeAssembly ? [RuntimeAssemblyName] : null
            };

            string editorFolderPath = Path.Combine(packagePath, "Editor");
            FileManager.CreateDirectory(editorFolderPath);

            string editorAsmdefPath = Path.Combine(editorFolderPath, $"{editorAssemblyName}.asmdef");
            await FileManager.WriteJsonAsync(editorAsmdefPath, editorAsmdef);
        }

        string documentationPath = Path.Combine(packagePath, "Documentation~");
        FileManager.CreateDirectory(documentationPath);

        var config = new DocumentationConfiguration();
        string configJsonPath = Path.Combine(documentationPath, "config.json");
        await FileManager.WriteJsonAsync(configJsonPath, config);

        string indexPath = Path.Combine(documentationPath, "index.md");
        string displayName = DisplayName ?? packageName;
        string indexContent = $"""
                               # {displayName} {version}

                               This is the documentation for the package `{displayName}`.
                               """;
        await FileManager.WriteTextAsync(indexPath, indexContent);
    }

    public async Task InitializeTestsPackage()
    {
        string testsPackageName = $"{packageName}.tests";

        Logger.Info($"Initializing package {testsPackageName}, version {version}.");

        string packagePath = Path.GetFullPath(testsPackageName);
        if (Directory.Exists(packagePath))
        {
            if (!Force)
                throw new CommandException($"Cannot create a new package at '{packagePath}'. The directory already exists. Use option --force to overwrite it.");
            FileManager.DeleteDirectory(packagePath);
        }

        FileManager.CreateDirectory(packagePath);

        var package = new PackageInfo
        {
            Name = testsPackageName,
            Version = version,
            Author = Author,
            Unity = UnityVersion,
            Dependencies = [new DependencyInfo(packageName, version)]
        };

        if (DisplayName is not null)
            package.DisplayName = $"{DisplayName} - Tests";

        string packageJsonPath = Path.Combine(packagePath, "package.json");
        await FileManager.WriteJsonAsync(packageJsonPath, package);

        string rootNameSpace = string.IsNullOrEmpty(RootNameSpace) ? "Tests" : $"{RootNameSpace}.Tests";
        bool hasRuntimeAssembly = (Mode & InitializationMode.Runtime) != 0;
        if (hasRuntimeAssembly)
        {
            string runtimeTestsAssemblyName = $"{RuntimeAssemblyName}.Tests";
            var runtimeTestsAsmdef = new AsmDefInfo
            {
                Name = runtimeTestsAssemblyName,
                RootNameSpace = rootNameSpace,
                AutoReferenced = false,
                References = [RuntimeAssemblyName],
                OverrideReferences = true,
                PrecompiledReferences = ["nunit.framework.dll"],
                DefineConstraints = ["UNITY_INCLUDE_TESTS"]
            };

            string runtimeFolderPath = Path.Combine(packagePath, "Runtime");
            FileManager.CreateDirectory(runtimeFolderPath);

            string runtimeTestsAsmdefPath = Path.Combine(runtimeFolderPath, $"{runtimeTestsAssemblyName}.asmdef");
            await FileManager.WriteJsonAsync(runtimeTestsAsmdefPath, runtimeTestsAsmdef);
        }

        string editorTestsAssemblyName = $"{EditorAssemblyName}.Tests";
        var editorTestsAsmdef = new AsmDefInfo
        {
            Name = editorTestsAssemblyName,
            RootNameSpace = rootNameSpace,
            IncludePlatforms = ["Editor"],
            AutoReferenced = false,
            OverrideReferences = true,
            PrecompiledReferences = ["nunit.framework.dll"],
            DefineConstraints = ["UNITY_INCLUDE_TESTS"]
        };

        bool hasEditorAssembly = (Mode & InitializationMode.Editor) != 0;
        var references = new List<string>();
        if (hasRuntimeAssembly)
            references.Add(RuntimeAssemblyName);
        if (hasEditorAssembly)
            references.Add(EditorAssemblyName);
        editorTestsAsmdef.References = references.ToArray();

        string editorFolderPath = Path.Combine(packagePath, "Editor");
        FileManager.CreateDirectory(editorFolderPath);

        string editorTestsAsmdefPath = Path.Combine(editorFolderPath, $"{editorTestsAssemblyName}.asmdef");
        await FileManager.WriteJsonAsync(editorTestsAsmdefPath, editorTestsAsmdef);
    }
}
