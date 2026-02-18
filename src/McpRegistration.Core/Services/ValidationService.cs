using McpRegistration.Core.Models;

namespace McpRegistration.Core.Services;

/// <summary>
/// Service for validating MCP metadata
/// </summary>
public class ValidationService
{
    private static readonly string[] ValidStatuses = { "active", "deprecated", "planned" };
    private static readonly string[] ValidLifecycles = { "design", "development", "testing", "preview", "production", "deprecated", "retired" };
    private static readonly string[] ValidAuthMethods = { "none", "api-key", "oauth2", "entra-id" };

    /// <summary>
    /// Validate metadata and return list of errors
    /// </summary>
    public List<string> ValidateMetadata(McpMetadata metadata)
    {
        var errors = new List<string>();

        // Required fields
        if (string.IsNullOrWhiteSpace(metadata.Name))
            errors.Add("Name is required");

        if (string.IsNullOrWhiteSpace(metadata.Description))
            errors.Add("Description is required");

        if (string.IsNullOrWhiteSpace(metadata.Company))
            errors.Add("Company is required");

        if (string.IsNullOrWhiteSpace(metadata.Owner))
            errors.Add("Owner is required");

        // Validate status
        if (!ValidStatuses.Contains(metadata.Status.ToLower()))
            errors.Add($"Status must be one of: {string.Join(", ", ValidStatuses)}");

        // Validate lifecycle
        if (!ValidLifecycles.Contains(metadata.Lifecycle.ToLower()))
            errors.Add($"Lifecycle must be one of: {string.Join(", ", ValidLifecycles)}");

        // Validate auth method
        if (!ValidAuthMethods.Contains(metadata.AuthMethod.ToLower()))
            errors.Add($"AuthMethod must be one of: {string.Join(", ", ValidAuthMethods)}");

        // Validate name format (lowercase, alphanumeric, hyphens only)
        if (!string.IsNullOrWhiteSpace(metadata.Name) && !IsValidName(metadata.Name))
            errors.Add("Name must contain only lowercase letters, numbers, and hyphens");

        // Validate version format (semantic versioning)
        if (!string.IsNullOrWhiteSpace(metadata.Version) && !IsValidVersion(metadata.Version))
            errors.Add("Version must follow semantic versioning (e.g., 1.0.0)");

        // Validate email if provided
        if (!string.IsNullOrWhiteSpace(metadata.ContactEmail) && !IsValidEmail(metadata.ContactEmail))
            errors.Add("ContactEmail must be a valid email address");

        // Validate URL if provided
        if (!string.IsNullOrWhiteSpace(metadata.EndpointUrl) && !IsValidUrl(metadata.EndpointUrl))
            errors.Add("EndpointUrl must be a valid URL");

        if (!string.IsNullOrWhiteSpace(metadata.DocumentationUrl) && !IsValidUrl(metadata.DocumentationUrl))
            errors.Add("DocumentationUrl must be a valid URL");

        return errors;
    }

    /// <summary>
    /// Get list of missing required fields
    /// </summary>
    public List<string> GetMissingRequiredFields(McpMetadata metadata)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(metadata.Name))
            missing.Add("name");

        if (string.IsNullOrWhiteSpace(metadata.Description))
            missing.Add("description");

        if (string.IsNullOrWhiteSpace(metadata.Company))
            missing.Add("company");

        if (string.IsNullOrWhiteSpace(metadata.Owner))
            missing.Add("owner");

        if (string.IsNullOrWhiteSpace(metadata.Status))
            missing.Add("status");

        if (string.IsNullOrWhiteSpace(metadata.Lifecycle))
            missing.Add("lifecycle");

        if (string.IsNullOrWhiteSpace(metadata.AuthMethod))
            missing.Add("authMethod");

        return missing;
    }

    private bool IsValidName(string name)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-z0-9-]+$");
    }

    private bool IsValidVersion(string version)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+\.\d+(-[a-zA-Z0-9.-]+)?$");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static string[] GetValidStatuses() => ValidStatuses;
    public static string[] GetValidLifecycles() => ValidLifecycles;
    public static string[] GetValidAuthMethods() => ValidAuthMethods;
}
