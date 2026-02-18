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
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = _repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
            throw new InvalidOperationException("Failed to start git process");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git command failed: {error}");
        }

        return output.Trim();
    }
}
