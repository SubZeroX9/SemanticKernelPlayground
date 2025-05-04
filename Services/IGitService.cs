namespace SemanticKernelPlayground.Services;
public interface IGitService
{
    bool SetRepoPath(string repoPath);
    string GetCommits(int count);
}
