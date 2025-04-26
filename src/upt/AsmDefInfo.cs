// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;

namespace UnityPackageTool;

[Serializable]
sealed class AsmDefInfo
{
    public string Name { get; set; } = string.Empty;
    public string RootNameSpace { get; set; } = string.Empty;
    public string[]? References { get; set; }
    public string[] IncludePlatforms { get; set; } = [];
    public string[] ExcludePlatforms { get; set; } = [];
    public bool AllowUnsafeCode { get; set; }
    public bool OverrideReferences { get; set; }
    public string[] PrecompiledReferences { get; set; } = [];
    public bool AutoReferenced { get; set; }
    public string[] DefineConstraints { get; set; } = [];
    public bool NoEngineReferences { get; set; }
}
