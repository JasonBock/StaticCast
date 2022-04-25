using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StaticCast.Builders;
using System.Collections.Immutable;

namespace StaticCast;

[Generator]
internal sealed class StaticCastGenerator
	: IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) =>
			node is MemberAccessExpressionSyntax access &&
			access.Expression is GenericNameSyntax genericName &&
			genericName.Identifier.Text == "StaticCast" &&
			genericName.TypeArgumentList.Arguments.Count == 2;

		static MemberAccessExpressionSyntax TransformTargets(GeneratorSyntaxContext context, CancellationToken token) =>
			(MemberAccessExpressionSyntax)context.Node;

		var provider = context.SyntaxProvider
			 .CreateSyntaxProvider(IsSyntaxTargetForGeneration, TransformTargets)
			 .Where(static _ => _ is not null);
		var compilationNodes = context.CompilationProvider.Combine(provider.Collect());
		var output = context.AnalyzerConfigOptionsProvider.Combine(compilationNodes);
		context.RegisterSourceOutput(output,
			(context, source) => CreateOutput(source.Right.Left, source.Right.Right, source.Left, context));
	}

	private static void CreateOutput(Compilation compilation, ImmutableArray<MemberAccessExpressionSyntax> accessNodes,
		AnalyzerConfigOptionsProvider options, SourceProductionContext context)
	{
		var signatures = new HashSet<string>();
		var membersToGenerate = new List<IMethodSymbol>();

		foreach (var accessNode in accessNodes)
		{
			var model = compilation.GetSemanticModel(accessNode.SyntaxTree)!;
			var castToParameterType = (accessNode.Expression as GenericNameSyntax)!.TypeArgumentList.Arguments[1];
			var castToParameterSymbol = model.GetSymbolInfo(castToParameterType).Symbol as INamedTypeSymbol;

			// TODO: if the type kind isn't found, but we can somehow infer that
			// it would be an interface based on constraints, that should be done.
			if (castToParameterSymbol!.TypeKind == TypeKind.Interface)
			{
				if (accessNode.Name is IdentifierNameSyntax accessNodeName)
				{
					var memberName = accessNodeName.Identifier.Text;
					// TODO: We also need properties.
					var members = castToParameterSymbol.GetMembers(memberName).OfType<IMethodSymbol>();

					foreach (var member in members)
					{
						// TODO: This may not be good enough (and probably isn't)
						var memberSignature = member.ToString();

						if (signatures.Add(memberSignature))
						{
							membersToGenerate.Add(member);
						}
					}
				}
			}
		}

		if (membersToGenerate.Count > 0)
		{
			var builder = new StaticCastBuilder(membersToGenerate);
			// TODO: Check diagnostics.
			context.AddSource("StaticCast.g.cs", builder.Code);
		}
	}
}