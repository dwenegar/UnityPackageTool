// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System;
using System.Linq;
using UnityPackageTool;
using UnityPackageTool.Commands;
using UnityPackageTool.Utils;

var app = new CommandLineApplication<UnityPackageToolApp>
{
    Name = AssemblyHelpers.GetAssemblyName()
};

app.Conventions.UseDefaultConventions();

string version = AssemblyHelpers.GetInformationalVersion();
app.VersionOption("--version", $"{app.Name} {version}", $"{app.Name} {version} - Copyright {DateTime.Now.Year} Simone Livieri");

app.OnValidationError(result =>
{
    AnsiConsole.Foreground = ConsoleColor.Red;
    AnsiConsole.WriteLine(result.ErrorMessage ?? "Unknown error");
    AnsiConsole.ResetColors();
    app.ShowHelp();
    return 1;
});

try
{
    return await app.ExecuteAsync(args);
}
catch (CommandParsingException ex)
{
    Console.Error.WriteLine(ex.Message);
    if (ex is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("Did you mean this?");
        Console.Error.WriteLine("    " + uex.NearestMatches.First());
    }

    return 1;
}
catch (CommandException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(ex.Message);
    Console.ResetColor();
    return 1;
}
