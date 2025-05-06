using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SemanticKernelPlayground.Services;
public class GitService(ILogger<GitService> logger) : IGitService
{
    private readonly ILogger<GitService> _logger = logger;

    private Repository? _repo;

    public string RepoPath { get; private set; } = string.Empty;

    public bool SetRepoPath(string repoPath)
    {
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            _logger.LogError("Repository path is empty or null.");
            return false;
        }

        string fullPath;
        if (repoPath == "/" || repoPath == "\\")
        {
            // When slash is provided, assume current directory
            fullPath = Directory.GetCurrentDirectory();
            _logger.LogInformation("Slash provided, using current directory: {Path}", fullPath);
        }
        else
        {
            // Convert relative path to absolute path
            fullPath = Path.GetFullPath(repoPath);
        }

        if (!Directory.Exists(fullPath))
        {
            _logger.LogError("Repository path does not exist: {RepoPath}", repoPath);
            return false;
        }

        RepoPath = fullPath;

        _logger.LogInformation("Set new Repo Path: {repoPath}", RepoPath);

        try
        {
            _repo = new Repository(RepoPath);
            return true;
        }
        catch (RepositoryNotFoundException)
        {
            _logger.LogError("Repository not found at path: {RepoPath}", repoPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting repository path: {RepoPath}", repoPath);
        }

        RepoPath = string.Empty;
        _repo = null;
        return false;
    }

    public string GetCommits(int count)
    {
        if (_repo == null)
        {
            _logger.LogError("Repository is not set.");
            return "Repository is not set.";
        }

        var commits = _repo.Commits.Take(count);
        var sb = new StringBuilder();

        foreach (var commit in commits)
        {
            sb.AppendLine($"Commit: {commit.Sha}");
            sb.AppendLine($"Author: {commit.Author.Name} <{commit.Author.Email}>");
            sb.AppendLine($"Date: {commit.Author.When.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Message: {commit.MessageShort}");

            // Get the full commit message (including any details beyond the short message)
            if (commit.Message != commit.MessageShort)
            {
                sb.AppendLine("Details:");
                sb.AppendLine(commit.Message);
            }

            // Add statistics about the commit (files changed, insertions, deletions)
            if (commit.Parents.Any())
            {
                var parent = commit.Parents.First();
                var patch = _repo.Diff.Compare<Patch>(parent.Tree, commit.Tree);

                int filesChanged = patch.Count();
                int additions = 0;
                int deletions = 0;

                foreach (var change in patch)
                {
                    additions += change.LinesAdded;
                    deletions += change.LinesDeleted;
                }

                sb.AppendLine($"Changes: {filesChanged} files changed, {additions} insertions(+), {deletions} deletions(-)");

                // List modified files
                sb.AppendLine("Modified files:");
                foreach (var change in patch.Take(5)) // Limit to first 5 files to avoid too much output
                {
                    sb.AppendLine($"  {change.Status}: {change.Path}");
                }

                if (patch.Count() > 5)
                {
                    sb.AppendLine($"  ... and {patch.Count() - 5} more files");
                }
            }

            // Add separator between commits
            sb.AppendLine();
            sb.AppendLine(new string('-', 80));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string GetLatestTaggedVersion()
    {
        if (_repo == null)
        {
            _logger.LogError("Repository is not set.");
            return "Repository is not set.";
        }
        // Get the list of tags in the repository
        var tags = _repo.Tags;

        // Find the latest tag based on commit reference
        var latestTag = tags.OrderByDescending(tag => tag.PeeledTarget as Commit)
            .FirstOrDefault();

        if (latestTag != null)
        {
            return latestTag.FriendlyName; // This will return the tag name
        }
        else
        {
            return "No tags found";
        }
    }
}
