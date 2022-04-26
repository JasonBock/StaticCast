using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace StaticCast.Tests;

public static class StaticCastGeneratorTests
{
	[Test]
	public static async Task GenerateWhereMethodReturnsVoidWithParameterAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract void Work(string data);
			}

			public static class Test
			{
				public static void CallWork() =>
					StaticCast<object, IWork>.Work("data");
			}
			""";

		var generatedCode =
			"""
			using System;
			using System.Reflection;
			
			#nullable enable
			public static class StaticCast<T, TAs>
			{
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
				
				public static void Work(string data)
				{
					Verify();
					
					if (typeof(T).IsAssignableTo(typeof(TAs)))
					{
						var interfaceMethod = typeof(TAs).GetMethod(
							"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
						var targetMethod = GetTargetMethod(interfaceMethod);
						targetMethod.Invoke(null, new object[] { data });
					}
				}
			}
			
			""";

	  await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[] { (typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhereMethodReturnsVoidNoParametersAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract void Work();
			}

			public static class Test
			{
				public static void CallWork() =>
					StaticCast<object, IWork>.Work();
			}
			""";

		var generatedCode =
			"""
			using System;
			using System.Reflection;
			
			#nullable enable
			public static class StaticCast<T, TAs>
			{
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
				
				public static void Work()
				{
					Verify();
					
					if (typeof(T).IsAssignableTo(typeof(TAs)))
					{
						var interfaceMethod = typeof(TAs).GetMethod(
							"Work", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
						var targetMethod = GetTargetMethod(interfaceMethod);
						targetMethod.Invoke(null, null);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			 new[] { (typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode) },
			 Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}
}