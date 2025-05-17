using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelPlayground.Models;
using SemanticKernelPlayground.Services.Interfaces;

namespace SemanticKernelPlayground.Services;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class CodebaseService(
    ICodebaseReaderService codebaseReader,
    IVectorStore vectorStore,
    ITextEmbeddingGenerationService embeddingService,
    ILogger<CodebaseService> logger)
    : ICodebaseService
{
    private const string CollectionName = "codebase";

    public async Task IndexCodebase(string basePath)
    {
        logger.LogInformation("Starting codebase indexing from: {BasePath}", basePath);

        try
        {
            // Get codebase chunks
            var codeChunks = codebaseReader.ScanCodebase(basePath);

            // Create or get vector store collection
            var collection = vectorStore.GetCollection<string, TextChunk>(CollectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            // Process and index each chunk
            var dataUploader = new DataUploader(vectorStore, embeddingService);
            await dataUploader.UploadToVectorStore(CollectionName, codeChunks);

            logger.LogInformation("Successfully indexed codebase with {Count} chunks", codeChunks.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error indexing codebase");
            throw;
        }
    }
}

#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.