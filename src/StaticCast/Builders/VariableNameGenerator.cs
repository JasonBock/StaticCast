using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace StaticCast.Builders;

internal static class VariableNameGenerator
{
	internal static string GenerateUniqueName(string proposedName, ImmutableArray<IParameterSymbol> parameters)
	{
		var name = proposedName;

		while(parameters.Any(_ => _.Name == name))
		{
			name = $"_{name}";
		}

		return name;
	}
}