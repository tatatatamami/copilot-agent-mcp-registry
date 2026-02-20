using McpRegistration.Core.Models;
using McpRegistration.Core.Services;

namespace McpRegistration.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  MCP Server Registration Assistant");
        Console.WriteLine("  🤖 Powered by GitHub Copilot");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        try
        {
            var copilotService = new CopilotService();
            var repoPath = GetRepositoryPath();

            // Ask only for the repository URL
            Console.Write("Repository URL (where the MCP server is hosted): ");
            var repositoryUrl = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(repositoryUrl))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Repository URL is required.");
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine("🤖 Analyzing repository and generating registration metadata...");
            Console.WriteLine("   (Copilot will ask you to confirm the generated values)");
            Console.WriteLine();

            // Copilot analyzes the URL, generates metadata, and confirms with the user
            var metadata = await copilotService.GenerateMetadataFromRepositoryUrlAsync(repositoryUrl);

            if (metadata == null)
            {
                Console.WriteLine();
                Console.WriteLine("Registration cancelled.");
                return 0;
            }

            // Validate generated metadata
            var validationService = new ValidationService();
            var errors = validationService.ValidateMetadata(metadata);

            if (errors.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Validation errors in generated metadata:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine("Generating registration files...");

            await GenerateFilesAsync(repoPath, metadata);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Files generated successfully!");
            Console.ResetColor();
            Console.WriteLine($"  Files created at: apis/{metadata.Name}/");
            Console.WriteLine("    - openapi.json");
            Console.WriteLine("    - metadata.json");
            Console.WriteLine("    - README.md");

            // Create branch, commit, push, and open a pull request
            await CreateBranchCommitAndPushAsync(repoPath, metadata);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Registration complete!");
            Console.ResetColor();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    static string GetRepositoryPath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")))
                return currentDir;
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new InvalidOperationException("Not in a git repository. Please run this from within the repository.");
    }

    static async Task GenerateFilesAsync(string repoPath, McpMetadata metadata)
    {
        var apiDir = Path.Combine(repoPath, "apis", metadata.Name);
        Directory.CreateDirectory(apiDir);

        var fileGenService = new FileGenerationService();

        await File.WriteAllTextAsync(Path.Combine(apiDir, "metadata.json"), fileGenService.GenerateMetadataJson(metadata));
        await File.WriteAllTextAsync(Path.Combine(apiDir, "openapi.json"), fileGenService.GenerateOpenApiSpec(metadata));
        await File.WriteAllTextAsync(Path.Combine(apiDir, "README.md"), fileGenService.GenerateReadme(metadata));
    }

    static async Task CreateBranchCommitAndPushAsync(string repoPath, McpMetadata metadata)
    {
        var gitService = new GitService(repoPath);
        var branchName = $"mcp-registration/{metadata.Name}";

        if (await gitService.BranchExistsAsync(branchName))
        {
            Console.WriteLine($"Branch {branchName} already exists. Using existing branch.");
        }
        else
        {
            await gitService.CreateBranchAsync(metadata.Name);
        }

        var commitMessage = $@"Register MCP server: {metadata.Name}

- Repository: {metadata.RepositoryUrl}
- Company: {metadata.Company}
- Owner: {metadata.Owner}
- Lifecycle: {metadata.Lifecycle}
- Status: {metadata.Status}";

        await gitService.CommitChangesAsync(metadata.Name, commitMessage);

        Console.WriteLine();
        Console.Write("Push branch and create Pull Request? (y/n): ");
        if (Console.ReadLine()?.Trim().ToLower() != "y")
        {
            Console.WriteLine();
            Console.WriteLine($"Branch committed locally. To push manually:");
            Console.WriteLine($"  git push -u origin {branchName}");
            return;
        }

        Console.WriteLine($"Pushing branch {branchName}...");
        await gitService.PushBranchAsync(branchName);

        var prTitle = $"Register MCP server: {metadata.Name}";
        var prBody = $@"## MCP Server Registration

This PR registers the following MCP server in the registry.

| Field | Value |
|-------|-------|
| **Name** | {metadata.Name} |
| **Repository** | {metadata.RepositoryUrl} |
| **Description** | {metadata.Description} |
| **Company** | {metadata.Company} |
| **Owner** | {metadata.Owner} |
| **Version** | {metadata.Version} |
| **Status** | {metadata.Status} |
| **Lifecycle** | {metadata.Lifecycle} |
| **Auth Method** | {metadata.AuthMethod} |

> Generated by MCP Registration Assistant with GitHub Copilot";

        Console.WriteLine("Creating Pull Request...");
        try
        {
            var prUrl = await gitService.CreatePullRequestAsync(prTitle, prBody);
            Console.WriteLine($"  PR: {prUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  Could not create Pull Request automatically: {ex.Message}");
            Console.WriteLine($"   Please create a PR manually for branch: {branchName}");
            Console.ResetColor();
        }
    }
}

