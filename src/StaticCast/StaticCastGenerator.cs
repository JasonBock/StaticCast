using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace StaticCastGenerator;

[Generator]
internal sealed class StaticCastGenerator
	: IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) => false;

		static object TransformTargets(GeneratorSyntaxContext context, CancellationToken token)
		{
			return new();
		}

		var provider = context.SyntaxProvider
			.CreateSyntaxProvider(IsSyntaxTargetForGeneration, TransformTargets)
			.Where(static _ => _ is not null);
		var compilationNodes = context.CompilationProvider.Combine(provider.Collect());
		var output = context.AnalyzerConfigOptionsProvider.Combine(compilationNodes);
		context.RegisterSourceOutput(output,
			(context, source) => CreateOutput(source.Right.Left, source.Right.Right, source.Left, context));
	}

	private static void CreateOutput(Compilation compilation, ImmutableArray<object> symbols,
		AnalyzerConfigOptionsProvider options, SourceProductionContext context)
	{
	}
}