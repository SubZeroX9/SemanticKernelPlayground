namespace SemanticKernelPlayground.Utilities;

public static class CodeFormatter
{
    // Helper method to determine the language for syntax highlighting based on file extension
    public static string GetLanguageFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            // .NET languages
            ".cs" => "csharp",
            ".vb" => "vb",
            ".fs" => "fsharp",

            // Web languages
            ".html" => "html",
            ".htm" => "html",
            ".css" => "css",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".jsx" => "jsx",
            ".tsx" => "tsx",

            // Markup/data
            ".json" => "json",
            ".xml" => "xml",
            ".md" => "markdown",
            ".yaml" => "yaml",
            ".yml" => "yaml",

            // Project files
            ".csproj" => "xml",
            ".vbproj" => "xml",
            ".fsproj" => "xml",
            ".sln" => "text",
            ".config" => "xml",
            ".props" => "xml",
            ".targets" => "xml",

            // Scripting languages
            ".py" => "python",
            ".rb" => "ruby",
            ".php" => "php",
            ".sh" => "bash",
            ".ps1" => "powershell",
            ".bat" => "batch",
            ".cmd" => "batch",

            // Other languages
            ".java" => "java",
            ".c" => "c",
            ".cpp" => "cpp",
            ".h" => "cpp",
            ".go" => "go",
            ".sql" => "sql",
            ".r" => "r",

            // Web templates
            ".cshtml" => "cshtml",
            ".razor" => "razor",
            ".aspx" => "aspx",

            // Plain text
            ".txt" => "text",

            // Default for unknown types
            _ => "text"
        };
    }
}