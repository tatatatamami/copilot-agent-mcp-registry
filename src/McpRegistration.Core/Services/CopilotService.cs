using McpRegistration.Core.Models;
using System.Reflection;

namespace McpRegistration.Core.Services;

/// <summary>
/// GitHub Copilot SDK integration for AI-powered assistance
/// </summary>
public class CopilotService
{
    private readonly dynamic? _session;
    private readonly bool _isAvailable;

    public CopilotService()
    {
        try
        {
            // Use reflection to load GitHub Copilot SDK dynamically
            var copilotAssembly = Assembly.Load("GitHub.Copilot");
            var clientType = copilotAssembly.GetType("GitHub.Copilot.CopilotClient");
            
            if (clientType != null)
            {
                var client = Activator.CreateInstance(clientType);
                var createSessionMethod = clientType.GetMethod("CreateSession");
                _session = createSessionMethod?.Invoke(client, null);
                _isAvailable = _session != null;
                
                if (_isAvailable)
                {
                    Console.WriteLine("?? GitHub Copilot SDK initialized successfully");
                }
            }
            else
            {
                _isAvailable = false;
                Console.WriteLine("??  GitHub Copilot SDK not found");
            }
        }
        catch (Exception ex)
        {
            _isAvailable = false;
            Console.WriteLine($"??  GitHub Copilot not available: {ex.Message}");
            Console.WriteLine("   Continuing with basic functionality...");
        }
    }

    public bool IsAvailable => _isAvailable;

    /// <summary>
    /// Suggest MCP server name based on description
    /// </summary>
    public async Task<string> SuggestMcpNameAsync(string description)
    {
        if (!_isAvailable || _session == null)
        {
            return GenerateFallbackName(description);
        }

        try
        {
            var prompt = $@"Generate a MCP server name based on this description: ""{description}""

Requirements:
- lowercase only
- use hyphens to separate words
- maximum 50 characters
- descriptive and memorable
- follow naming conventions for APIs

Reply with ONLY the suggested name, nothing else. No explanations.";

            // Use dynamic to call the method
            dynamic message = CreateMessage(prompt);
            dynamic response = await _session.SendAndWaitAsync(message);
            
            var suggested = ((string)response.Content).Trim().ToLower()
                .Replace(" ", "-")
                .Replace("_", "-");

            // Clean up
            suggested = System.Text.RegularExpressions.Regex.Replace(suggested, @"[^a-z0-9-]", "");
            
            return suggested.Length > 50 ? suggested.Substring(0, 50) : suggested;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"??  Copilot suggestion failed: {ex.Message}");
            return GenerateFallbackName(description);
        }
    }

    private dynamic CreateMessage(string content)
    {
        var assembly = Assembly.Load("GitHub.Copilot");
        var messageType = assembly.GetType("GitHub.Copilot.CopilotMessage");
        var message = Activator.CreateInstance(messageType!);
        messageType!.GetProperty("Content")!.SetValue(message, content);
        return message!;
    }

    /// <summary>
    /// Validate field with AI assistance
    /// </summary>
    public async Task<(bool IsValid, string? Message, string? Suggestion)> ValidateFieldAsync(
        string fieldName,
        string value)
    {
        if (!_isAvailable || _session == null)
        {
            return (true, null, null);
        }

        try
        {
            var prompt = $@"Validate this MCP server field:
Field: {fieldName}
Value: '{value}'

Check if it follows best practices:
- Naming conventions
- Clarity and readability
- Common patterns

Respond in JSON format:
{{
  ""isValid"": true/false,
  ""message"": ""explanation if invalid"",
  ""suggestion"": ""corrected version if applicable""
}}";

            dynamic message = CreateMessage(prompt);
            dynamic response = await _session.SendAndWaitAsync(message);
            
            // Try to parse JSON response
            var json = System.Text.Json.JsonSerializer.Deserialize<ValidationResponse>((string)response.Content);
            
            if (json != null)
            {
                return (json.IsValid, json.Message, json.Suggestion);
            }
        }
        catch
        {
            // Fallback to basic validation
        }

        return (true, null, null);
    }

    /// <summary>
    /// Generate enhanced README with AI assistance
    /// </summary>
    public async Task<string> GenerateEnhancedReadmeAsync(McpMetadata metadata)
    {
        if (!_isAvailable || _session == null)
        {
            return string.Empty; // Use default README generation
        }

        try
        {
            var prompt = $@"Generate a comprehensive README.md for a MCP server with these details:
- Name: {metadata.Name}
- Description: {metadata.Description}
- Company: {metadata.Company}
- Authentication: {metadata.AuthMethod}

Include sections for:
1. Overview
2. Getting Started
3. API Endpoints
4. Authentication Guide
5. Examples
6. Configuration
7. Troubleshooting

Use markdown format. Be professional and concise.";

            dynamic message = CreateMessage(prompt);
            dynamic response = await _session.SendAndWaitAsync(message);
            return (string)response.Content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"??  Enhanced README generation failed: {ex.Message}");
            return string.Empty;
        }
    }

    private static string GenerateFallbackName(string description)
    {
        return description
            .ToLower()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Substring(0, Math.Min(50, description.Length));
    }

    private class ValidationResponse
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? Suggestion { get; set; }
    }
}

