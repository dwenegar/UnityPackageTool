// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System.Diagnostics.CodeAnalysis;
using UnityPackageTool.Upm;

namespace UnityPackageTool.Commands.Dependencies;

static class DependenciesCommandHelpers
{
    public static bool TryResolvePackageVersion(UpmPackageInfo[] packageVersions, PackageInfo package, [NotNullWhen(true)] ref string? packageVersion)
    {
        foreach (UpmPackageInfo upmPackageInfo in packageVersions)
        {
            if (package.Unity is not null && upmPackageInfo.Unity != package.Unity)
                continue;

            if (packageVersion is null)
            {
                packageVersion = upmPackageInfo.Version;
                return true;
            }

            if (upmPackageInfo.Version == packageVersion)
                return true;
        }

        return false;
    }
}
