// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;

namespace UnityPackageTool.Commands;

sealed class CommandException(string message)
    : Exception(message) { }
