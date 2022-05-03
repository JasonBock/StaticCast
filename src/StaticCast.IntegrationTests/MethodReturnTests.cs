using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class MethodReturnTests
{
	[Test]
	public static void CallWithNoParametersWithImplementingClass()
	{
		var (wasInvoked, result) = StaticCast<MethodReturn, IMethodReturn>.Int.NoParameters();
		Assert.That(wasInvoked, Is.True);
		Assert.That(result, Is.EqualTo(1));
	}

	[Test]
	public static void CallWithNoParametersWithNonImplementingClass()
	{
		var (wasInvoked, _) = StaticCast<NotImplementingIMethodReturn, IMethodReturn>.Int.NoParameters();
		Assert.That(wasInvoked, Is.False);
	}

	[Test]
	public static void CallWithMultipleParametersWithImplementingClass()
	{
		var (wasInvoked, result) = StaticCast<MethodReturn, IMethodReturn>.Int.MultipleParameters("a", 2);
		Assert.That(wasInvoked, Is.True);
		Assert.That(result, Is.EqualTo(2));
	}

	[Test]
	public static void CallWithMultipleParametersWithNonImplementingClass()
	{
		var (wasInvoked, _) = StaticCast<NotImplementingIMethodReturn, IMethodReturn>.Int.MultipleParameters("a", 2);
		Assert.That(wasInvoked, Is.False);
	}

	public interface IMethodReturn
	{
		static abstract int NoParameters();
		static abstract int MultipleParameters(string a, int b);
	}

	public sealed class NotImplementingIMethodReturn
	{
		public static void MultipleParameters(string a, int b) { }

		public static void NoParameters() { }
	}

	public sealed class MethodReturn
		: IMethodReturn
	{
		public static int MultipleParameters(string a, int b) => 2;

		public static int NoParameters() => 1;
	}
}