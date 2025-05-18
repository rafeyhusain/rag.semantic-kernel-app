namespace Rag.SemanticKernel.Core.Sdk.Parser;

public class MarkdownHeading
{
    public string Text { get; set; }
    public HeadingType Type { get; set; }
    public int LineNumber { get; set; }
    public string Content { get; set; } = string.Empty;

    public MarkdownHeading(string text, HeadingType type, int lineNumber)
    {
        Text = text.Trim();
        Type = type;
        LineNumber = lineNumber;
    }
}
