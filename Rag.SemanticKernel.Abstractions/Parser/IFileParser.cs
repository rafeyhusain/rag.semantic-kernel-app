namespace Rag.SemanticKernel.Abstractions.Parser;

public interface IFileParser
{
    string FileName { get; }
    string FileNameWithoutExtension { get; }
    string FilePath { get; }
    List<ParsedBlock> Blocks { get; }

    void Parse(string filePath);
}