// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands.Init;

[Flags]
enum InitializationMode
{
    Editor = 1, Runtime = 2, Default = Editor | Runtime
}

sealed class PackageInitializer(string packageName, string version, InitializationMode initializationMode, string outputPath)
{
    public required ISimpleLogger Logger { get; init; }
    public required FileManager FileManager { get; init; }
    public string? DisplayName { get; init; }
    public required string RuntimeAssemblyName { get; init; }

    public AuthorInfo? Author { get; init; }

    public string? UnityVersion { get; init; }

    string EditorAssemblyName => $"{RuntimeAssemblyName}.Editor";

    public async Task InitializePackageAsync(bool force)
    {
        Logger.Info($"Initializing package {packageName}, version {version}.");

        string packagePath = Path.Combine(outputPath, packageName);

        DeleteDirectory(packagePath, force);
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

        bool hasRuntimeAssembly = (initializationMode & InitializationMode.Runtime) != 0;
        if (hasRuntimeAssembly)
        {
            var runtimeAsmdef = new AsmDefInfo
            {
                Name = RuntimeAssemblyName,
                AutoReferenced = false
            };

            string runtimeAsmdefPath = Path.Combine(packagePath, "Runtime", $"{RuntimeAssemblyName}.asmdef");
            await FileManager.WriteJsonAsync(runtimeAsmdefPath, runtimeAsmdef);
        }

        if ((initializationMode & InitializationMode.Editor) != 0)
        {
            string editorAssemblyName = EditorAssemblyName;
            var editorAsmdef = new AsmDefInfo
            {
                Name = editorAssemblyName,
                IncludePlatforms = ["Editor"],
                AutoReferenced = false,
                References = hasRuntimeAssembly ? [RuntimeAssemblyName] : null
            };

            string editorAsmdefPath = Path.Combine(packagePath, "Editor", $"{editorAssemblyName}.asmdef");
            await FileManager.WriteJsonAsync(editorAsmdefPath, editorAsmdef);
        }
    }

    public async Task InitializeTestsPackage(bool force)
    {
        string testsPackageName = $"{packageName}.tests";

        Logger.Info($"Initializing package {testsPackageName}, version {version}.");

        string packagePath = Path.Combine(outputPath, testsPackageName);
        DeleteDirectory(packagePath, force);
        FileManager.CreateDirectory(packagePath);

        var package = new PackageInfo
        {
            Name = testsPackageName,
            Version = version,
            Description = string.Empty,
            Author = Author,
            Unity = UnityVersion,
            Dependencies = [new DependencyInfo(packageName, version)]
        };

        if (DisplayName is not null)
            package.DisplayName = $"{DisplayName} - Tests";

        string packageJsonPath = Path.Combine(packagePath, "package.json");
        await FileManager.WriteJsonAsync(packageJsonPath, package);

        bool hasRuntimeAssembly = (initializationMode & InitializationMode.Runtime) != 0;
        if (hasRuntimeAssembly)
        {
            string runtimeTestsAssemblyName = $"{RuntimeAssemblyName}.Tests";
            var runtimeTestsAsmdef = new AsmDefInfo
            {
                Name = runtimeTestsAssemblyName,
                AutoReferenced = false,
                References = [RuntimeAssemblyName],
                OverrideReferences = true,
                PrecompiledReferences = ["nunit.framework.dll"],
                DefineConstraints = ["UNITY_INCLUDE_TESTS"]
            };

            string runtimeTestsAsmdefPath = Path.Combine(packagePath, "Runtime", $"{runtimeTestsAssemblyName}.asmdef");
            await FileManager.WriteJsonAsync(runtimeTestsAsmdefPath, runtimeTestsAsmdef);
        }

        string editorTestsAssemblyName = $"{EditorAssemblyName}.Tests";
        var editorTestsAsmdef = new AsmDefInfo
        {
            Name = editorTestsAssemblyName,
            IncludePlatforms = ["Editor"],
            AutoReferenced = false,
            References = hasRuntimeAssembly ? [RuntimeAssemblyName, EditorAssemblyName] : [EditorAssemblyName],
            OverrideReferences = true,
            PrecompiledReferences = ["nunit.framework.dll"],
            DefineConstraints = ["UNITY_INCLUDE_TESTS"]
        };

        string editorTestsAsmdefPath = Path.Combine(packagePath, "Editor", $"{editorTestsAssemblyName}.asmdef");
        await FileManager.WriteJsonAsync(editorTestsAsmdefPath, editorTestsAsmdef);
    }

    void DeleteDirectory(string packagePath, bool force)
    {
        if (!Directory.Exists(packagePath))
            return;
        if (!force)
            throw new InvalidOperationException($"Cannot create a new package at `{packagePath}`. The directory already exists. Use option --force to overwrite it.");
        FileManager.DeleteDirectory(packagePath);
    }
}
