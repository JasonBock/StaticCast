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
			var (symbol, castToParameterSymbols) = StaticCastInformation.GetParameterTypes(castToParameterType, model);

			if (castToParameterSymbols.Length == 0)
			{
				diagnostics.Add(GenericParameterIsNotInterfaceDiagnostic.Create(symbol, castToParameterType));
			}
			else
			{
				foreach (var castToParameterSymbol in castToParameterSymbols)
				{
					if (castToParameterSymbol!.TypeKind != TypeKind.Interface)
					{
						diagnostics.Add(GenericParameterIsNotInterfaceDiagnostic.Create(castToParameterSymbol, castToParameterType));
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
								diagnostics.Add(InterfaceHasNoStaticAbstractMembersDiagnostic.Create(castToParameterSymbol, castToParameterType));
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
			}
		}

		this.Diagnostics = diagnostics.ToImmutableArray();
		this.MembersToGenerate = membersToGenerate.ToImmutableDictionary();
	}

	private static (ISymbol?, ImmutableArray<ITypeSymbol>) GetParameterTypes(TypeSyntax node, SemanticModel model)
	{
		var symbol = model.GetSymbolInfo(node).Symbol;
		return symbol switch
		{
			INamedTypeSymbol typeSymbol => (symbol, ImmutableArray.Create<ITypeSymbol>(typeSymbol)),
			ITypeParameterSymbol typeParameterSymbol => (symbol, typeParameterSymbol.ConstraintTypes),
			_ => (symbol, ImmutableArray<ITypeSymbol>.Empty)
		};
	}

	internal ImmutableArray<Diagnostic> Diagnostics { get; private set; }
	internal ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> MembersToGenerate { get; private set; }
}