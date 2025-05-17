using SemanticKernelPlayground.Models;

namespace SemanticKernelPlayground.Services.Interfaces;

public interface ICodebaseReaderService
{
    IEnumerable<TextChunk> ScanCodebase(string basePath, int maxChunkSize = 1000);
}