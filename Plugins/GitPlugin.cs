using Microsoft.SemanticKernel;
using System.ComponentModel;
using SemanticKernelPlayground.Services;

namespace SemanticKernelPlayground.Plugins;
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GitPlugin(IGitService gitService)
{
    [KernelFunction, Description("Sets the repository path")]
    public string SetRepoPath(string repoPath)
    {
        var result = gitService.SetRepoPath(repoPath);
        if (!result)
        {
            return "Failed to Set Repo Path";
        }

        return "Path was set to " + gitService.RepoPath;
    }

    [KernelFunction, Description("Gets the last N commits")]
    public string GetCommits(int count)
    {
        var result = gitService.GetCommits(count);
        return result;
    }
}
