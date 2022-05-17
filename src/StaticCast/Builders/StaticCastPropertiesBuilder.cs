using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Reflection;

namespace StaticCast.Builders;

internal static class StaticCastPropertiesBuilder
{
	internal static void Build(IndentedTextWriter writer, NamespaceGatherer gatherer,
		  ImmutableDictionary<ITypeSymbol, HashSet<PropertySymbolSignature>> propertiesToGenerate)
	{
		gatherer.Add(typeof(BindingFlags));

		foreach(var propertyToGenerate in propertiesToGenerate)
		{
			writer.WriteLine();
			writer.WriteLine($"public static partial class {propertyToGenerate.Key.GetName(TypeNameOption.Flatten).ToPascalCase()}");
			writer.WriteLine("{");
			writer.Indent++;

			foreach(var signature in propertyToGenerate.Value)
			{
				writer.WriteLine(signature.Signature);
				writer.WriteLine("{");
				writer.Indent++;

				if (signature.Property.GetMethod is not null)
				{
					StaticCastPropertiesBuilder.BuildGetter(writer, signature);
				}

				if (signature.Property.SetMethod is not null)
				{
					StaticCastPropertiesBuilder.BuildSetter(writer, signature);
				}

				writer.Indent--;
				writer.WriteLine("}");
			}

			writer.Indent--;
			writer.WriteLine("}");
		}
	}

	private static void BuildGetter(IndentedTextWriter writer, PropertySymbolSignature signature)
	{
		// TODO: what about generic parameter names?
		// TODO: For now, I'm assuming the tests will be very simple -
		// i.e. no generics, no outs or refs, etc.
		var getMethod = signature.Property.GetMethod!;

		var code =
			$$"""
			get
			{
				Verify();
				
				if (typeof(T).IsAssignableTo(typeof(TAs)))
				{
					var interfaceMethod = typeof(TAs).GetProperty("{{signature.Property.Name}}")!.GetGetMethod()!;
					var targetMethod = GetTargetMethod(interfaceMethod);
					var result = targetMethod.Invoke(null, null);
					return ({{getMethod.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}})result!;
				}
				
				return default!;
			}
			""";
		writer.WriteLines(code, "\t");
	}

	private static void BuildSetter(IndentedTextWriter writer, PropertySymbolSignature signature)
	{
		// TODO: what about generic parameter names?
		var setMethod = signature.Property.SetMethod!;

		// TODO: For now, I'm assuming the tests will be very simple -
		// i.e. no generics, no outs or refs, etc.
		var parameterNames =
			"new object[] { " + string.Join(", ", setMethod.Parameters.Select(_ => $"{_.Name}")) + " }";

		var code =
			$$"""
			set
			{
				Verify();
				
				if (typeof(T).IsAssignableTo(typeof(TAs)))
				{
					var interfaceMethod = typeof(TAs).GetProperty("{{signature.Property.Name}}")!.GetSetMethod()!;
					var targetMethod = GetTargetMethod(interfaceMethod);
					targetMethod.Invoke(null, {{parameterNames}});
				}
			}
			""";
		writer.WriteLines(code, "\t");
	}
}