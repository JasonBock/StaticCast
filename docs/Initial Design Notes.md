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

# Using Source Generators

Essentially, this is what the user will type in:

```
StaticCast<Type, InterfaceType>.MethodName(...);
```

With the `IWork/Work` example above, it would be this:

```
StaticCast<Work, IWork>.Work("data");
```

The generator looks for method invocations that have the name "StaticCast", very similar to what I did with PartiallyApplied. This call needs exactly 2 generic parameters. The name of the call is what's used later (the parameters are irrelevant for generation as you'll see in a moment).

If the 2nd type is an interface (we can't do that with a constraint, and if the type is "open" we can't check it with a diagnostic either), we can generate all calls that match the name of the method. Internally it'll do a lookup via reflection (and make the generic method if needed) and invoke it. If it returns a value, I may need the concept of a Maybe<T> type.

If the 2nd generic parameter is "open" and not an interface, I should check this at runtime and throw NotSupportedException.

Using the InterfaceMapping type makes it straightforward to handle if the member implementation is explicit or not.