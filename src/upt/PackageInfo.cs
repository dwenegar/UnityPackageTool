// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityPackageTool;

[Serializable]
sealed class PackageInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DisplayName { get; set; }
    public string? Unity { get; set; }
    public AuthorInfo? Author { get; set; }
    public string? ChangelogUrl { get; set; }
    public DependencyList? Dependencies { get; set; }
    public string? DocumentationUrl { get; set; }
    public bool? HideInEditor { get; set; }
    public string[]? KeyWords { get; set; }
    public string? License { get; set; }
    public string? LicenseUrl { get; set; }
    public SampleInfo[]? Samples { get; set; }
    public string? UnityRelease { get; set; }

    [JsonIgnore]
    public string TrimmedVersion
    {
        get
        {
            var version = System.Version.Parse(Version);
            return $"{version.Major}.{version.Minor}";
        }
    }

    [JsonIgnore]
    public bool IsValid => !string.IsNullOrEmpty(Name)
                           && !string.IsNullOrEmpty(Version);

    public override string ToString() => $"{DisplayName} {Version}";
}

[Serializable]
record AuthorInfo(string Name, string? Email, string? Url);

[Serializable]
record DependencyInfo(string Name, string Version)
{
    public override string ToString() => $"{Name}@{Version}";
}

sealed class DependencyList : IEnumerable<DependencyInfo>
{
    readonly List<DependencyInfo> m_Dependencies = new();

    public DependencyInfo this[int index]
    {
        get => m_Dependencies[index];
        set => m_Dependencies[index] = value;
    }

    public int Count => m_Dependencies.Count;

    IEnumerator IEnumerable.GetEnumerator() => m_Dependencies.GetEnumerator();

    IEnumerator<DependencyInfo> IEnumerable<DependencyInfo>.GetEnumerator() => m_Dependencies.GetEnumerator();

    public void Add(DependencyInfo dependency) => m_Dependencies.Add(dependency);

    public void Clear() => m_Dependencies.Clear();

    public void Add(string name, string version) => m_Dependencies.Add(new DependencyInfo(name, version));

    public bool TryGet(string name, [NotNullWhen(true)] out DependencyInfo? result)
    {
        foreach (DependencyInfo dependency in m_Dependencies)
        {
            if (dependency.Name == name)
            {
                result = dependency;
                return true;
            }
        }

        result = null;
        return false;
    }

    public bool TryFindByName(string name, out int index, [NotNullWhen(true)] out DependencyInfo? dependency)
    {
        for (index = 0; index < m_Dependencies.Count; index++)
        {
            dependency = m_Dependencies[index];
            if (dependency.Name == name)
                return true;
        }

        index = -1;
        dependency = null;
        return false;
    }

    public bool TryRemove(string name, [NotNullWhen(true)] out DependencyInfo? result)
    {
        for (int i = 0; i < m_Dependencies.Count; i++)
        {
            DependencyInfo dependency = m_Dependencies[i];
            if (dependency.Name == name)
            {
                m_Dependencies.RemoveAt(i);
                result = dependency;
                return true;
            }
        }

        result = null;
        return false;
    }
}

[Serializable]
record SampleInfo(string DisplayName, string Description, string Path);

sealed class DependencyListConverter : JsonConverter<DependencyList>
{
    public override DependencyList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new DependencyList();
        using var doc = JsonDocument.ParseValue(ref reader);
        foreach (JsonProperty property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.GetString() is { } version)
                result.Add(new DependencyInfo(property.Name, version));
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, DependencyList value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (DependencyInfo dependency in value)
        {
            writer.WritePropertyName(dependency.Name);
            writer.WriteStringValue(dependency.Version);
        }

        writer.WriteEndObject();
    }
}
