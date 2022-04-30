using Microsoft.CodeAnalysis;
using StaticCast.Extensions;
using System.CodeDom.Compiler;
using System.Reflection;

namespace StaticCast.Builders;

internal static class StaticCastMembersBuilder
{
	internal static void Build(IndentedTextWriter writer, NamespaceGatherer gatherer,
		 List<IMethodSymbol> membersToGenerate)
	{
		gatherer.Add(typeof(BindingFlags));

		// TODO: For now, I'm assuming the tests will be very simple -
		// i.e. no generics, no outs or refs, etc.
		foreach (var memberToGenerate in membersToGenerate)
		{
			var returnType = memberToGenerate.ReturnsVoid ? "void" : 
				memberToGenerate.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			var parameters = string.Join(", ", memberToGenerate.Parameters.Select(
				_ => $"{_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {_.Name}"));
			var parameterTypes = memberToGenerate.Parameters.Length > 0 ?
				"new[] { " + string.Join(", ", memberToGenerate.Parameters.Select(
					_ => $"typeof({_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})")) + " }" :
				"Type.EmptyTypes";
			var parameterNames = memberToGenerate.Parameters.Length > 0 ?
				"new object[] { " + string.Join(", ", memberToGenerate.Parameters.Select(_ => $"{_.Name}")) + " }" :
				"null";
			var shouldReturn = !memberToGenerate.ReturnsVoid ?
				$"return ({memberToGenerate.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})" : string.Empty;

			var code =
				$$"""
				
				public static {{returnType}} {{memberToGenerate.Name}}({{parameters}})
				{
					Verify();
					
					if (typeof(T).IsAssignableTo(typeof(TAs)))
					{
						var interfaceMethod = typeof(TAs).GetMethod(
							"{{memberToGenerate.Name}}", BindingFlags.Public | BindingFlags.Static, {{parameterTypes}})!;
						var targetMethod = GetTargetMethod(interfaceMethod);
						{{shouldReturn}}targetMethod.Invoke(null, {{parameterNames}});
					}
				}
				""";
			writer.WriteLines(code, "\t");
		}
	}
}