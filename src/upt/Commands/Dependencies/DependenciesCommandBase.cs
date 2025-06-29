// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System.Threading.Tasks;
using UnityPackageTool.Utils;
using static UnityPackageTool.Commands.CommandHelpers;

namespace UnityPackageTool.Commands.Dependencies;

abstract class DependenciesCommandBase : CommandBase
{
    protected override async ValueTask ExecuteAsync(FileManager fileManager, SimpleLogger logger)
    {
        PackageInfo package = await ReadPackageJsonAsync(fileManager);
        await ExecuteAsync(package, fileManager, logger);
    }

    protected abstract ValueTask ExecuteAsync(PackageInfo fileManager, FileManager logger, SimpleLogger simpleLogger);
}
