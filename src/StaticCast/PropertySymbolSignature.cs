using Microsoft.CodeAnalysis;

namespace StaticCast;

internal sealed class PropertySymbolSignature
	: IEquatable<PropertySymbolSignature>
{
	internal PropertySymbolSignature(IPropertySymbol property) =>
		(this.Property, this.Signature) = (property, PropertySymbolSignature.GetSignature(property));

	private static string GetSignature(IPropertySymbol symbol)
	{
		var parameterType = symbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
		var name = symbol.Name;
		return $"public static {parameterType} {name}";
	}

	public override bool Equals(object obj) => this.Equals(obj as PropertySymbolSignature);

	public bool Equals(PropertySymbolSignature? other) => other is not null && this.Signature == other.Signature;

	public override int GetHashCode() => this.Signature.GetHashCode();

	public IPropertySymbol Property { get; }
	public string Signature { get; }
}