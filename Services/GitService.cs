using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPlayground.Services;
public class GitService(ILogger<GitService> logger)
{
    private readonly ILogger<GitService> _logger = logger;

    private string _repoPath = string.Empty;
    private Repository? _repo;

    public bool SetRepoPath(string repoPath)
    {
        _repoPath = repoPath;
        try
        {
            _repo = new Repository(_repoPath);
            return true;
        }
        catch (RepositoryNotFoundException)
        {
            _logger.LogError("Repository not found at path: {RepoPath}", repoPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting repository path: {RepoPath}", repoPath);
            return false;
        }
    }

    public string GetCommits(int count)
    {
        if (_repo == null)
        {
            _logger.LogError("Repository is not set.");
            return string.Empty;
        }
        var commits = _repo.Commits.Take(count);
        var commitMessages = string.Join(Environment.NewLine, commits.Select(c => c.MessageShort));
        return commitMessages;
    }
}
