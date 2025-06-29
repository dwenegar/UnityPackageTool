// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using McMaster.Extensions.CommandLineUtils;

namespace UnityPackageTool.Commands.Documentation;

[Command("documentation", "docs", Description = "Manage package documentation")]
[Subcommand(typeof(DocumentationInitCommand))]
[Subcommand(typeof(DocumentationBuildCommand))]
sealed class DocumentationCommand : RootCommandBase;
