using DevLife.Enums;
using DevLife.Services.Interfaces;
using System.Text.RegularExpressions;

namespace DevLife.Services.Implementation;

public class CodeFormatterService : ICodeFormatterService
{
    private readonly ILogger<CodeFormatterService> _logger;

    public CodeFormatterService(ILogger<CodeFormatterService> logger)
    {
        _logger = logger;
    }

    public string FormatCode(string code, TechnologyStack techStack)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        try
        {
            // Clean the code first
            var cleanedCode = CleanCode(code);

            // Apply language-specific formatting
            return techStack switch
            {
                TechnologyStack.DotNet => FormatCSharpCode(cleanedCode),
                TechnologyStack.JavaScript => FormatJavaScriptCode(cleanedCode),
                TechnologyStack.TypeScript => FormatTypeScriptCode(cleanedCode),
                TechnologyStack.Python => FormatPythonCode(cleanedCode),
                TechnologyStack.Java => FormatJavaCode(cleanedCode),
                TechnologyStack.React => FormatReactCode(cleanedCode),
                _ => FormatGenericCode(cleanedCode)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting code for tech stack: {TechStack}", techStack);
            return CleanCode(code); // Return cleaned version if formatting fails
        }
    }

    private string CleanCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        // Remove excessive whitespace while preserving structure
        var lines = code.Split('\n', StringSplitOptions.None);
        var cleanedLines = new List<string>();

        foreach (var line in lines)
        {
            // Remove trailing whitespace but keep leading indentation
            var cleanedLine = line.TrimEnd();
            cleanedLines.Add(cleanedLine);
        }

        // Remove excessive empty lines (max 2 consecutive)
        var result = new List<string>();
        int emptyLineCount = 0;

        foreach (var line in cleanedLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                emptyLineCount++;
                if (emptyLineCount <= 2)
                {
                    result.Add(line);
                }
            }
            else
            {
                emptyLineCount = 0;
                result.Add(line);
            }
        }

        return string.Join('\n', result).Trim();
    }

    private string FormatCSharpCode(string code)
    {
        var lines = code.Split('\n');
        var formattedLines = new List<string>();
        int indentLevel = 0;
        const string indent = "    "; // 4 spaces

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine))
            {
                formattedLines.Add("");
                continue;
            }

            // Adjust indent level before adding the line
            if (trimmedLine.Contains('}'))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // Add the properly indented line
            var indentString = string.Concat(Enumerable.Repeat(indent, indentLevel));
            formattedLines.Add(indentString + trimmedLine);

            // Adjust indent level after adding the line
            if (trimmedLine.Contains('{') && !trimmedLine.Contains('}'))
            {
                indentLevel++;
            }
        }

        return string.Join('\n', formattedLines);
    }

    private string FormatJavaScriptCode(string code)
    {
        var lines = code.Split('\n');
        var formattedLines = new List<string>();
        int indentLevel = 0;
        const string indent = "  "; // 2 spaces for JS

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine))
            {
                formattedLines.Add("");
                continue;
            }

            // Adjust indent level before adding the line
            if (trimmedLine.Contains('}') || trimmedLine.Contains(']') || trimmedLine.Contains(')'))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // Add the properly indented line
            var indentString = string.Concat(Enumerable.Repeat(indent, indentLevel));
            formattedLines.Add(indentString + trimmedLine);

            // Adjust indent level after adding the line
            if ((trimmedLine.Contains('{') || trimmedLine.Contains('[') || trimmedLine.Contains('('))
                && !trimmedLine.Contains('}') && !trimmedLine.Contains(']') && !trimmedLine.Contains(')'))
            {
                indentLevel++;
            }
        }

        return string.Join('\n', formattedLines);
    }

    private string FormatTypeScriptCode(string code)
    {
        // TypeScript formatting is similar to JavaScript
        return FormatJavaScriptCode(code);
    }

    private string FormatPythonCode(string code)
    {
        var lines = code.Split('\n');
        var formattedLines = new List<string>();
        int indentLevel = 0;
        const string indent = "    "; // 4 spaces for Python

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine))
            {
                formattedLines.Add("");
                continue;
            }

            // Python uses colons to indicate blocks
            if (indentLevel > 0 && !trimmedLine.StartsWith(' ') &&
                !trimmedLine.StartsWith('\t') &&
                !IsIndentedLine(trimmedLine))
            {
                indentLevel = 0; // Reset indent for non-indented lines
            }

            // Add the properly indented line
            var indentString = string.Concat(Enumerable.Repeat(indent, indentLevel));
            formattedLines.Add(indentString + trimmedLine);

            // Increase indent after lines ending with colon
            if (trimmedLine.EndsWith(':'))
            {
                indentLevel++;
            }
        }

        return string.Join('\n', formattedLines);
    }

    private string FormatJavaCode(string code)
    {
        // Java formatting is similar to C#
        return FormatCSharpCode(code);
    }

    private string FormatReactCode(string code)
    {
        var lines = code.Split('\n');
        var formattedLines = new List<string>();
        int indentLevel = 0;
        const string indent = "  "; // 2 spaces for React/JSX

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine))
            {
                formattedLines.Add("");
                continue;
            }

            // Adjust indent level before adding the line
            if (trimmedLine.Contains('}') || trimmedLine.Contains("</") || trimmedLine.Contains("/>"))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // Add the properly indented line
            var indentString = string.Concat(Enumerable.Repeat(indent, indentLevel));
            formattedLines.Add(indentString + trimmedLine);

            // Adjust indent level after adding the line
            if (trimmedLine.Contains('{') || (trimmedLine.Contains('<') && !trimmedLine.Contains("/>")))
            {
                indentLevel++;
            }
        }

        return string.Join('\n', formattedLines);
    }

    private string FormatGenericCode(string code)
    {
        // Basic formatting for unknown languages
        var lines = code.Split('\n');
        var formattedLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!string.IsNullOrEmpty(trimmedLine))
            {
                formattedLines.Add(trimmedLine);
            }
            else
            {
                formattedLines.Add("");
            }
        }

        return string.Join('\n', formattedLines);
    }

    private bool IsIndentedLine(string line)
    {
        // Helper method to detect if a line should be indented
        var keywords = new[] { "def", "class", "if", "elif", "else", "for", "while", "try", "except", "finally", "with" };
        return !keywords.Any(keyword => line.TrimStart().StartsWith(keyword + " "));
    }

    public string BeautifyCode(string code, TechnologyStack techStack)
    {
        // Advanced beautification logic
        var formatted = FormatCode(code, techStack);

        // Add proper spacing around operators
        formatted = AddOperatorSpacing(formatted);

        // Ensure consistent bracket placement
        formatted = NormalizeBrackets(formatted, techStack);

        return formatted;
    }

    private string AddOperatorSpacing(string code)
    {
        // Add spaces around common operators
        var operators = new[] { "=", "+", "-", "*", "/", "==", "!=", "<=", ">=", "<", ">", "&&", "||" };

        foreach (var op in operators)
        {
            // Avoid double spacing
            code = Regex.Replace(code, $@"\s*{Regex.Escape(op)}\s*", $" {op} ");
        }

        return code;
    }

    private string NormalizeBrackets(string code, TechnologyStack techStack)
    {
        // Language-specific bracket styling
        switch (techStack)
        {
            case TechnologyStack.DotNet:
            case TechnologyStack.Java:
                // Opening brace on new line
                code = Regex.Replace(code, @"\s*{\s*", "\n{");
                break;

            case TechnologyStack.JavaScript:
            case TechnologyStack.TypeScript:
            case TechnologyStack.React:
                // Opening brace on same line
                code = Regex.Replace(code, @"\s*{\s*", " {");
                break;
        }

        return code;
    }
}