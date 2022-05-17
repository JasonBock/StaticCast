using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class MethodVoidTests
{
	[Test]
	public static void CallWithNoParametersWithImplementingClass()
	{
		ImplementsIMethodVoid.WasNoParametersInvoked = false;
		StaticCast<ImplementsIMethodVoid, IMethodVoid>.Void.NoParameters();
		Assert.That(ImplementsIMethodVoid.WasNoParametersInvoked, Is.True);
	}

	[Test]
	public static void CallWithNoParametersWithNonImplementingClass()
	{
		NotImplementingIMethodVoid.WasMultipleParametersInvoked = false;
		StaticCast<NotImplementingIMethodVoid, IMethodVoid>.Void.NoParameters();
		Assert.That(NotImplementingIMethodVoid.WasMultipleParametersInvoked, Is.False);
	}

	[Test]
	public static void CallWithMultipleParametersWithImplementingClass()
	{
		ImplementsIMethodVoid.WasMultipleParametersInvoked = false;
		StaticCast<ImplementsIMethodVoid, IMethodVoid>.Void.MultipleParameters("a", 2);
		Assert.That(ImplementsIMethodVoid.WasMultipleParametersInvoked, Is.True);
	}

	[Test]
	public static void CallWithMultipleParametersWithNonImplementingClass()
	{
		NotImplementingIMethodVoid.WasMultipleParametersInvoked = false;
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

		public static bool WasNoParametersInvoked { get; set; }
		public static bool WasMultipleParametersInvoked { get; set; }
	}

	public sealed class ImplementsIMethodVoid
		: IMethodVoid
	{
		public static void MultipleParameters(string a, int b) =>
			ImplementsIMethodVoid.WasMultipleParametersInvoked = true;

		public static void NoParameters() =>
			ImplementsIMethodVoid.WasNoParametersInvoked = true;

		public static bool WasNoParametersInvoked { get; set; }
		public static bool WasMultipleParametersInvoked { get; set; }
	}
}