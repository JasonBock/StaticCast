using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.Globalization;

namespace StaticCast.Diagnostics;

internal static class GenericParameterIsNotInterfaceDiagnostic
{
	internal static Diagnostic Create(ISymbol? type, SyntaxNode castToParameterType) =>
		Diagnostic.Create(new(GenericParameterIsNotInterfaceDiagnostic.Id, GenericParameterIsNotInterfaceDiagnostic.Title,
			string.Format(CultureInfo.CurrentCulture, GenericParameterIsNotInterfaceDiagnostic.Message,
				type?.Name ?? "?"),
			DiagnosticConstants.Usage, DiagnosticSeverity.Error, true,
			helpLinkUri: HelpUrlBuilder.Build(
				GenericParameterIsNotInterfaceDiagnostic.Id, GenericParameterIsNotInterfaceDiagnostic.Title)), 
			castToParameterType.GetLocation());

	public const string Id = "SC2";
	public const string Message = "The generic parameter type {0} is not an interface or it is not constrained to an interface";
	public const string Title = "Generic Parameter Type";
}