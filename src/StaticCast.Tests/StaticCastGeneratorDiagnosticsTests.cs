using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using StaticCast.Diagnostics;

namespace StaticCast.Tests;

public static class StaticCastGeneratorDiagnosticsTests
{
	[Test]
	public static async Task GenerateWhenAsTypeIsInterfaceWithNoStaticAbstractMembersAsync()
	{
		var code =
			"""
			public interface IThing 
			{ 
				void Foo();
			}

			public static class Test
			{
				public static void CallWork()
				{
					StaticCast<object, IThing>.Void.Foo();
				}
			}
			""";

		var diagnostic = new DiagnosticResult(InterfaceHasNoStaticAbstractMembersDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(10, 22, 10, 28);
		var compilerDiagnostic = new DiagnosticResult("CS0307", DiagnosticSeverity.Error)
			.WithSpan(10, 3, 10, 29);
		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic, compilerDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenAsTypeIsConstraintedToAClassAsync()
	{
		var code =
			"""
			public class Thing { }

			public static class Test<T>
				where T : Thing
			{
				public static void CallWork()
				{
					StaticCast<object, T>.Void.Work("data");
				}
			}
			""";

		var diagnostic = new DiagnosticResult(GenericParameterIsNotInterfaceDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(8, 22, 8, 23);
		var compilerDiagnostic = new DiagnosticResult("CS0307", DiagnosticSeverity.Error)
			.WithSpan(8, 3, 8, 24);
		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic, compilerDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenAsTypeHasClassConstraintAsync()
	{
		var code =
			"""
			public static class Test<T>
				where T : class
			{
				public static void CallWork()
				{
					StaticCast<object, T>.Void.Work("data");
				}
			}
			""";

		var diagnostic = new DiagnosticResult(GenericParameterIsNotInterfaceDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(6, 22, 6, 23);
		var compilerDiagnostic = new DiagnosticResult("CS0307", DiagnosticSeverity.Error)
			.WithSpan(6, 3, 6, 24);
		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic, compilerDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenAsTypeIsAClassAsync()
	{
		var code =
			"""
			public static class Test
			{
				public static void CallWork()
				{
					StaticCast<object, string>.Void.Work("data");
				}
			}
			""";

		var diagnostic = new DiagnosticResult(GenericParameterIsNotInterfaceDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(5, 22, 5, 28);
		var compilerDiagnostic = new DiagnosticResult("CS0307", DiagnosticSeverity.Error)
			.WithSpan(5, 3, 5, 29);
		await TestAssistants.RunAsync<StaticCastGenerator>(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic, compilerDiagnostic }).ConfigureAwait(false);
	}
}