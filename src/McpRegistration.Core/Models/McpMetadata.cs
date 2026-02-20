namespace McpRegistration.Core.Models;

/// <summary>
/// Represents metadata for an MCP server registration
/// </summary>
public class McpMetadata
{
    /// <summary>
    /// Name of the MCP server (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the MCP server (required)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// One-line summary of the MCP server (optional)
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Version of the MCP server (required)
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Company or organization owning this MCP server (required)
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the MCP server (required)
    /// Values: active, deprecated, planned
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Lifecycle stage (required)
    /// Values: design, development, testing, preview, production, deprecated, retired
    /// </summary>
    public string Lifecycle { get; set; } = "development";

    /// <summary>
    /// Owner/contact person for this MCP server (required)
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Authentication method (required)
    /// Values: none, api-key, oauth2, entra-id
    /// </summary>
    public string AuthMethod { get; set; } = "api-key";

    /// <summary>
    /// Contact email for the owner
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// URL to the MCP server endpoint
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL to documentation
    /// </summary>
    public string DocumentationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Additional custom properties
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; set; } = new();

    /// <summary>
    /// Repository URL where this MCP server source is located
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;
}
