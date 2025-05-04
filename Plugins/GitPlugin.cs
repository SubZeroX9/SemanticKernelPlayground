using Microsoft.SemanticKernel;
using System.ComponentModel;
using SemanticKernelPlayground.Services;

namespace SemanticKernelPlayground.Plugins;
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GitPlugin(IGitService gitService)
{
    [KernelFunction, Description("Sets the repository path")]
    public void SetRepoPath(string repoPath)
    {
        gitService.SetRepoPath(repoPath);
    }
}
