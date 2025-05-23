namespace Rag.Abstractions.Parser;

public class ParsedBlock
{
    public string Text { get; set; }
    public BlockType Type { get; set; }
    public int LineNumber { get; set; }
    public string Content { get; set; } = string.Empty;

    public ParsedBlock(string text, BlockType type, int lineNumber)
    {
        Text = text.Trim();
        Type = type;
        LineNumber = lineNumber;
    }

    public override string ToString()
    {
        return $"Text: {Text}, Type: {Type}, LineNumber: {LineNumber}, Content: {Content}";
    }
}
