using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class MethodVoidTests
{
	[Test]
	public static void CallWithNoParametersWithImplementingClass()
	{
		var (wasInvoked, result) = StaticCast<MethodVoid, IMethodVoid>.Void.NoParameters();
		Assert.That(wasInvoked, Is.True);
		Assert.That(result, Is.SameAs(Unit.Instance));
	}

	[Test]
	public static void CallWithNoParametersWithNonImplementingClass()
	{
		var (wasInvoked, result) = StaticCast<NotImplementingIMethodVoid, IMethodVoid>.Void.NoParameters();
		Assert.That(wasInvoked, Is.False);
		Assert.That(result, Is.SameAs(Unit.Instance));
	}

	[Test]
	public static void CallWithMultipleParametersWithImplementingClass()
	{
		var (wasInvoked, result) = StaticCast<MethodVoid, IMethodVoid>.Void.MultipleParameters("a", 2);
		Assert.That(wasInvoked, Is.True);
		Assert.That(result, Is.SameAs(Unit.Instance));
	}

	[Test]
	public static void CallWithMultipleParametersWithNonImplementingClass()
	{
		var (wasInvoked, result) = StaticCast<NotImplementingIMethodVoid, IMethodVoid>.Void.MultipleParameters("a", 2);
		Assert.That(wasInvoked, Is.False);
		Assert.That(result, Is.SameAs(Unit.Instance));
	}

	public interface IMethodVoid
	{
		static abstract void NoParameters();
		static abstract void MultipleParameters(string a, int b);
	}

	public sealed class NotImplementingIMethodVoid
	{
		public static void MultipleParameters(string a, int b) { }

		public static void NoParameters() { }
	}

	public sealed class MethodVoid
		: IMethodVoid
	{
		public static void MultipleParameters(string a, int b) { }

		public static void NoParameters() { }
	}
}