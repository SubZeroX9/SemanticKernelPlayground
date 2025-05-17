using Microsoft.SemanticKernel.Data;

namespace SemanticKernelPlayground.Models;

internal sealed class TextChunkTextSearchStringMapper : ITextSearchStringMapper
{
    /// <inheritdoc />
    public string MapFromResultToString(object result)
    {
        if (result is TextChunk dataModel)
        {
            return dataModel.Text;
        }
        throw new ArgumentException("Invalid result type.");
    }
}

internal sealed class TextChunkTextSearchResultMapper : ITextSearchResultMapper
{
    /// <inheritdoc />
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is TextChunk dataModel)
        {
            return new(value: dataModel.Text) { Name = dataModel.Key, Link = dataModel.DocumentName };
        }
        throw new ArgumentException("Invalid result type.");
    }
}