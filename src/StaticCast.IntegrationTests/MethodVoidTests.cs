using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class MethodVoidTests
{
	[Test]
	public static void CallWithNoParametersWithImplementingClass()
	{
		StaticCast<MethodVoid, IMethodVoid>.Void.NoParameters();
		Assert.That(MethodVoid.WasNoParametersInvoked, Is.True);
	}

	[Test]
	public static void CallWithNoParametersWithNonImplementingClass()
	{
		StaticCast<NotImplementingIMethodVoid, IMethodVoid>.Void.NoParameters();
		Assert.That(NotImplementingIMethodVoid.WasNoParametersInvoked, Is.False);
	}

	[Test]
	public static void CallWithMultipleParametersWithImplementingClass()
	{
		StaticCast<MethodVoid, IMethodVoid>.Void.MultipleParameters("a", 2);
		Assert.That(MethodVoid.WasMultipleParametersInvoked, Is.True);
	}

	[Test]
	public static void CallWithMultipleParametersWithNonImplementingClass()
	{
		StaticCast<NotImplementingIMethodVoid, IMethodVoid>.Void.MultipleParameters("a", 2);
		Assert.That(NotImplementingIMethodVoid.WasMultipleParametersInvoked, Is.False);
	}

	public interface IMethodVoid
	{
		static abstract void NoParameters();
		static abstract void MultipleParameters(string a, int b);
	}

	public sealed class NotImplementingIMethodVoid
	{
		public static void MultipleParameters(string a, int b) =>
			NotImplementingIMethodVoid.WasMultipleParametersInvoked = true;

		public static void NoParameters() =>
			NotImplementingIMethodVoid.WasNoParametersInvoked = true;

		public static bool WasMultipleParametersInvoked { get; private set; }

		public static bool WasNoParametersInvoked { get; private set; }
	}

	public sealed class MethodVoid
		: IMethodVoid
	{
		public static void MultipleParameters(string a, int b) =>
			MethodVoid.WasMultipleParametersInvoked = true;

		public static void NoParameters() =>
			MethodVoid.WasNoParametersInvoked = true;

		public static bool WasMultipleParametersInvoked { get; private set; }

		public static bool WasNoParametersInvoked { get; private set; }
	}
}