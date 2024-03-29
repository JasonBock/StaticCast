﻿using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.Globalization;

namespace StaticCast.Diagnostics;

internal static class InterfaceHasNoStaticAbstractMembersDiagnostic
{
	internal static Diagnostic Create(ITypeSymbol type, SyntaxNode castToParameterType) =>
		Diagnostic.Create(new(InterfaceHasNoStaticAbstractMembersDiagnostic.Id, InterfaceHasNoStaticAbstractMembersDiagnostic.Title,
			string.Format(CultureInfo.CurrentCulture, InterfaceHasNoStaticAbstractMembersDiagnostic.Message,
				type.GetName()),
			DiagnosticConstants.Usage, DiagnosticSeverity.Error, true,
			helpLinkUri: HelpUrlBuilder.Build(
				InterfaceHasNoStaticAbstractMembersDiagnostic.Id, InterfaceHasNoStaticAbstractMembersDiagnostic.Title)),
			castToParameterType.GetLocation());

	public const string Id = "SC1";
	public const string Message = "The interface {0} has not static abstract members";
	public const string Title = "Static Abstract Members on Interface";
}