using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;

namespace StaticCast.Builders;

internal sealed class StaticCastBuilder
{
	private readonly ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> methodsToGenerate;
	private readonly ImmutableDictionary<ITypeSymbol, HashSet<PropertySymbolSignature>> propertiesToGenerate;

	internal StaticCastBuilder(ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> methodsToGenerate,
		ImmutableDictionary<ITypeSymbol, HashSet<PropertySymbolSignature>> propertiesToGenerate)
	{
		(this.methodsToGenerate, this.propertiesToGenerate) = (methodsToGenerate, propertiesToGenerate);
		this.Code = SourceText.From(this.Build(), Encoding.UTF8);
	}

	private string Build()
	{
		using var textWriter = new StringWriter();
		using var writer = new IndentedTextWriter(textWriter, "\t");

		var gatherer = new NamespaceGatherer();

		writer.WriteLine($"public static class StaticCast<T, TAs>");
		writer.WriteLine("{");
		writer.Indent++;
		StaticCastHelpersBuilder.Build(writer, gatherer);
		StaticCastMethodsBuilder.Build(writer, gatherer, this.methodsToGenerate);
		StaticCastPropertiesBuilder.Build(writer, gatherer, this.propertiesToGenerate);
		writer.Indent--;
		writer.WriteLine("}");

		return string.Join(Environment.NewLine,
			string.Join(Environment.NewLine, gatherer.Values.Select(_ => $"using {_};")), string.Empty, "#nullable enable", textWriter.ToString());
	}

	public SourceText Code { get; private set; }
}