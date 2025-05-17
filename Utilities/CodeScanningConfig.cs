namespace SemanticKernelPlayground.Utilities;

/// <summary>
/// Provides configuration settings for codebase scanning and processing.
/// </summary>
public static class CodeScanningConfig
{
    /// <summary>
    /// Gets all file extensions that should be considered for codebase scanning.
    /// These should align with what CodeFormatter can syntax-highlight.
    /// </summary>
    public static string[] GetSupportedFileExtensions()
    {
        return
        [
            ".cs",    // C#
            ".json",  // JSON
            ".xml",   // XML
            ".csproj", // Project files
            ".sln",   // Solution files
            ".md",    // Markdown
            ".html",  // HTML
            ".css",   // CSS
            ".js",    // JavaScript
            ".ts",    // TypeScript
            ".py",    // Python
            ".java",  // Java
            ".sh",    // Bash
            ".bat",   // Batch
            ".ps1",   // PowerShell
            ".sql",   // SQL
            ".yml",   // YAML
            ".yaml",  // YAML
            ".txt",   // Text files
            ".config", // Config files
            ".xaml",  // XAML
            ".cshtml", // Razor views
            ".razor",  // Blazor components
            ".jsx",   // React JSX
            ".tsx",   // React TSX
            ".c",     // C
            ".cpp",   // C++
            ".h",     // Header files
            ".go",    // Go
            ".rb",    // Ruby
            ".php"    // PHP
        ];
    }

    /// <summary>
    /// Gets directories that should be excluded from codebase scanning.
    /// </summary>
    public static string[] GetExcludedDirectories()
    {
        return
        [
            "bin",       // Binary output
            "obj",       // Object files
            ".git",      // Git repository
            ".vs",       // Visual Studio config
            "node_modules", // NPM packages
            "packages",  // NuGet packages
            "dist",      // Distribution output
            "build",     // Build output
            ".vscode",   // VS Code config
            ".idea",     // JetBrains IDE config
            "vendor",    // Vendor dependencies
            "wwwroot/lib", // Web libraries
            "target",    // Maven/Gradle build output
            "out",       // Output directories
            "Debug",     // Debug builds
            "Release",   // Release builds
            ".svn",      // SVN repository
            ".hg",       // Mercurial repository
            ".nuget",    // NuGet cache
            "artifacts"  // Build artifacts
        ];
    }
}