// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;
using System;
using System.Linq;
using UnityPackageTool;
using UnityPackageTool.Utils;

var app = new CommandLineApplication<UnityPackageToolApp>
{
    Name = AssemblyHelpers.GetAssemblyName()
};

app.Conventions.UseDefaultConventions();

string version = AssemblyHelpers.GetInformationalVersion();
app.VersionOption("--version", $"{app.Name} {version}", $"{app.Name} {version} - Copyright {DateTime.Now.Year} Simone Livieri");

try
{
    return await app.ExecuteAsync(args);
}
catch (CommandParsingException ex)
{
    await Console.Error.WriteLineAsync(ex.Message);
    if (ex is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
    {
        await Console.Error.WriteLineAsync();
        await Console.Error.WriteLineAsync("The most similar commands are");
        foreach (string match in uex.NearestMatches)
            await Console.Error.WriteLineAsync($"        {match}");
    }

    return 1;
}
catch (Exception e)
{
    await Console.Error.WriteLineAsync($"INTERNAL ERROR: {e.Message}");
    await Console.Error.WriteLineAsync(e.StackTrace);
    return 1;
}
