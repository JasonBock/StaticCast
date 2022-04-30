using System.CodeDom.Compiler;

namespace StaticCast.Extensions;

internal static class IndentedTextWriterExtensions
{
	internal static void WriteLines(this IndentedTextWriter self, string content, string templateIndentation, string tabString, int indentation = 0)
	{
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