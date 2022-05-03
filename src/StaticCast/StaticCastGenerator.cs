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
		context.RegisterPostInitializationOutput(context =>
			context.AddSource("Unit.g.cs", new UnitBuilder().Code));
		context.RegisterSourceOutput(output,
			(context, source) => CreateOutput(source.Right.Left, source.Right.Right, source.Left, context));
	}

	private static void CreateOutput(Compilation compilation, ImmutableArray<MemberAccessExpressionSyntax> accessNodes,
		AnalyzerConfigOptionsProvider options, SourceProductionContext context)
	{
		var information = new StaticCastInformation(accessNodes, compilation);

		foreach(var diagnostic in information.Diagnostics)
		{
			context.ReportDiagnostic(diagnostic);
		}
	
		if (information.MembersToGenerate.Count > 0)
		{
			var builder = new StaticCastBuilder(information.MembersToGenerate);
			context.AddSource("StaticCast.g.cs", builder.Code);
		}
	}
}