using System.Text.Json;
using McpRegistration.Core.Models;

namespace McpRegistration.Core.Services;

/// <summary>
/// Service for generating MCP registration files
/// </summary>
public class FileGenerationService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public FileGenerationService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Generate metadata.json content
    /// </summary>
    public string GenerateMetadataJson(McpMetadata metadata)
    {
        return JsonSerializer.Serialize(metadata, _jsonOptions);
    }

    /// <summary>
    /// Generate minimal OpenAPI specification
    /// </summary>
    public string GenerateOpenApiSpec(McpMetadata metadata)
    {
        var spec = new OpenApiSpec
        {
            Info = new OpenApiInfo
            {
                Title = metadata.Name,
                Description = metadata.Description,
                Version = metadata.Version,
                Contact = new OpenApiContact
                {
                    Name = metadata.Owner,
                    Email = metadata.ContactEmail
                }
            },
            Servers = new List<OpenApiServer>(),
            Paths = new Dictionary<string, OpenApiPathItem>
            {
                ["/health"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        Summary = "Health check endpoint",
                        Description = "Returns the health status of the MCP server",
                        OperationId = "getHealth",
                        Responses = new Dictionary<string, OpenApiResponse>
                        {
                            ["200"] = new OpenApiResponse { Description = "Healthy" },
                            ["503"] = new OpenApiResponse { Description = "Unhealthy" }
                        }
                    }
                },
                ["/mcp"] = new OpenApiPathItem
                {
                    Post = new OpenApiOperation
                    {
                        Summary = "MCP protocol endpoint",
                        Description = "Main endpoint for MCP protocol communication",
                        OperationId = "mcpEndpoint",
                        Responses = new Dictionary<string, OpenApiResponse>
                        {
                            ["200"] = new OpenApiResponse { Description = "Success" },
                            ["400"] = new OpenApiResponse { Description = "Bad Request" },
                            ["401"] = new OpenApiResponse { Description = "Unauthorized" }
                        }
                    }
                }
            }
        };

        if (!string.IsNullOrEmpty(metadata.EndpointUrl))
        {
            spec.Servers.Add(new OpenApiServer
            {
                Url = metadata.EndpointUrl,
                Description = $"{metadata.Name} server"
            });
        }

        return JsonSerializer.Serialize(spec, _jsonOptions);
    }

    /// <summary>
    /// Generate README.md documentation
    /// </summary>
    public string GenerateReadme(McpMetadata metadata)
    {
        return $@"# {metadata.Name}

## Overview

{metadata.Description}

## Metadata

- **Version**: {metadata.Version}
- **Owner**: {metadata.Owner}
- **Company**: {metadata.Company}
- **Status**: {metadata.Status}
- **Lifecycle**: {metadata.Lifecycle}

## Authentication

This MCP server uses **{metadata.AuthMethod}** for authentication.

{GetAuthenticationDetails(metadata.AuthMethod)}

## Contact

- **Owner**: {metadata.Owner}
{(string.IsNullOrEmpty(metadata.ContactEmail) ? "" : $"- **Email**: {metadata.ContactEmail}")}

## Endpoint

{(string.IsNullOrEmpty(metadata.EndpointUrl) ? "Endpoint URL will be provided upon deployment." : $"- **URL**: {metadata.EndpointUrl}")}

## Documentation

{(string.IsNullOrEmpty(metadata.DocumentationUrl) ? "Additional documentation will be provided." : $"For more details, see: {metadata.DocumentationUrl}")}

## Tags

{(metadata.Tags.Any() ? string.Join(", ", metadata.Tags.Select(t => $"`{t}`")) : "No tags specified")}

## Usage

To use this MCP server:

1. Ensure you have the appropriate authentication credentials
2. Connect to the endpoint using an MCP-compatible client
3. Follow the API specification defined in `openapi.json`

## Support

For support or questions, please contact {metadata.Owner}{(string.IsNullOrEmpty(metadata.ContactEmail) ? "." : $" at {metadata.ContactEmail}.")}
";
    }

    private string GetAuthenticationDetails(string authMethod)
    {
        return authMethod.ToLower() switch
        {
            "none" => "No authentication required. This is suitable for internal or development environments only.",
            "api-key" => @"### API Key Authentication

To authenticate, include your API key in the request headers:

```
Authorization: Bearer YOUR_API_KEY
```

Contact the server owner to obtain an API key.",
            "oauth2" => @"### OAuth 2.0 Authentication

This server uses OAuth 2.0 for authentication. Follow the OAuth flow to obtain an access token:

1. Register your application with the OAuth provider
2. Obtain authorization from the user
3. Exchange the authorization code for an access token
4. Include the access token in your requests

Contact the server owner for OAuth configuration details.",
            "entra-id" => @"### Microsoft Entra ID Authentication

This server uses Microsoft Entra ID (formerly Azure AD) for authentication:

1. Register your application in Azure Portal
2. Request the appropriate scopes/permissions
3. Obtain an access token from Entra ID
4. Include the token in the Authorization header

Required scopes and application registration details are available from the server owner.",
            _ => "Authentication details will be provided by the server owner."
        };
    }
}
