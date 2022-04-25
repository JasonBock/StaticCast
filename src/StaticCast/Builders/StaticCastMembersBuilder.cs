using Microsoft.CodeAnalysis;
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
			writer.WriteLine();
			var returnType = memberToGenerate.ReturnsVoid ? "void" : memberToGenerate.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			var parameters = string.Join(", ", memberToGenerate.Parameters.Select(
				_ => $"{_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {_.Name}"));
			writer.WriteLine($"public static {returnType} {memberToGenerate.Name}({parameters})");
			writer.WriteLine("{");
			writer.Indent++;
			writer.WriteLine("Verify();");
			writer.WriteLine();
			writer.WriteLine("if (typeof(T).IsAssignableTo(typeof(TAs)))");
			writer.WriteLine("{");
			writer.Indent++;
			writer.WriteLine("var interfaceMethod = typeof(TAs).GetMethod(");
			writer.Indent++;
			var parameterTypes = string.Join(", ", memberToGenerate.Parameters.Select(
				_ => $"typeof({_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})"));
			writer.WriteLine($"\"{memberToGenerate.Name}\", BindingFlags.Public | BindingFlags.Static, new[] {{ {parameterTypes} }})!;");
			writer.Indent--;
			writer.WriteLine("var targetMethod = GetTargetMethod(interfaceMethod);");
			var parameterNames = string.Join(", ", memberToGenerate.Parameters.Select(
				_ => $"{_.Name}"));
			var shouldReturn = !memberToGenerate.ReturnsVoid ? 
				$"return ({memberToGenerate.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)})" : string.Empty;
			writer.WriteLine($"{shouldReturn}targetMethod.Invoke(null, new object[] {{ {parameterNames} }});");
			writer.Indent--;
			writer.WriteLine("}");
			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}