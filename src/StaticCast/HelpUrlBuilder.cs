namespace StaticCast;

internal static class HelpUrlBuilder
{
	internal static string Build(string identifier, string title) =>
		$"https://github.com/JasonBock/StaticCast/tree/main/docs/{identifier}-{title}.md";
}