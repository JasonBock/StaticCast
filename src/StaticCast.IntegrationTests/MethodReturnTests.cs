using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class MethodReturnTests
{
	/*
	[Test]
	public static void CallWithNoParametersWithImplementingClass()
	{
		StaticCast<MethodReturn, IMethodReturn>.NoParameters();
		Assert.That(MethodReturn.WasNoParametersInvoked, Is.True);
	}

	[Test]
	public static void CallWithNoParametersWithNonImplementingClass()
	{
		StaticCast<NotImplementingIMethodVoid, IMethodReturn>.NoParameters();
		Assert.That(NotImplementingIMethodVoid.WasNoParametersInvoked, Is.False);
	}

	[Test]
	public static void CallWithMultipleParametersWithImplementingClass()
	{
		StaticCast<MethodVoid, IMethodReturn>.MultipleParameters("a", 2);
		Assert.That(MethodVoid.WasMultipleParametersInvoked, Is.True);
	}

	[Test]
	public static void CallWithMultipleParametersWithNonImplementingClass()
	{
		StaticCast<NotImplementingIMethodVoid, IMethodReturn>.MultipleParameters("a", 2);
		Assert.That(NotImplementingIMethodVoid.WasMultipleParametersInvoked, Is.False);
	}
	*/

	public interface IMethodReturn
	{
		static abstract int NoParameters();
		static abstract int MultipleParameters(string a, int b);
	}

	public sealed class NotImplementingIMethodReturn
	{
		public static void MultipleParameters(string a, int b) =>
			NotImplementingIMethodReturn.WasMultipleParametersInvoked = true;

		public static void NoParameters() =>
			NotImplementingIMethodReturn.WasNoParametersInvoked = true;

		public static bool WasMultipleParametersInvoked { get; private set; }

		public static bool WasNoParametersInvoked { get; private set; }
	}

	public sealed class MethodReturn
		: IMethodReturn
	{
		public static int MultipleParameters(string a, int b)
		{
			MethodReturn.WasMultipleParametersInvoked = true;
			return 1;
		}

		public static int NoParameters()
		{
			MethodReturn.WasNoParametersInvoked = true;
			return 1;
		}

		public static bool WasMultipleParametersInvoked { get; private set; }

		public static bool WasNoParametersInvoked { get; private set; }
	}
}