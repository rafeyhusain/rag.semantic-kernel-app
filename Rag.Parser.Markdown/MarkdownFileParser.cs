namespace Rag.Parser.Markdown;

using Rag.Abstractions.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class MarkdownFileParser : IFileParser
{
    public string FilePath { get; private set; } = "";
    public string FileName => Path.GetFileName(FilePath);
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FilePath);
    public List<ParsedBlock> Blocks { get; private set; } = [];

    public void Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        FilePath = filePath;
        var lines = File.ReadAllLines(filePath);
        ParsedBlock? currentBlock = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            int lineNumber = i + 1;

            BlockType? blockType = null;
            string blockText = "";

            if (line.StartsWith("### "))
            {
                blockType = BlockType.H3;
                blockText = line[4..];
            }
            else if (line.StartsWith("## "))
            {
                blockType = BlockType.H2;
                blockText = line[3..];
            }
            else if (line.StartsWith("# "))
            {
                blockType = BlockType.H1;
                blockText = line[2..];
            }
            else if (Regex.IsMatch(line, @"^\*\*\d+\s§\*\*"))  // e.g., "**1 §**"
            {
                blockType = BlockType.Section;
                blockText = Regex.Match(line, @"^\*\*(\d+\s§)\*\*").Groups[1].Value;
            }
            else if (Regex.IsMatch(line, @"^\*\*.+?\*\*"))
            {
                blockType = BlockType.BoldParagraph;
                blockText = Regex.Match(line, @"^\*\*(.+?)\*\*").Groups[1].Value;
            }

            if (blockType.HasValue)
            {
                currentBlock = new ParsedBlock(blockText, blockType.Value, lineNumber);
                Blocks.Add(currentBlock);
            }
            else if (currentBlock != null)
            {
                // Accumulate content under the current block
                currentBlock.Content += lines[i] + Environment.NewLine;
            }
        }
    }
}
