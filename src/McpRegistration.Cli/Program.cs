using System.Text.Json;
using McpRegistration.Core.Models;
using McpRegistration.Core.Services;

namespace McpRegistration.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  MCP Server Registration Assistant");
        Console.WriteLine("  🤖 With GitHub Copilot Integration");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        try
        {
            // Initialize Copilot service
            var copilotService = new CopilotService();
            
            var repoPath = GetRepositoryPath();
            var metadata = await CollectMetadataAsync(copilotService);
            
            var validationService = new ValidationService();
            var errors = validationService.ValidateMetadata(metadata);
            
            if (errors.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Validation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine("Generating files...");
            
            await GenerateFilesAsync(repoPath, metadata);
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Files generated successfully!");
            Console.ResetColor();
            
            Console.WriteLine();
            Console.WriteLine($"Files created at: apis/{metadata.Name}/");
            Console.WriteLine("  - openapi.json");
            Console.WriteLine("  - metadata.json");
            Console.WriteLine("  - README.md");
            
            Console.WriteLine();
            Console.Write("Would you like to commit these changes? (y/n): ");
            var commit = Console.ReadLine()?.Trim().ToLower() == "y";
            
            if (commit)
            {
                await CreateBranchAndCommitAsync(repoPath, metadata);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Changes committed successfully!");
                Console.WriteLine($"  Branch: mcp-registration/{metadata.Name}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Push the branch: git push -u origin mcp-registration/" + metadata.Name);
                Console.WriteLine("  2. Create a Pull Request on GitHub");
                Console.WriteLine("  3. After PR approval and merge, GitHub Actions will register to Azure API Center");
            }
            
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

    static async Task<McpMetadata> CollectMetadataAsync(CopilotService copilotService)
    {
        var metadata = new McpMetadata();

        Console.WriteLine("Please provide the following information:");
        Console.WriteLine("(Press Enter to accept default values shown in brackets)");
        Console.WriteLine();

        // First, get description to help with name suggestion
        metadata.Description = PromptRequired("Description", 
            hint: "Brief description of what this MCP server does");

        metadata.Summary = Prompt("Summary (one-line, optional)", 
            hint: "Short summary for quick reference");

        // Use Copilot to suggest name
        if (copilotService.IsAvailable)
        {
            Console.Write("🤖 Generating name suggestion...");
            var suggestedName = await copilotService.SuggestMcpNameAsync(metadata.Description);
            Console.WriteLine($" {suggestedName}");
            
            metadata.Name = Prompt("MCP Server Name (lowercase, use hyphens)", suggestedName);
            
            // Validate with Copilot
            var (isValid, message, suggestion) = await copilotService.ValidateFieldAsync("name", metadata.Name);
            if (!isValid && message != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  {message}");
                if (suggestion != null)
                {
                    Console.WriteLine($"   Suggestion: {suggestion}");
                }
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Name looks good!");
                Console.ResetColor();
            }
        }
        else
        {
            metadata.Name = PromptRequired("MCP Server Name (lowercase, use hyphens)", 
                hint: "e.g., my-awesome-mcp");
        }

        metadata.Version = Prompt("Version", "1.0.0");

        metadata.Company = PromptRequired("Company/Organization");

        metadata.Owner = PromptRequired("Owner (name of person or team)");

        metadata.ContactEmail = Prompt("Contact Email (optional)");

        metadata.Status = PromptChoice("Status", ValidationService.GetValidStatuses(), "active");

        metadata.Lifecycle = PromptChoice("Lifecycle", ValidationService.GetValidLifecycles(), "development");

        metadata.AuthMethod = PromptChoice("Authentication Method", ValidationService.GetValidAuthMethods(), "api-key");

        metadata.EndpointUrl = Prompt("Endpoint URL (optional)", 
            hint: "https://your-mcp-server.example.com");

        metadata.DocumentationUrl = Prompt("Documentation URL (optional)");

        Console.Write("Tags (comma-separated, optional): ");
        var tagsInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(tagsInput))
        {
            metadata.Tags = tagsInput.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        return metadata;
    }

    static string PromptRequired(string fieldName, string? hint = null)
    {
        while (true)
        {
            Console.Write($"{fieldName}: ");
            if (hint != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"({hint}) ");
                Console.ResetColor();
            }
            
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
                return input;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {fieldName} is required. Please provide a value.");
            Console.ResetColor();
        }
    }

    static string Prompt(string fieldName, string? defaultValue = null, string? hint = null)
    {
        Console.Write($"{fieldName}");
        if (defaultValue != null)
        {
            Console.Write($" [{defaultValue}]");
        }
        Console.Write(": ");
        
        if (hint != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"({hint}) ");
            Console.ResetColor();
        }

        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(input) ? (defaultValue ?? string.Empty) : input;
    }

    static string PromptChoice(string fieldName, string[] options, string defaultValue)
    {
        Console.WriteLine($"{fieldName} (options: {string.Join(", ", options)}) [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim().ToLower();
        
        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;

        return options.Contains(input) ? input : defaultValue;
    }

    static async Task GenerateFilesAsync(string repoPath, McpMetadata metadata)
    {
        var apiDir = Path.Combine(repoPath, "apis", metadata.Name);
        Directory.CreateDirectory(apiDir);

        var fileGenService = new FileGenerationService();

        // Generate metadata.json
        var metadataJson = fileGenService.GenerateMetadataJson(metadata);
        await File.WriteAllTextAsync(Path.Combine(apiDir, "metadata.json"), metadataJson);

        // Generate openapi.json
        var openApiJson = fileGenService.GenerateOpenApiSpec(metadata);
        await File.WriteAllTextAsync(Path.Combine(apiDir, "openapi.json"), openApiJson);

        // Generate README.md
        var readme = fileGenService.GenerateReadme(metadata);
        await File.WriteAllTextAsync(Path.Combine(apiDir, "README.md"), readme);
    }

    static async Task CreateBranchAndCommitAsync(string repoPath, McpMetadata metadata)
    {
        var gitService = new GitService(repoPath);
        
        var branchName = $"mcp-registration/{metadata.Name}";
        
        // Check if branch exists
        if (await gitService.BranchExistsAsync(branchName))
        {
            Console.WriteLine($"Branch {branchName} already exists. Using existing branch.");
        }
        else
        {
            await gitService.CreateBranchAsync(metadata.Name);
        }

        var commitMessage = $@"Register MCP server: {metadata.Name}

- Company: {metadata.Company}
- Owner: {metadata.Owner}
- Lifecycle: {metadata.Lifecycle}
- Status: {metadata.Status}";

        await gitService.CommitChangesAsync(metadata.Name, commitMessage);
    }
}
