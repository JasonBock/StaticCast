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
		var methodsToGenerate = new Dictionary<ITypeSymbol, HashSet<MethodSymbolSignature>>();
		var propertiesToGenerate = new Dictionary<ITypeSymbol, HashSet<PropertySymbolSignature>>();

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
						if (accessNode.Name is IdentifierNameSyntax)
						{
							if(!castToParameterSymbol.GetMembers().Any(
								_ => (_.Kind == SymbolKind.Method || _.Kind == SymbolKind.Property) && _.IsStatic && _.IsAbstract))
							{
								diagnostics.Add(InterfaceHasNoStaticAbstractMembersDiagnostic.Create(castToParameterSymbol, castToParameterType));
							}
							else
							{
								StaticCastInformation.GetMethods(methodsToGenerate, castToParameterType, castToParameterSymbol);
								StaticCastInformation.GetProperties(propertiesToGenerate, castToParameterType, castToParameterSymbol);
							}
						}
					}
				}
			}
		}

		this.Diagnostics = diagnostics.ToImmutableArray();
		this.MethodsToGenerate = methodsToGenerate.ToImmutableDictionary();
		this.PropertiesToGenerate = propertiesToGenerate.ToImmutableDictionary();
	}

	private static void GetMethods(Dictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> methodsToGenerate, 
		TypeSyntax castToParameterType, ITypeSymbol castToParameterSymbol)
	{
		var methods = castToParameterSymbol.GetMembers().OfType<IMethodSymbol>()
			.Where(_ => _.IsStatic && _.IsAbstract && 
				_.MethodKind != MethodKind.PropertyGet && _.MethodKind != MethodKind.PropertySet);

		foreach (var method in methods)
		{
			if (methodsToGenerate.ContainsKey(method.ReturnType))
			{
				methodsToGenerate[method.ReturnType].Add(new MethodSymbolSignature(method));
			}
			else
			{
				methodsToGenerate.Add(method.ReturnType, new() { new MethodSymbolSignature(method) });
			}
		}
	}

	private static void GetProperties(Dictionary<ITypeSymbol, HashSet<PropertySymbolSignature>> propertiesToGenerate,
		TypeSyntax castToParameterType, ITypeSymbol castToParameterSymbol)
	{
		var properties = castToParameterSymbol.GetMembers().OfType<IPropertySymbol>()
			.Where(_ => _.IsStatic && _.IsAbstract);

		foreach (var property in properties)
		{
			if (propertiesToGenerate.ContainsKey(property.Type))
			{
				propertiesToGenerate[property.Type].Add(new PropertySymbolSignature(property));
			}
			else
			{
				propertiesToGenerate.Add(property.Type, new() { new PropertySymbolSignature(property) });
			}
		}
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
	internal ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> MethodsToGenerate { get; private set; }
	internal ImmutableDictionary<ITypeSymbol, HashSet<PropertySymbolSignature>> PropertiesToGenerate { get; private set; }
}