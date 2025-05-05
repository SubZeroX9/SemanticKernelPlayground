namespace SemanticKernelPlayground.Services;
public interface IGitService
{
    string RepoPath { get; }
    bool SetRepoPath(string repoPath);
    string GetCommits(int count);
}
