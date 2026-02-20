using System.Diagnostics;
using System.Text;

namespace McpRegistration.Core.Services;

/// <summary>
/// Service for Git operations
/// </summary>
public class GitService
{
    private readonly string _repoPath;

    public GitService(string repoPath)
    {
        _repoPath = repoPath;
    }

    /// <summary>
    /// Create a new branch for the MCP registration
    /// </summary>
    public async Task<string> CreateBranchAsync(string mcpName)
    {
        var branchName = $"mcp-registration/{mcpName}";
        await ExecuteGitCommandAsync($"checkout -b {branchName}");
        return branchName;
    }

    /// <summary>
    /// Stage and commit files
    /// </summary>
    public async Task CommitChangesAsync(string mcpName, string message)
    {
        await ExecuteGitCommandAsync("add .");
        await ExecuteGitCommandAsync($"commit -m \"{message}\"");
    }

    /// <summary>
    /// Push branch to remote
    /// </summary>
    public async Task PushBranchAsync(string branchName)
    {
        await ExecuteGitCommandAsync($"push -u origin {branchName}");
    }

    /// <summary>
    /// Get current branch name
    /// </summary>
    public async Task<string> GetCurrentBranchAsync()
    {
        return await ExecuteGitCommandAsync("rev-parse --abbrev-ref HEAD");
    }

    /// <summary>
    /// Create a pull request using the GitHub CLI (gh)
    /// </summary>
    public async Task<string> CreatePullRequestAsync(string title, string body, string baseBranch = "main")
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "gh",
            WorkingDirectory = _repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        processStartInfo.ArgumentList.Add("pr");
        processStartInfo.ArgumentList.Add("create");
        processStartInfo.ArgumentList.Add("--title");
        processStartInfo.ArgumentList.Add(title);
        processStartInfo.ArgumentList.Add("--body");
        processStartInfo.ArgumentList.Add(body);
        processStartInfo.ArgumentList.Add("--base");
        processStartInfo.ArgumentList.Add(baseBranch);

        using var process = Process.Start(processStartInfo);
        if (process == null)
            throw new InvalidOperationException("Failed to start gh process");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to create Pull Request. Ensure the GitHub CLI (gh) is installed and authenticated.\n{error}");
        }

        return output.Trim();
    }

    /// <summary>
    /// Check if branch exists locally
    /// </summary>
    public async Task<bool> BranchExistsAsync(string branchName)
    {
        try
        {
            var result = await ExecuteGitCommandAsync($"rev-parse --verify {branchName}");
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> ExecuteGitCommandAsync(string arguments)
    {
        return await ExecuteCommandAsync("git", arguments);
    }

    private async Task<string> ExecuteCommandAsync(string fileName, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = _repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
            throw new InvalidOperationException($"Failed to start {fileName} process");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} command failed: {error}");
        }

        return output.Trim();
    }
}
