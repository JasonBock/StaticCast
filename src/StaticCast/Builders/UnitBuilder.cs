using Microsoft.CodeAnalysis.Text;
using StaticCast.Extensions;
using System.CodeDom.Compiler;
using System.Text;

namespace StaticCast.Builders;

internal sealed class UnitBuilder
{
	internal UnitBuilder()
	{
		using var textWriter = new StringWriter();
		using var writer = new IndentedTextWriter(textWriter, "\t");

		var code =
			"""
			public sealed class Unit
			{
				public static Unit Instance { get; } = new();
				
				private Unit() { }
			}
			""";
		writer.WriteLines(code, "\t");

		this.Code = SourceText.From(textWriter.ToString(), Encoding.UTF8);
	}

	internal SourceText Code { get; }
}