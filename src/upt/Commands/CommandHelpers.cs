// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System.IO;
using System.Threading.Tasks;
using UnityPackageTool.Utils;

namespace UnityPackageTool.Commands;

static class CommandHelpers
{
    public static async ValueTask<PackageInfo> ReadPackageJsonAsync(FileManager fileManager)
    {
        return await fileManager.ReadPackageJsonAsync()
               ?? throw new CommandValidationException($"Invalid package folder: {Directory.GetCurrentDirectory()}. Missing or invalid 'package.json' file.");
    }
}
