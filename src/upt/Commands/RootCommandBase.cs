// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

#pragma warning disable CA1822

using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace UnityPackageTool;

class RootCommandBase
{
    [UsedImplicitly]
    protected int OnExecute(CommandLineApplication app)
    {
        Console.WriteLine("Specify a command");
        app.ShowHelp();
        return 1;
    }
}
