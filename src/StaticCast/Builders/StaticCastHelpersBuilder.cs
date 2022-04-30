using StaticCast.Extensions;
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

		var code =
			"""
			private static void Verify()
			{
				var tType = typeof(T);
				
				if (tType.IsInterface)
				{
					throw new NotSupportedException($"The T type, {tType.FullName}, is an interface.");
				}
				else if (tType.IsAbstract)
				{
					throw new NotSupportedException($"The T type, {tType.FullName}, is abstract.");
				}
				
				var asType = typeof(TAs);
				
				if (!asType.IsInterface)
				{
					throw new NotSupportedException($"The TAs type, {asType.FullName}, is not an interface.");
				}
			}
			""";
		writer.WriteLines(code, "\t", "\t");
	}

	private static void BuildGetTargetMethod(IndentedTextWriter writer, NamespaceGatherer gatherer)
	{
		gatherer.Add(typeof(MethodInfo));
		gatherer.Add(typeof(NotSupportedException));

		// Note: if T is abstract, it still must implement
		// static abstract members from TAs as you can't have
		// static abstract members in a class. So this would be
		// really odd for the exception to occur.
		var code =
			"""
			private static MethodInfo GetTargetMethod(MethodInfo interfaceMethod)
			{
				var interfaceMap = typeof(T).GetInterfaceMap(typeof(TAs));
				
				MethodInfo? targetMethod = null;
				
				for (var i = 0; i < interfaceMap.InterfaceMethods.Length; i++)
				{
					if (interfaceMap.InterfaceMethods[i] == interfaceMethod)
					{
						targetMethod = interfaceMap.TargetMethods[i]!;
					}
				}
				
				if (targetMethod is null)
				{
					throw new NotSupportedException(
						$"{typeof(TAs).FullName} does not have a mapping for {interfaceMethod.Name} on type {typeof(T).FullName}");
				}
				
				return targetMethod!;
			}
			""";
		writer.WriteLines(code, "\t", "\t");
	}
}