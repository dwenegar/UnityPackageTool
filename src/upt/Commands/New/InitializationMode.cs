// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;

namespace UnityPackageTool.Commands.New;

[Flags]
enum InitializationMode
{
    Editor = 1,
    Runtime = 2,
    Default = Editor | Runtime
}
