using Microsoft.CodeAnalysis;

namespace StaticCast;

internal sealed class MethodSymbolSignature
	: IEquatable<MethodSymbolSignature>
{
	internal MethodSymbolSignature(IMethodSymbol member) =>
		(this.Method, this.Signature) = (member, MethodSymbolSignature.GetSignature(member));

	private static string GetSignature(IMethodSymbol symbol)
	{
		var returnType = symbol.ReturnsVoid ? "void" :
			symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
		var parameters = string.Join(", ", symbol.Parameters.Select(
			_ => $"{_.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {_.Name}"));
		return $"public static {returnType} {symbol.Name}({parameters})";
	}

	public override bool Equals(object obj) => this.Equals(obj as MethodSymbolSignature);

	public bool Equals(MethodSymbolSignature? other) => other is null ? false : this.Signature == other.Signature;

	public override int GetHashCode() => this.Signature.GetHashCode();

	public IMethodSymbol Method { get; }
	public string Signature { get; }
}