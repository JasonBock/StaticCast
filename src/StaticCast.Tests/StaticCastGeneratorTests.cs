using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace StaticCast.Tests;

public static class StaticCastGeneratorTests
{
	private const string UnitCode =
		"""
		public sealed class Unit
		{
			public static Unit Instance { get; } = new();
			
			private Unit() { }
		}

		""";

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
					StaticCast<object, IWork>.Void.Work("data");
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
				
				public static class Void
				{
					public static (bool, Unit) Work(string data)
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, new object[] { data });
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			 new[]
			 {
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			 },
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
					StaticCast<object, IWork>.Void.Work();
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
				
				public static class Void
				{
					public static (bool, Unit) Work()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, null);
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhereMethodReturnsIntAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract int Work(string data);
			}

			public static class Test
			{
				public static int CallWork()
				{
					var (_, result) = StaticCast<object, IWork>.Int.Work("data");
					return result;
				}
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
				
				public static class Int
				{
					public static (bool, int) Work(string data)
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, new object[] { data });
							return (true, (int)result!);
						}
						
						return (false, default!);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	// TODO: Test where two interfaces have the same signature.
	// Should only generate one
	[Test]
	public static async Task GenerateWhereTwoMethodsFromTwoInterfacesHaveSameSignatureAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract void Work(string data);
			}

			public interface IExercise
			{
				static abstract void Work(string data);
			}

			public static class Test
			{
				public static void CallIWork() =>
					StaticCast<object, IWork>.Void.Work("data");

				public static void CallIExercise() =>
					StaticCast<object, IExercise>.Void.Work("data");
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
				
				public static class Void
				{
					public static (bool, Unit) Work(string data)
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, new object[] { data });
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhereInterfaceHasMultipleMembersAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract void Work(string data);
				static abstract void Rest();
			}

			public static class Test
			{
				public static void CallIWork() =>
					StaticCast<object, IWork>.Void.Work("data");
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
				
				public static class Void
				{
					public static (bool, Unit) Work(string data)
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, new object[] { data });
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
					public static (bool, Unit) Rest()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Rest", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, null);
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhereInterfaceNeedsToBeInferredAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract void Work();
			}

			public static class Test<T>
				where T : IWork
			{
				public static void CallWork() =>
					StaticCast<object, T>.Void.Work();
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
				
				public static class Void
				{
					public static (bool, Unit) Work()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, null);
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWithMultipleConstraintsAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract void Work();
			}

			public interface ICar
			{
				static abstract void Drive();
			}
			
			public static class Test<T>
				where T : IWork, ICar
			{
				public static void CallWork() =>
					StaticCast<object, T>.Void.Work();
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
				
				public static class Void
				{
					public static (bool, Unit) Work()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, null);
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
					public static (bool, Unit) Drive()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Drive", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, null);
							return (true, Unit.Instance);
						}
						
						return (false, Unit.Instance);
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "Unit.g.cs", StaticCastGeneratorTests.UnitCode),
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}
}