using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Reflection;

namespace StaticCast.Builders;

internal static class StaticCastMembersBuilder
{
	internal static void Build(IndentedTextWriter writer, NamespaceGatherer gatherer,
		  ImmutableDictionary<ITypeSymbol, HashSet<MethodSymbolSignature>> membersToGenerate)
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
				var returnValueForInvocation = !signature.Method.ReturnsVoid ?
					$"return (true, ({signature.Method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})result!)" : 
					"return (true, Unit.Instance)";
				var returnForNoInvocation =
					!signature.Method.ReturnsVoid ? "return (false, default!)" : "return (false, Unit.Instance)";

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
							var result = targetMethod.Invoke(null, {{parameterNames}});
							{{returnValueForInvocation}};
						}
						
						{{returnForNoInvocation}};
					}
					""";
				writer.WriteLines(code, "\t");
			}

			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}