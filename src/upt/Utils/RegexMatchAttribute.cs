// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UnityPackageTool.Utils;

[AttributeUsage(AttributeTargets.Property)]
class RegexMatchAttribute(string regex, string? errorMessage = null)
    : ValidationAttribute(errorMessage ?? $"Invalid value '{{0}}'. Value must match \"{regex}\"")
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue && Regex.IsMatch(stringValue, regex, RegexOptions.None))
            return ValidationResult.Success;

        return new ValidationResult(FormatErrorMessage(value?.ToString() ?? string.Empty));
    }
}

class PackageNameAttribute()
    : RegexMatchAttribute(@"^(?:[a-z0-9][a-z0-9\-_]*\.)+[a-z0-9][a-z0-9\-_]*$", "Invalid package name '{0}'. See https://docs.unity3d.com/Manual/cus-naming.html for how to name your package.");

class PackageVersionAttribute()
    : RegexMatchAttribute(@"^\d+\.\d+\.\d+$", "Invalid version '{0}'. See https://docs.unity3d.com/Manual/upm-semver.html for how to version your package.");
