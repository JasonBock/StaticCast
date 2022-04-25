using System.CodeDom.Compiler;
using System.Reflection;

namespace StaticCast.Builders;

internal static class StaticCastHelpersBuilder
{
	internal static void Build(IndentedTextWriter writer, NamespaceGatherer gatherer)
	{
		StaticCastHelpersBuilder.BuildVerify(writer, gatherer);
		writer.WriteLine();
		StaticCastHelpersBuilder.BuildGetTargetMethod(writer, gatherer);
	}

	private static void BuildVerify(IndentedTextWriter writer, NamespaceGatherer gatherer)
	{
		gatherer.Add(typeof(NotSupportedException));

		writer.WriteLine("private static void Verify()");
		writer.WriteLine("{");
		writer.Indent++;

		writer.WriteLine("var tType = typeof(T);");
		writer.WriteLine();
		writer.WriteLine("if (tType.IsInterface)");
		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine("throw new NotSupportedException($\"The T type, {tType.FullName}, is an interface.\");");
		writer.Indent--;
		writer.WriteLine("}");
		writer.WriteLine("else if (tType.IsAbstract)");
		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine("throw new NotSupportedException($\"The T type, {tType.FullName}, is abstract.\");");
		writer.Indent--;
		writer.WriteLine("}");
		writer.WriteLine();
		writer.WriteLine("var asType = typeof(TAs);");
		writer.WriteLine();
		writer.WriteLine("if (!asType.IsInterface)");
		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine("throw new NotSupportedException($\"The TAs type, {asType.FullName}, is not an interface.\");");
		writer.Indent--;
		writer.WriteLine("}");

		writer.Indent--;
		writer.WriteLine("}");
	}

	private static void BuildGetTargetMethod(IndentedTextWriter writer, NamespaceGatherer gatherer)
	{
		gatherer.Add(typeof(MethodInfo));
		gatherer.Add(typeof(NotSupportedException));

		writer.WriteLine("var interfaceMap = typeof(T).GetInterfaceMap(typeof(TAs));");
		writer.WriteLine();
		writer.WriteLine("MethodInfo? targetMethod = null;");
		writer.WriteLine();
		writer.WriteLine("for (var i = 0; i < interfaceMap.InterfaceMethods.Length; i++)");
		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine("if (interfaceMap.InterfaceMethods[i] == interfaceMethod)");
		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine("targetMethod = interfaceMap.TargetMethods[i]!;");
		writer.Indent--;
		writer.WriteLine("}");
		writer.Indent--;
		writer.WriteLine("}");
		writer.WriteLine();
		writer.WriteLine("if (targetMethod is null)");
		writer.WriteLine("{");
		writer.Indent++;
		// Note: if T is abstract, it still must implement
		// static abstract members from TAs as you can't have
		// static abstract members in a class. So this would be
		// really odd for this to occur.
		writer.WriteLine("throw new NotSupportedException(");
		writer.Indent++;
		writer.WriteLine("$\"{typeof(TAs).FullName} does not have a mapping for {interfaceMethod.Name} on type {typeof(T).FullName}\");");
		writer.Indent--;
		writer.Indent--;
		writer.WriteLine("}");
		writer.WriteLine();
		writer.WriteLine("return targetMethod!;");
	}
}