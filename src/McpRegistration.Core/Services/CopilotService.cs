using System.Text.Json;
using System.Text.Json.Serialization;
using GitHub.Copilot.SDK;
using McpRegistration.Core.Models;

namespace McpRegistration.Core.Services;

/// <summary>
/// GitHub Copilot SDK integration for AI-powered MCP metadata generation
/// </summary>
public class CopilotService
{
    private readonly bool _isAvailable;

    public CopilotService()
    {
        try
        {
            // Verify the Copilot SDK is accessible by creating a temporary client
            using var client = new CopilotClient();
            _isAvailable = true;
            Console.WriteLine("🤖 GitHub Copilot SDK initialized successfully");
        }
        catch (Exception ex)
        {
            _isAvailable = false;
            Console.WriteLine($"⚠️  GitHub Copilot not available: {ex.Message}");
            Console.WriteLine("   Continuing with basic functionality...");
        }
    }

    public bool IsAvailable => _isAvailable;

    /// <summary>
    /// Generate MCP registration metadata from a repository URL using Copilot.
    /// Copilot will analyze the repository, generate all required fields, and ask
    /// the user to confirm via the ask_user tool.
    /// Returns null if the user cancels during confirmation.
    /// </summary>
    public async Task<McpMetadata?> GenerateMetadataFromRepositoryUrlAsync(string repositoryUrl)
    {
        if (!_isAvailable)
        {
            return GenerateFallbackMetadata(repositoryUrl);
        }

        await using var client = new CopilotClient();
        await client.StartAsync();

        await using var session = await client.CreateSessionAsync(new SessionConfig
        {
            InfiniteSessions = new InfiniteSessionConfig { Enabled = false },
            OnUserInputRequest = async (request, _) =>
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"🤖 Copilot: {request.Question}");
                Console.ResetColor();

                if (request.Choices?.Count > 0)
                {
                    Console.WriteLine($"   Options: {string.Join(", ", request.Choices)}");
                }

                Console.Write("Your answer: ");
                var answer = Console.ReadLine()?.Trim() ?? string.Empty;
                return new UserInputResponse { Answer = answer, WasFreeform = true };
            }
        });

        string? responseText = null;
        var done = new TaskCompletionSource();

        session.On(evt =>
        {
            if (evt is AssistantMessageEvent msg)
            {
                responseText = msg.Data.Content;
            }
            else if (evt is SessionIdleEvent)
            {
                done.TrySetResult();
            }
            else if (evt is SessionErrorEvent err)
            {
                done.TrySetException(new InvalidOperationException(
                    err.Data?.Message ?? "Copilot session error"));
            }
        });

        await session.SendAsync(new MessageOptions { Prompt = BuildMetadataGenerationPrompt(repositoryUrl) });
        await done.Task;

        if (responseText == null)
            return GenerateFallbackMetadata(repositoryUrl);

        return ParseCopilotResponse(responseText, repositoryUrl);
    }

    private static string BuildMetadataGenerationPrompt(string repositoryUrl)
    {
        return $$"""
            You are helping register an MCP (Model Context Protocol) server in a registry.
            The user has provided the following repository URL: {{repositoryUrl}}

            Please:
            1. Analyze the repository URL and any publicly available information about the repository.
            2. Generate the following registration metadata:
               - name: lowercase with hyphens, derived from the repository name (e.g., "my-mcp-server")
               - description: what this MCP server does (2-3 sentences)
               - summary: one-line summary under 100 characters
               - version: "1.0.0"
               - company: the organization or user that owns the repository
               - owner: the repository owner (GitHub username or org name)
               - status: one of "active", "deprecated", "planned"
               - lifecycle: one of "design", "development", "testing", "preview", "production", "deprecated", "retired"
               - authMethod: one of "none", "api-key", "oauth2", "entra-id"
               - endpointUrl: the server endpoint URL if found in the repository (empty string if not found)
               - documentationUrl: documentation URL if found (empty string if not found)
               - tags: list of relevant string tags
            3. Use the ask_user tool to display all generated values to the user clearly, then ask:
               "Would you like to proceed with registration using these values? (yes/no)"
            4. If the user answers affirmatively, respond with ONLY this JSON and nothing else:
               {"confirmed":true,"name":"...","description":"...","summary":"...","version":"1.0.0","company":"...","owner":"...","status":"active","lifecycle":"development","authMethod":"none","endpointUrl":"","documentationUrl":"","tags":[]}
            5. If the user declines, respond with ONLY: {"confirmed":false}
            """;
    }

    private static McpMetadata? ParseCopilotResponse(string responseText, string repositoryUrl)
    {
        var jsonText = ExtractJson(responseText);
        if (jsonText == null)
            return GenerateFallbackMetadata(repositoryUrl);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<CopilotMetadataResponse>(jsonText, options);

            if (response == null || !response.Confirmed)
                return null; // User cancelled

            return new McpMetadata
            {
                Name = Sanitize(response.Name),
                Description = response.Description ?? string.Empty,
                Summary = response.Summary ?? string.Empty,
                Version = string.IsNullOrWhiteSpace(response.Version) ? "1.0.0" : response.Version,
                Company = response.Company ?? string.Empty,
                Owner = response.Owner ?? string.Empty,
                Status = response.Status ?? "active",
                Lifecycle = response.Lifecycle ?? "development",
                AuthMethod = response.AuthMethod ?? "none",
                EndpointUrl = response.EndpointUrl ?? string.Empty,
                DocumentationUrl = response.DocumentationUrl ?? string.Empty,
                Tags = response.Tags ?? new List<string>(),
                RepositoryUrl = repositoryUrl
            };
        }
        catch
        {
            return GenerateFallbackMetadata(repositoryUrl);
        }
    }

    private static string? ExtractJson(string text)
    {
        // Strip markdown code fences if present
        var cleaned = System.Text.RegularExpressions.Regex.Replace(
            text, @"```(?:json)?\s*([\s\S]*?)```", "$1").Trim();

        // Find the last complete JSON object in the text
        var lastBrace = cleaned.LastIndexOf('{');
        if (lastBrace < 0)
            return null;

        var sub = cleaned[lastBrace..];
        var depth = 0;
        for (var i = 0; i < sub.Length; i++)
        {
            if (sub[i] == '{') depth++;
            else if (sub[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return sub[..(i + 1)];
            }
        }

        return null;
    }

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(
            value.ToLower().Replace(" ", "-").Replace("_", "-"), @"[^a-z0-9-]", "");
    }

    private static McpMetadata GenerateFallbackMetadata(string repositoryUrl)
    {
        var parts = repositoryUrl.TrimEnd('/').Split('/');
        var repoName = Sanitize(parts.LastOrDefault() ?? "mcp-server");
        var orgName = parts.Length >= 2 ? parts[^2] : "unknown";

        return new McpMetadata
        {
            Name = repoName,
            Description = $"MCP server from {repositoryUrl}",
            Summary = repoName,
            Version = "1.0.0",
            Company = orgName,
            Owner = orgName,
            Status = "active",
            Lifecycle = "development",
            AuthMethod = "none",
            RepositoryUrl = repositoryUrl
        };
    }

    private class CopilotMetadataResponse
    {
        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("company")]
        public string? Company { get; set; }

        [JsonPropertyName("owner")]
        public string? Owner { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("lifecycle")]
        public string? Lifecycle { get; set; }

        [JsonPropertyName("authMethod")]
        public string? AuthMethod { get; set; }

        [JsonPropertyName("endpointUrl")]
        public string? EndpointUrl { get; set; }

        [JsonPropertyName("documentationUrl")]
        public string? DocumentationUrl { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }
    }
}

