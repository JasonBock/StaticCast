using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Reflection;

namespace StaticCast.Builders;

internal static class StaticCastMethodsBuilder
{
	internal static void Build(IndentedTextWriter writer, NamespaceGatherer gatherer,
		  ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> methodsToGenerate)
	{
		gatherer.Add(typeof(BindingFlags));

		foreach(var methodToGenerate in methodsToGenerate)
		{
			writer.WriteLine();
			writer.WriteLine($"public static partial class {methodToGenerate.Key.GetName(TypeNameOption.Flatten).ToPascalCase()}");
			writer.WriteLine("{");
			writer.Indent++;

			foreach(var signature in methodToGenerate.Value)
			{
				// TODO: what about generic parameter names?
				var interfaceMethodVariable = VariableNameGenerator.GenerateUniqueName("interfaceMethod", signature.Method.Parameters);
				var targetMethodVariable = VariableNameGenerator.GenerateUniqueName("targetMethod", signature.Method.Parameters);

				// TODO: For now, I'm assuming the tests will be very simple -
				// i.e. no generics, no outs or refs, etc.
				var parameterTypes = signature.Method.Parameters.Length > 0 ?
					"new[] { " + string.Join(", ", signature.Method.Parameters.Select(
						_ => $"typeof({_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})")) + " }" :
					"Type.EmptyTypes";
				var parameterNames = signature.Method.Parameters.Length > 0 ?
					"new object[] { " + string.Join(", ", signature.Method.Parameters.Select(_ => $"{_.Name}")) + " }" :
					"null";

				if(signature.Method.ReturnsVoid)
				{
					var code =
						$$"""
						{{signature.Signature}}
						{
							Verify();
							
							if (typeof(T).IsAssignableTo(typeof(TAs)))
							{
								var {{interfaceMethodVariable}} = typeof(TAs).GetMethod(
									"{{signature.Method.Name}}", BindingFlags.Public | BindingFlags.Static, {{parameterTypes}})!;
								var {{targetMethodVariable}} = GetTargetMethod({{interfaceMethodVariable}});
								{{targetMethodVariable}}.Invoke(null, {{parameterNames}});
							}
						}
						""";
					writer.WriteLines(code, "\t");
				}
				else
				{
					var resultVariable = VariableNameGenerator.GenerateUniqueName("result", signature.Method.Parameters);
					var returnValueForInvocation =
						$"return ({signature.Method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}){resultVariable}!";

					var code =
						$$"""
						{{signature.Signature}}
						{
							Verify();
							
							if (typeof(T).IsAssignableTo(typeof(TAs)))
							{
								var {{interfaceMethodVariable}} = typeof(TAs).GetMethod(
									"{{signature.Method.Name}}", BindingFlags.Public | BindingFlags.Static, {{parameterTypes}})!;
								var {{targetMethodVariable}} = GetTargetMethod({{interfaceMethodVariable}});
								var {{resultVariable}} = {{targetMethodVariable}}.Invoke(null, {{parameterNames}});
								{{returnValueForInvocation}};
							}
							
							return default!;
						}
						""";
					writer.WriteLines(code, "\t");
				}
			}

			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}