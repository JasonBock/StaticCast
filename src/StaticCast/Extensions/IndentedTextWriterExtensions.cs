using System.CodeDom.Compiler;
using System.Reflection;

namespace StaticCast.Extensions;

internal static class IndentedTextWriterExtensions
{
	// This is fragile, but hopefully we can get a public readonly property for this private field
	// in the future.
	internal static string GetTabString(this IndentedTextWriter self) =>
			(string)typeof(IndentedTextWriter).GetField("_tabString", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(self);

	internal static void WriteLines(this IndentedTextWriter self, string content, string templateIndentation, int indentation = 0)
	{
		var tabString = self.GetTabString();

		if (indentation > 0)
		{
			self.Indent += indentation;
		}
	
		foreach (var line in content.Split(new[] { self.NewLine }, StringSplitOptions.None))
		{
			var contentLine = line;

			if (templateIndentation != tabString)
			{
				var foundTemplateIndentationCount = 0;

				while (contentLine.StartsWith(templateIndentation, StringComparison.InvariantCultureIgnoreCase))
				{
					contentLine = contentLine.Substring(templateIndentation.Length);
					foundTemplateIndentationCount++;
				}
				for (var i = 0; i < foundTemplateIndentationCount; i++)
				{
					contentLine = contentLine.Insert(0, tabString);
				}
			}

			self.WriteLine(contentLine);
		}

		if (indentation > 0)
		{
			self.Indent -= indentation;
		}
	}
}