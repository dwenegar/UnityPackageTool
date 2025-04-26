// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;

namespace UnityPackageTool;

[Serializable]
sealed class DocumentationConfig
{
    public string[] DefineConstants { get; set; } = [];
    public string[] Sources { get; set; } = ["**/Editor", "**/Runtime"];
}
