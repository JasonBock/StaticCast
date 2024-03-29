﻿using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace StaticCast.Tests;

public static class StaticCastGeneratorTests
{
	[Test]
	public static async Task GenerateWhenParameterNamesCollideWithLocalVariablesAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract int Work(string interfaceMethod, string _interfaceMethod, string targetMethod, string result);
			}

			public static class Test
			{
				public static int CallWork() =>
					StaticCast<object, IWork>.Int.Work("a", "b", "c", "d");
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
				
				public static partial class Int
				{
					public static int Work(string interfaceMethod, string _interfaceMethod, string targetMethod, string result)
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var __interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string), typeof(string), typeof(string), typeof(string) })!;
							var _targetMethod = GetTargetMethod(__interfaceMethod);
							var _result = _targetMethod.Invoke(null, new object[] { interfaceMethod, _interfaceMethod, targetMethod, result });
							return (int)_result!;
						}
						
						return default!;
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			 new[]
			 {
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			 },
			 Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

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
				
				public static partial class Void
				{
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
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			 new[]
			 {
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
				
				public static partial class Void
				{
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
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
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
				public static int CallWork() =>
					StaticCast<object, IWork>.Int.Work("data");
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
				
				public static partial class Int
				{
					public static int Work(string data)
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Work", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							var result = targetMethod.Invoke(null, new object[] { data });
							return (int)result!;
						}
						
						return default!;
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
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
				
				public static partial class Void
				{
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
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
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
				
				public static partial class Void
				{
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
					public static void Rest()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Rest", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							targetMethod.Invoke(null, null);
						}
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
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
				
				public static partial class Void
				{
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
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
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
				
				public static partial class Void
				{
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
					public static void Drive()
					{
						Verify();
						
						if (typeof(T).IsAssignableTo(typeof(TAs)))
						{
							var interfaceMethod = typeof(TAs).GetMethod(
								"Drive", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;
							var targetMethod = GetTargetMethod(interfaceMethod);
							targetMethod.Invoke(null, null);
						}
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			new[]
			{
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWithPropertyAsync()
	{
		var code =
			"""
			public interface IWork
			{
				static abstract string Name { get; set; }
			}

			public static class Test
			{
				public static void UseName()
				{
					var name = StaticCast<object, IWork>.String.Name;
					StaticCast<object, IWork>.String.Name = name;
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
				
				public static partial class String
				{
					public static string Name
					{
						get
						{
							Verify();
							
							if (typeof(T).IsAssignableTo(typeof(TAs)))
							{
								var interfaceMethod = typeof(TAs).GetProperty("Name")!.GetGetMethod()!;
								var targetMethod = GetTargetMethod(interfaceMethod);
								var result = targetMethod.Invoke(null, null);
								return (string)result!;
							}
							
							return default!;
						}
						set
						{
							Verify();
							
							if (typeof(T).IsAssignableTo(typeof(TAs)))
							{
								var interfaceMethod = typeof(TAs).GetProperty("Name")!.GetSetMethod()!;
								var targetMethod = GetTargetMethod(interfaceMethod);
								targetMethod.Invoke(null, new object[] { value });
							}
						}
					}
				}
			}
			
			""";

		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			 new[]
			 {
				(typeof(StaticCastGenerator), "StaticCast.g.cs", generatedCode),
			 },
			 Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}
}