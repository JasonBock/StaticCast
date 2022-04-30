using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticCast.Diagnostics;
using System.Collections.Immutable;

namespace StaticCast;

internal sealed class StaticCastInformation
{
	internal StaticCastInformation(ImmutableArray<MemberAccessExpressionSyntax> accessNodes,
		Compilation compilation)
	{
		var diagnostics = new List<Diagnostic>();
		var membersToGenerate = new Dictionary<ITypeSymbol, HashSet<MethodSymbolSignature>>();

		foreach (var accessNode in accessNodes)
		{
			var model = compilation.GetSemanticModel(accessNode.SyntaxTree)!;
			var castToParameterType = (accessNode.Expression as GenericNameSyntax)!.TypeArgumentList.Arguments[1];
			var castToParameterSymbol = model.GetSymbolInfo(castToParameterType).Symbol as INamedTypeSymbol;

			// TODO: if the type kind isn't found, but we can somehow infer that
			// it would be an interface based on constraints, that should be done.
			if(castToParameterSymbol!.TypeKind != TypeKind.Interface)
			{
				diagnostics.Add(GenericParameterIsNotInterfaceDiagnostic.Create(castToParameterSymbol));
			}
			else
			{
				if (accessNode.Name is IdentifierNameSyntax accessNodeName)
				{
					var memberName = accessNodeName.Identifier.Text;
					// TODO: We also need properties.
					var members = castToParameterSymbol.GetMembers().OfType<IMethodSymbol>()
						.Where(_ => _.IsStatic && _.IsAbstract);

					if (!members.Any())
					{
						diagnostics.Add(InterfaceHasNoStaticAbstractMembersDiagnostic.Create(castToParameterSymbol));
					}
					else
					{
						foreach (var member in members)
						{
							if (membersToGenerate.ContainsKey(member.ReturnType))
							{
								membersToGenerate[member.ReturnType].Add(new MethodSymbolSignature(member));
							}
							else
							{
								membersToGenerate.Add(member.ReturnType, new() { new MethodSymbolSignature(member) });
							}
						}
					}
				}
			}
		}

		this.Diagnostics = diagnostics.ToImmutableArray();
		this.MembersToGenerate = membersToGenerate.ToImmutableDictionary();
	}

	internal ImmutableArray<Diagnostic> Diagnostics { get; private set; }
	internal ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> MembersToGenerate { get; private set; }
}