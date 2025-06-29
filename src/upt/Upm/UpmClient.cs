// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UnityPackageTool.Upm;

class UpmClient(SimpleLogger logger)
{
    static readonly JsonSerializerOptions k_JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public async ValueTask<UpmPackageInfo[]?> GetPackageVersionsAsync(string packageName)
    {
        string requestUri = $"https://packages.unity.com/{packageName}";

        using var client = new HttpClient();
        try
        {
            logger.Debug($"Fetching version list for package '{packageName}' from {requestUri}.");
            var sw = Stopwatch.StartNew();
            UpmPackageCollection? upmPackageCollection = await client.GetFromJsonAsync<UpmPackageCollection>(requestUri, k_JsonSerializerOptions);
            if (upmPackageCollection is { Versions: { } versions })
            {
                logger.Debug($"Retrieved {versions.Count} package version{(versions.Count == 1 ? "" : "s")} for '{packageName}' in {sw.ElapsedMilliseconds} ms.");
                UpmPackageInfo[] result = versions.Values.Where(x => !x.Version.Contains('-', StringComparison.Ordinal)).ToArray();
                Array.Sort(result, (x, y) => -x.CompareTo(y));
                return result;
            }
        }
        catch (Exception e)
        {
            logger.Error("Failed to fetch package versions.", e);
        }

        return null;
    }
}
