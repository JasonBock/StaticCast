using System.Globalization;
using System.Text.RegularExpressions;

namespace StaticCast.Extensions;

internal static class StringExtensions
{
	// This code came from Humanize:
	// https://github.com/Humanizr/Humanizer/blob/7492f69c25be62c3be8cd435d9ccaa95a2ef20e9/src/Humanizer/InflectorExtensions.cs
	// Trying to reference the package in the source generator
	// just wasn't working, and the implementation is pretty small
	// so I basically copied it here.
	// Giving credit where credit is due.
	internal static string ToPascalCase(this string self) => 
		Regex.Replace(self, "(?:^|_| +)(.)", match => match.Groups[1].Value.ToUpper(CultureInfo.CurrentCulture));
}