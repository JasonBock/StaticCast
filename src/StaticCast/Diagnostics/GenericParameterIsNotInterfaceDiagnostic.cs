using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.Globalization;

namespace StaticCast.Diagnostics;

internal static class GenericParameterIsNotInterfaceDiagnostic
{
	internal static Diagnostic Create(ITypeSymbol type) =>
		Diagnostic.Create(new(GenericParameterIsNotInterfaceDiagnostic.Id, GenericParameterIsNotInterfaceDiagnostic.Title,
			string.Format(CultureInfo.CurrentCulture, GenericParameterIsNotInterfaceDiagnostic.Message,
				type.GetName()),
			DiagnosticConstants.Usage, DiagnosticSeverity.Error, true,
			helpLinkUri: HelpUrlBuilder.Build(
				GenericParameterIsNotInterfaceDiagnostic.Id, GenericParameterIsNotInterfaceDiagnostic.Title)),
			type.Locations.Length > 0 ? type.Locations[0] : null);

	public const string Id = "SC2";
	public const string Message = "The generic parameter type {0} is not an interface";
	public const string Title = "Generic Parameter Type";
}