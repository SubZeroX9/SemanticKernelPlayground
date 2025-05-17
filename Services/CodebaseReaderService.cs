using Microsoft.Extensions.Logging;
using SemanticKernelPlayground.Models;
using SemanticKernelPlayground.Services.Interfaces;
using SemanticKernelPlayground.Utilities;
using System.Text;

namespace SemanticKernelPlayground.Services;

public class CodebaseReaderService : ICodebaseReaderService
{
    private readonly ILogger<CodebaseReaderService> _logger;

    public CodebaseReaderService(ILogger<CodebaseReaderService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<TextChunk> ScanCodebase(string basePath, int maxChunkSize = 1000)
    {
        var chunks = new List<TextChunk>();
        var codeFiles = GetAllCodeFiles(basePath);

        _logger.LogInformation("Found {Count} code files to process", codeFiles.Count);

        foreach (var filePath in codeFiles)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                var relativeFilePath = Path.GetRelativePath(basePath, filePath);

                // Process file into manageable chunks
                chunks.AddRange(ChunkFileContent(relativeFilePath, fileContent, maxChunkSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Created {Count} text chunks from codebase", chunks.Count);
        return chunks;
    }

    private List<string> GetAllCodeFiles(string basePath)
    {
        var supportedExtensions = CodeScanningConfig.GetSupportedFileExtensions();
        var excludedDirs = CodeScanningConfig.GetExcludedDirectories();

        _logger.LogInformation("Scanning for files with extensions: {Extensions}", string.Join(", ", supportedExtensions));

        // Normalize path separators in basePath
        basePath = Path.GetFullPath(basePath);

        return Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
            // Filter by supported extensions
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            // Filter out excluded directories - more comprehensive check
            .Where(f =>
            {
                // Convert to a normalized path relative to basePath
                var relativePath = Path.GetRelativePath(basePath, f);

                // Check if any excluded directory is in the path segments
                var pathParts = relativePath.Split(Path.DirectorySeparatorChar);

                // Return true if NONE of the path parts match excluded directories
                return !pathParts.Any(part => excludedDirs.Contains(part));
            })
            .ToList();
    }

    private IEnumerable<TextChunk> ChunkFileContent(string fileName, string content, int maxChunkSize)
    {
        var chunks = new List<TextChunk>();

        // For small files, just create a single chunk
        if (content.Length <= maxChunkSize)
        {
            chunks.Add(new TextChunk
            {
                Key = fileName,
                DocumentName = fileName,
                ParagraphId = 1,
                Text = content,
                TextEmbedding = ReadOnlyMemory<float>.Empty
            });

            return chunks;
        }

        // For larger files, split by logical sections
        var lines = content.Split('\n');
        var currentChunk = new StringBuilder();
        var chunkNumber = 1;

        foreach (var line in lines)
        {
            // If adding this line would exceed the chunk size, create a new chunk
            if (currentChunk.Length + line.Length > maxChunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(new TextChunk
                    {
                        Key = $"{fileName}_{chunkNumber}",
                        DocumentName = fileName,
                        ParagraphId = chunkNumber,
                        Text = currentChunk.ToString(),
                        TextEmbedding = ReadOnlyMemory<float>.Empty
                    });

                    chunkNumber++;
                    currentChunk.Clear();
                }
            }

            currentChunk.AppendLine(line);
        }

        // Add the last chunk if it has content
        if (currentChunk.Length > 0)
        {
            chunks.Add(new TextChunk
            {
                Key = $"{fileName}_{chunkNumber}",
                DocumentName = fileName,
                ParagraphId = chunkNumber,
                Text = currentChunk.ToString(),
                TextEmbedding = ReadOnlyMemory<float>.Empty
            });
        }

        return chunks;
    }
}