// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Collections.Generic;

namespace UnityPackageTool.Upm;

[Serializable]
class UpmPackageCollection
{
    public Dictionary<string, UpmPackageInfo>? Versions { get; set; }
}

[Serializable]
class UpmPackageInfo : IComparable<UpmPackageInfo>
{
    Version? m_ParsedVersion;

    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Unity { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public int CompareTo(UpmPackageInfo? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        int nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0)
            return nameComparison;

        Version version = m_ParsedVersion ??= System.Version.Parse(Version);
        Version otherVersion = other.m_ParsedVersion ??= System.Version.Parse(Version);
        return version.CompareTo(otherVersion);
    }
}
