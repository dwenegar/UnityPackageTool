// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Collections;
using System.Collections.Generic;
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
record DependencyInfo(string Name, string Version);

sealed class DependencyList : IList<DependencyInfo>
{
    readonly List<DependencyInfo> m_Dependencies = new();

    public DependencyInfo this[int index]
    {
        get => m_Dependencies[index];
        set => m_Dependencies[index] = value;
    }

    public int Count => m_Dependencies.Count;

    public bool IsReadOnly => false;

    public void Add(DependencyInfo item) => m_Dependencies.Add(item);

    public void Clear() => m_Dependencies.Clear();

    public bool Contains(DependencyInfo item) => m_Dependencies.Contains(item);

    public void CopyTo(DependencyInfo[] array, int arrayIndex) => m_Dependencies.CopyTo(array, arrayIndex);

    public bool Remove(DependencyInfo item) => m_Dependencies.Remove(item);

    public int IndexOf(DependencyInfo item) => m_Dependencies.IndexOf(item);

    public void Insert(int index, DependencyInfo item) => m_Dependencies.Insert(index, item);

    public void RemoveAt(int index) => m_Dependencies.RemoveAt(index);

    public IEnumerator<DependencyInfo> GetEnumerator() => m_Dependencies.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Dependencies).GetEnumerator();
}

[Serializable]
record SampleInfo(string DisplayName, string Description, string Path);

sealed class DependencyInfoConverter : JsonConverter<DependencyList>
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
