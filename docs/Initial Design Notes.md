# Introduction

This source generator will allow developers to call implementations of static abstract members. This is a new feature coming in C# 11, and while this may be implemented into the language at some point in the future, this generator is an attempt to fill in that gap for now.

## Scenario

Let's say you had this in code:

```
public static class Test
{
    public static void CheckForString(object value)
    {
        if(value is string data)
        {
            // Do something with data that's specific
            // for a string
            var split = data.Split(',');
        }
    }
}
```

This is an example where the instance that's given to the method parameter has a more specific type than the one declared for that parameter. We want to use members on that instance that are defined on that specific type, so we make a cast.

While this example is somewhat contrived, this situation occurs in .NET. For example, in a source generator, you have to define methods that take a `SyntaxNode` for one of the parameters. However, you're typically looking for a specific kind of `SyntaxNode`, and you're probably going to use members on that node kind. Therefore, you cast it to something like `InvocationExpressionSyntax`.

This feature has been around since C# 1. However, in C# 11, a new feature, static abstract members in interfaces, allows you to define static members on an interface that must be implemented in a class:

```
public interface IWork
{
    static abstract void Work();
}

public class Work
	: IWork
{
	public static void Work(string data) { /* ... */ }
}
```

Of course, there may be multiple implementations of `IWork`. You'd like to be able to call them without having to create a method that has a constraint on the generic type that it must be of an `IWork` type:

```
public static void Work<T>()
{
	/* This won't work right now.
	if(T is TWork work)
	{
		// Do something specific with work
		// that's specific for an IWork implementation
		work.Work("stuff");
	}
	*/
}
```

Unfortunately, that won't work. You can't cast generic parameter values like that. This might be done in the language in the future, but there are no plans to support this in C# 11.

However, with source generators (and a bit of Reflection) we can do this.

## Using Source Generators

Essentially, this is what the user will type in:

```
StaticCast<Type, InterfaceType>.MethodName(...);
```

With the `IWork/Work` example above, it would be this (assume `T` is a generic parameter from a containing method or type):

```
StaticCast<T, IWork>.Work("data");
```

The generator looks for method invocations that is a `SimpleMemberAccessExpression`, where the first part has the name "StaticCast", very similar to what I did with PartiallyApplied. This member needs exactly 2 generic parameters. The name of the call is what's used later (the parameters are irrelevant for generation as you'll see in a moment). If the 2nd type is an interface (we can't do that with a constraint, and if the type is "open" we can't check it with a diagnostic either), and it has static abstract members that match the provided name, we can generate all calls that match the name of the method. Internally it'll do a lookup via reflection (and make the generic method if needed) and invoke it.

If the 2nd generic parameter is "open" and doesn't have enough constraint information to determine if it **can** be of an interface type, I should check this at runtime and throw `NotSupportedException`.

Using the InterfaceMapping type makes it straightforward to handle if the member implementation is explicit or not.

So, we only generate members that are `public`, `static` and `abstract`.

## Generation

First, I'll need to generate a class called `StaticCast`:

```
public static class StaticCast<T, TAs>
```

I may end up making this `partial`. Not sure if it's necessary at this point. I'll also need the `NamespaceGatherer` to ensure I create the right `using` statements. I'll also need a `#nullable enable;`. I won't put `StaticCast` into a namespace, that should make it easier to use.

It may seem odd at first that there isn't a constraint like `where T : TAs`. The reason is that the "cast" is being done at runtime, so I can't constrain the type like that. This will make more sense when I show the implementation.

Next, I look for every member on the interface with the given name. I don't care about any parameters, both generic and for the method itself. For each member, I'll generate a method that looks like this:

```
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
```

There are two helper methods, `Verify()` and `GetTargetMethod()`, that need to be generated as well:

```
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
```

It may not be necessary to check `TAs` to be an interface at runtime, as code won't be generated if I couldn't determine if what was passed into `TAs` was an interface in the first place. The checks for `T` are needed as we won't be able to consistently resolve this at compile time.

```
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

	if(targetMethod is null)
	{
		// Note: if T is abstract, it still must implement
		// static abstract members from TAs as you can't have
		// static abstract members in a class. So this would be
		// really odd for this to occur
		throw new NotSupportedException(
			$"{typeof(TAs).FullName} does not have a mapping for {interfaceMethod.Name} on type {typeof(T).FullName}");
	}

	return targetMethod!;
}
```

When `GetTargetMethod()` is called, I've already verified that `T` implements `TAs`, so not finding a method map would be highly unusual.

Properties will also work in a similar fashion. Let's say our target interface has a static abstract read-only property called `Name` that returns a `string`. In that case, it would be called like this:

```
StaticCast<TargetTypeName, InterfaceName>.Name;
```

Here's what that would generate:

```
public static string Name
{
	get
	{
		Verify();

		if (typeof(T).IsAssignableTo(typeof(TAs)))
		{
			var interfaceMethod = typeof(TAs).GetProperty(
				"Name", BindingFlags.Public | BindingFlags.Static)!.GetGetMethod()!;
			var targetMethod = GetTargetMethod(interfaceMethod);
			return (string)targetMethod.Invoke(null, null)!;
		}
		else
		{
			return default!;
		}
	}
}
```

I look for either the getter or setter method, find the mapped method, and invoke that. Indexers would work in a very similar fashion - it's basically a combination of a method and a property.

For methods and properties that return a value, it may be desirable to know if a call actually occurred. Returning `default` may be what the underlying method does as well. Generating something like an `Option<>` type and using that for the return type might address this issue. [This library](https://github.com/louthy/language-ext), or [this library](https://github.com/la-yumba/functional-csharp-code/), may give me a starting point. Though I need something very simplistic: either you didn't call anything, or you returned something.

Operators are an unknown right now. With static abstract members in interfaces, you can declare an operator. However, the way generics work with the operator definition, it doesn't seem like it can handled with this `StaticCast<,>` approach. For now, I'm not going to handle operators.

When I'm generating implementations in `StaticCast<,>`, I can't repeat them. For example, there may be two interfaces that have the same members in terms of their signature. Generating duplicates will cause a compilation error in the generated code. Therefore, I need to ensure I only create a member in `StaticCast<,>` if I haven't done it before. Maybe the `.ToString()` call on a `IMethodSymbol` will provide that for me automatically. We'll see. At some point, I'll have to figure out what the signature should be with all the `ref`, `out`, generic parameters, constraints, etc. in place.

## Diagnostics

* If a member is not `public`, `static`, and `abstract`.
* If the `StaticCast` usage does not provide exactly 2 generic parameters.
* If the 2nd generic parameter to `StaticCast` is not an interface, or, if it's an open generic, it cannot be determined if it is constrained to an interface.
* If the given member name isn't found on `TAs`