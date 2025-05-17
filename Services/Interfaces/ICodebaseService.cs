namespace SemanticKernelPlayground.Services.Interfaces;

public interface ICodebaseService
{
    Task IndexCodebase(string basePath);
}