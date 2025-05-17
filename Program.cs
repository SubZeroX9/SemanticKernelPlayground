using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using SemanticKernelPlayground.Plugins;
using SemanticKernelPlayground.Services.Interfaces;
using SemanticKernelPlayground.Services;

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

var modelName = configuration["ModelName"] ?? throw new ApplicationException("ModelName not found");
var embeddingModel = configuration["EmbeddingModel"] ?? throw new ApplicationException("EmbeddingModel not found");
var endpoint = configuration["Endpoint"] ?? throw new ApplicationException("Endpoint not found");
var apiKey = configuration["ApiKey"] ?? throw new ApplicationException("ApiKey not found");

// Setup services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Register our services
services.AddSingleton<ICodebaseReaderService, CodebaseReaderService>();
services.AddSingleton<ICodebaseService, CodebaseService>();

// Configure Semantic Kernel
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(modelName, endpoint, apiKey)
    .AddAzureOpenAITextEmbeddingGeneration(embeddingModel, endpoint, apiKey)
    .AddInMemoryVectorStore();

// Add our service registrations to the kernel builder
foreach (var serviceDescriptor in services)
{
    builder.Services.Add(serviceDescriptor);
}

// Register plugins
builder.Plugins.AddFromType<CodebasePlugin>();

// Build the kernel
var kernel = builder.Build();

// Input for codebase path
Console.WriteLine("Enter the path to the codebase you want to index (press Enter to use current directory):");
var codebasePath = Console.ReadLine() ?? string.Empty;

if (string.IsNullOrWhiteSpace(codebasePath))
{
    codebasePath = Directory.GetCurrentDirectory();
}

// Setup codebase indexing
Console.WriteLine($"Indexing your codebase at: {codebasePath}");
var codebaseService = kernel.GetRequiredService<ICodebaseService>();
await codebaseService.IndexCodebase(codebasePath);
Console.WriteLine("Codebase indexing complete!");

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

AzureOpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var history = new ChatHistory();
history.AddSystemMessage(
    "Your job is to assist me with understanding and navigating codebases. " +
    "You're an expert programmer and can explain code concepts clearly. " +
    "When I ask about code, use your search abilities to find and explain relevant parts of the codebase. " +
    "Include code snippets in your explanations and provide context about how different components interact. " +
    "If you're uncertain about anything, ask for clarification. " +
    "Focus on being clear, accurate, and educational in your responses.");

do
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Me > ");
    Console.ResetColor();

    var userInput = Console.ReadLine();
    if (userInput == "exit")
    {
        break;
    }

    history.AddUserMessage(userInput!);

    var streamingResponse =
        chatCompletionService.GetStreamingChatMessageContentsAsync(
            history,
            openAiPromptExecutionSettings,
            kernel);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Agent > ");
    Console.ResetColor();

    var fullResponse = "";
    await foreach (var chunk in streamingResponse)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(chunk.Content);
        Console.ResetColor();
        fullResponse += chunk.Content;
    }
    Console.WriteLine();

    history.AddMessage(AuthorRole.Assistant, fullResponse);


} while (true);

#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.