using NUnit.Framework;

namespace StaticCast.IntegrationTests;

public static class PropertyTests
{
	[Test]
	public static void CallWithImplementingClass()
	{
		var name = StaticCast<ImplementsIProperty, IProperty>.String.Name;
		Assert.That(name, Is.EqualTo("name"));
		StaticCast<ImplementsIProperty, IProperty>.String.Name = name;
		name = StaticCast<ImplementsIProperty, IProperty>.String.Name;
		Assert.That(name, Is.EqualTo("namename"));
	}

	[Test]
	public static void CallWithNonImplementingClass()
	{
		var name = StaticCast<NotImplementingIProperty, IProperty>.String.Name;
		Assert.That(name, Is.Null);
		StaticCast<NotImplementingIProperty, IProperty>.String.Name = "value";
		name = StaticCast<NotImplementingIProperty, IProperty>.String.Name;
		Assert.That(name, Is.Null);
	}

	public interface IProperty
	{
		static abstract string Name { get; set; }
	}

	public sealed class NotImplementingIProperty
	{
		public static string? Name { get; set; }
	}

	public sealed class ImplementsIProperty
		: IProperty
	{
		private static string name = "name";

		public static string Name 
		{
			get => ImplementsIProperty.name; 
			set => ImplementsIProperty.name += value; 
		}
	}
}