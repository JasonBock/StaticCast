# StaticCast

Casting generic types for static abstract member implementation

## Overview

In C#, we've been able to cast parameters to specific types for a long time:

```
public void Work(object data)
{
  if (data is string content)
  {
    // Do something with content...
  }
}
```

In C# 11, a new feature called [static abstract members in interfaces](https://github.com/dotnet/csharplang/issues/4436) allows a developer to define static members in an interface that must be implemented in a class:

```
public interface IWork
{
  static abstract void Work(string data);
}
```

Unfortunately, we won't be able to do a "static cast" to a generic parameter, something like this:

```
// This won't work, just illustrating the point...
public void Work<T>(string data)
{
  // Do something with the data parameter...
  if(T is IWork TWork)
  {
    TWork.Work(data);
  }
}
```

That's what this library is trying to support!

## Tutorial

To try this out, [install the NuGet package](https://www.nuget.org/packages/StaticCast). Note that your project must be enabled for C# latest features. Then, all you have to do is call `StaticCast`, providing the generic type values along with the member you want to invoke:

```
StaticCast<T, IWork>.Void.Work("value");
```

`StaticCast` is a type that doesn't exist until you type it in code. The source generator looks for that invocation pattern, and will generate the necessary code to invoke the method or property that you specify after the dot.

Right now, this only works for "simple" methods. Over time, support for generic methods, properties (and indexers), and other cases will be supported in the near future. Feel free to peruse the code and let me know if you have any suggestions.