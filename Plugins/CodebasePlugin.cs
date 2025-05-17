using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelPlayground.Models;
using SemanticKernelPlayground.Utilities;
using System.ComponentModel;
using System.Text;

namespace SemanticKernelPlayground.Plugins;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class CodebasePlugin(
    IVectorStore vectorStore,
    ITextEmbeddingGenerationService embeddingService)
{
    private const string CollectionName = "codebase";

    [KernelFunction, Description("Search the codebase for relevant information")]
    public async Task<string> SearchCodebase(
        [Description("The search query")] string query,
        [Description("Maximum number of results to return")] int maxResults = 5)
    {
        try
        {
            var collection = vectorStore.GetCollection<string, TextChunk>(CollectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            // Generate embedding for the query
            var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

            // Search for similar vectors in the collection
            var searchResults = collection.SearchEmbeddingAsync(queryEmbedding, maxResults);

            var resultList = new List<VectorSearchResult<TextChunk>>();
            await foreach (var result in searchResults)
            {
                resultList.Add(result);
            }

            if (!resultList.Any())
            {
                return "No relevant code found for your query.";
            }

            var builder = new StringBuilder();
            builder.AppendLine("### Relevant code snippets found:");
            builder.AppendLine();

            foreach (var result in resultList)
            {
                builder.AppendLine($"**File: {result.Record.DocumentName}**");

                // Determine language for syntax highlighting based on file extension
                string language = CodeFormatter.GetLanguageFromFileName(result.Record.DocumentName);

                builder.AppendLine($"```{language}");
                builder.AppendLine(result.Record.Text);
                builder.AppendLine("```");
                builder.AppendLine();
            }

            return builder.ToString();
        }
        catch (Exception ex)
        {
            return $"Error searching codebase: {ex.Message}";
        }
    }

    [KernelFunction, Description("List all files in the codebase or in a specific directory")]
    public async Task<string> ListFiles(
        [Description("Optional directory path to filter results")] string? directoryPath = null)
    {
        try
        {
            var collection = vectorStore.GetCollection<string, TextChunk>(CollectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            // Get all items from collection using SearchAsync with a general query
            var allItems = new List<TextChunk>();

            // Create a general embedding that should match all documents
            var generalEmbedding = await embeddingService.GenerateEmbeddingAsync("code");

            // Use a large limit to get all documents
            var searchResults = collection.SearchEmbeddingAsync(generalEmbedding, 1000);

            await foreach (var result in searchResults)
            {
                allItems.Add(result.Record);
            }

            // Get unique document names
            var uniqueDocuments = allItems
                .Select(item => item.DocumentName)
                .Distinct()
                .OrderBy(docName => docName)
                .ToList();

            // Filter by directory if provided
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                uniqueDocuments = uniqueDocuments
                    .Where(doc => doc.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!uniqueDocuments.Any())
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return "No files found in the codebase.";
                }
                else
                {
                    return $"No files found in directory: {directoryPath}";
                }
            }

            var resultBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                resultBuilder.AppendLine($"### Found {uniqueDocuments.Count} files in the codebase:");
            }
            else
            {
                resultBuilder.AppendLine($"### Found {uniqueDocuments.Count} files in directory '{directoryPath}':");
            }

            resultBuilder.AppendLine();

            // Group files by directory for better organization
            var filesByDirectory = uniqueDocuments
                .GroupBy(doc => Path.GetDirectoryName(doc) ?? string.Empty)
                .OrderBy(group => group.Key);

            foreach (var directoryGroup in filesByDirectory)
            {
                var directory = directoryGroup.Key;
                if (string.IsNullOrWhiteSpace(directory))
                {
                    resultBuilder.AppendLine("**Root directory:**");
                }
                else
                {
                    resultBuilder.AppendLine($"**{directory}:**");
                }

                foreach (var file in directoryGroup.OrderBy(f => f))
                {
                    resultBuilder.AppendLine($"- {Path.GetFileName(file)}");
                }

                resultBuilder.AppendLine();
            }

            return resultBuilder.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing files: {ex.Message}";
        }
    }

    // Remaining methods with CreateCollectionIfNotExistsAsync added
    [KernelFunction, Description("Get information about a specific file in the codebase")]
    public async Task<string> GetFileInfo(
        [Description("The filename or partial path to search for")] string fileName)
    {
        try
        {
            var collection = vectorStore.GetCollection<string, TextChunk>(CollectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            // Create a general embedding
            var generalEmbedding = await embeddingService.GenerateEmbeddingAsync("code");

            // Use a large limit to get all documents (adjust if needed)
            var searchResults = collection.SearchAsync(generalEmbedding, 1000);

            var allItems = new List<TextChunk>();
            await foreach (var result in searchResults)
            {
                allItems.Add(result.Record);
            }

            // Filter items by filename (case-insensitive contains)
            var matchingItems = allItems
                .Where(item => item.DocumentName.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matchingItems.Any())
            {
                return $"No files found matching '{fileName}'";
            }

            // Group by document name to get complete files
            var fileGroups = matchingItems.GroupBy(item => item.DocumentName);

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"### Found {fileGroups.Count()} files matching '{fileName}':");
            resultBuilder.AppendLine();

            foreach (var fileGroup in fileGroups)
            {
                resultBuilder.AppendLine($"**{fileGroup.Key}**");

                // Order chunks by paragraph ID
                var orderedChunks = fileGroup.OrderBy(item => item.ParagraphId);

                // Determine language for syntax highlighting based on file extension
                string language = CodeFormatter.GetLanguageFromFileName(fileGroup.Key);

                resultBuilder.AppendLine($"```{language}");
                foreach (var chunk in orderedChunks)
                {
                    resultBuilder.Append(chunk.Text);
                }
                resultBuilder.AppendLine("```");
                resultBuilder.AppendLine();
            }

            return resultBuilder.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving file information: {ex.Message}";
        }
    }

    [KernelFunction, Description("Analyze code structure and dependencies")]
    public async Task<string> AnalyzeCodeStructure(
        [Description("The filename or partial path to analyze")] string fileName)
    {
        try
        {
            // First get the file content
            var fileContent = await GetFileInfo(fileName);
            if (fileContent.StartsWith("No files found"))
            {
                return fileContent;
            }

            // Now we'll look for dependencies and relationships
            var collection = vectorStore.GetCollection<string, TextChunk>(CollectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            // Generate a search query to find related files
            var queryEmbedding = await embeddingService.GenerateEmbeddingAsync($"Code that interacts with or is related to {fileName}");

            // Search for related code
            var searchResults = collection.SearchEmbeddingAsync(queryEmbedding, 10);

            var resultList = new List<VectorSearchResult<TextChunk>>();
            await foreach (var result in searchResults)
            {
                resultList.Add(result);
            }

            var relatedFiles = resultList
                .Where(r => !r.Record.DocumentName.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Record.DocumentName)
                .Distinct()
                .Take(5)
                .ToList();

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"### Code Structure Analysis for '{fileName}':");
            resultBuilder.AppendLine();

            // Add information about related files
            if (relatedFiles.Any())
            {
                resultBuilder.AppendLine("**Related Files:**");
                foreach (var relatedFile in relatedFiles)
                {
                    resultBuilder.AppendLine($"- {relatedFile}");
                }
                resultBuilder.AppendLine();
            }

            // Add the file content for context
            resultBuilder.Append(fileContent);

            return resultBuilder.ToString();
        }
        catch (Exception ex)
        {
            return $"Error analyzing code structure: {ex.Message}";
        }
    }
}

#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.