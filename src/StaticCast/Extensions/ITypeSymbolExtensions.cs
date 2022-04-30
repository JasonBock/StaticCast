using Microsoft.CodeAnalysis;

namespace StaticCast.Extensions;

internal static class ITypeSymbolExtensions
{
	// "Borrowed" from Rocks.
	internal static string GetName(this ITypeSymbol self, TypeNameOption options = TypeNameOption.IncludeGenerics)
	{
		static string GetFlattenedName(INamedTypeSymbol flattenedName, TypeNameOption flattenedOptions)
		{
			if (flattenedName.TypeArguments.Length == 0)
			{
				return flattenedName.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}
			else
			{
				return $"{flattenedName.Name}Of{string.Join("_", flattenedName.TypeArguments.Select(_ => _.GetName(flattenedOptions)))}";
			}
		}

		if (options == TypeNameOption.IncludeGenerics)
		{
			return self.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
		}
		else if (options == TypeNameOption.Flatten)
		{
			if (self.Kind == SymbolKind.PointerType)
			{
				return self.ToDisplayString().Replace("*", "Pointer");
			}
			else if (self.Kind == SymbolKind.FunctionPointerType)
			{
				// delegate* unmanaged[Stdcall, SuppressGCTransition] <int, int>;
				return self.ToDisplayString().Replace("*", "Pointer").Replace(" ", "_")
					.Replace("[", "_").Replace(",", "_").Replace("]", "_")
					.Replace("<", "Of").Replace(">", string.Empty);
			}
			else if (self is INamedTypeSymbol namedSelf)
			{
				return GetFlattenedName(namedSelf, options);
			}
			else
			{
				return self.Name;
			}
		}
		else
		{
			return self.Kind == SymbolKind.PointerType || self.Kind == SymbolKind.FunctionPointerType ?
				self.ToDisplayString() : self.Name;
		}
	}
}