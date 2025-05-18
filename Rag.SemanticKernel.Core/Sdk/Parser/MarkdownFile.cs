namespace Rag.SemanticKernel.Core.Sdk.Parser;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class MarkdownFile
{
    public string FilePath { get; private set; }
    public string FileName => Path.GetFileName(FilePath);
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FilePath);
    public List<MarkdownHeading> Headings { get; private set; } = new();

    public void Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        FilePath = filePath;
        var lines = File.ReadAllLines(filePath);
        MarkdownHeading currentHeading = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            int lineNumber = i + 1;

            HeadingType? headingType = null;
            string headingText = null;

            if (line.StartsWith("### "))
            {
                headingType = HeadingType.H3;
                headingText = line[4..];
            }
            else if (line.StartsWith("## "))
            {
                headingType = HeadingType.H2;
                headingText = line[3..];
            }
            else if (line.StartsWith("# "))
            {
                headingType = HeadingType.H1;
                headingText = line[2..];
            }
            else if (Regex.IsMatch(line, @"^\*\*\d+\s§\*\*"))  // e.g., "**1 §**"
            {
                headingType = HeadingType.Section;
                headingText = Regex.Match(line, @"^\*\*(\d+\s§)\*\*").Groups[1].Value;
            }
            else if (Regex.IsMatch(line, @"^\*\*.+?\*\*"))
            {
                headingType = HeadingType.BoldParagraph;
                headingText = Regex.Match(line, @"^\*\*(.+?)\*\*").Groups[1].Value;
            }

            if (headingType.HasValue)
            {
                currentHeading = new MarkdownHeading(headingText, headingType.Value, lineNumber);
                Headings.Add(currentHeading);
            }
            else if (currentHeading != null)
            {
                // Accumulate content under the current heading
                currentHeading.Content += lines[i] + Environment.NewLine;
            }
        }
    }
}
