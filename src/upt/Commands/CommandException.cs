// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;

namespace UnityPackageTool.Commands;

class CommandException(string message)
    : Exception(message) { }

class CommandValidationException(string message)
    : CommandException(message) { }
