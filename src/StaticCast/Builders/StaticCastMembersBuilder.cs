using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.CodeDom.Compiler;
using System.Reflection;

namespace StaticCast.Builders;

internal static class StaticCastMembersBuilder
{
	internal static void Build(IndentedTextWriter writer, NamespaceGatherer gatherer,
		  Dictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> membersToGenerate)
	{
		gatherer.Add(typeof(BindingFlags));

		foreach(var memberToGenerate in membersToGenerate)
		{
			writer.WriteLine();
			writer.WriteLine($"public static class {memberToGenerate.Key.GetName(TypeNameOption.Flatten).ToPascalCase()}");
			writer.WriteLine("{");
			writer.Indent++;

			foreach(var signature in memberToGenerate.Value)
			{
				// TODO: For now, I'm assuming the tests will be very simple -
				// i.e. no generics, no outs or refs, etc.
				var parameterTypes = signature.Method.Parameters.Length > 0 ?
					"new[] { " + string.Join(", ", signature.Method.Parameters.Select(
						_ => $"typeof({_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})")) + " }" :
					"Type.EmptyTypes";
				var parameterNames = signature.Method.Parameters.Length > 0 ?
					"new object[] { " + string.Join(", ", signature.Method.Parameters.Select(_ => $"{_.Name}")) + " }" :
					"null";
				var shouldReturn = !signature.Method.ReturnsVoid ?
					$"return ({signature.Method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})" : string.Empty;
				var returnForNoAssignment =
					signature.Method.ReturnsVoid ? string.Empty : "return default!;";
				var suppresionOperator =
					signature.Method.ReturnsVoid ? string.Empty : "!";

				var code =
					$$"""
					{{signature.Signature}}
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"{{signature.Method.Name}}", BindingFlags.Public | BindingFlags.Static, {{parameterTypes}})!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							{{shouldReturn}}targetMethod.Invoke(null, {{parameterNames}}){{suppresionOperator}};
						}
						{{returnForNoAssignment}}
					}
					""";
				writer.WriteLines(code, "\t");
			}

			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}