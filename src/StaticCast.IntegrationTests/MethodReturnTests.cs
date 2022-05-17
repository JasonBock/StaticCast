using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class MethodReturnTests
{
	[Test]
	public static void CallWithNoParametersWithImplementingClass()
	{
		var result = StaticCast<ImplementsIMethodReturn, IMethodReturn>.Int.NoParameters();
		Assert.That(result, Is.EqualTo(1));
	}

	[Test]
	public static void CallWithNoParametersWithNonImplementingClass()
	{
		var result = StaticCast<NotImplementingIMethodReturn, IMethodReturn>.Int.NoParameters();
		Assert.That(result, Is.EqualTo(0));
	}

	[Test]
	public static void CallWithMultipleParametersWithImplementingClass()
	{
		var result = StaticCast<ImplementsIMethodReturn, IMethodReturn>.Int.MultipleParameters("a", 2);
		Assert.That(result, Is.EqualTo(2));
	}

	[Test]
	public static void CallWithMultipleParametersWithNonImplementingClass()
	{
		var result = StaticCast<NotImplementingIMethodReturn, IMethodReturn>.Int.MultipleParameters("a", 2);
		Assert.That(result, Is.EqualTo(0));
	}

	public interface IMethodReturn
	{
		static abstract int NoParameters();
		static abstract int MultipleParameters(string a, int b);
	}

	public sealed class NotImplementingIMethodReturn
	{
		public static int MultipleParameters(string a, int b) => -2;

		public static int NoParameters() => -1;
	}

	public sealed class ImplementsIMethodReturn
		: IMethodReturn
	{
		public static int MultipleParameters(string a, int b) => 2;

		public static int NoParameters() => 1;
	}
}