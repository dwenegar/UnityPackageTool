// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ReplaceAutoPropertyWithComputedProperty

using McMaster.Extensions.CommandLineUtils;

namespace UnityPackageTool.Commands.Dependencies;

[Command("dependencies", "deps", Description = "Manage package dependencies")]
[Subcommand(typeof(DependenciesListCommand))]
[Subcommand(typeof(DependenciesAddCommand))]
[Subcommand(typeof(DependenciesRemoveCommand))]
[Subcommand(typeof(DependenciesUpdateCommand))]
sealed class DependenciesCommand : RootCommandBase;
