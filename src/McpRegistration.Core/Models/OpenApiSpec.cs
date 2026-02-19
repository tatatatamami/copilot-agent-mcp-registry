namespace McpRegistration.Core.Models;

/// <summary>
/// Minimal OpenAPI 3.0 specification structure
/// </summary>
public class OpenApiSpec
{
    public string Openapi { get; set; } = "3.0.0";
    public OpenApiInfo Info { get; set; } = new();
    public List<OpenApiServer> Servers { get; set; } = new();
    public Dictionary<string, OpenApiPathItem> Paths { get; set; } = new();
}

public class OpenApiInfo
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public OpenApiContact? Contact { get; set; }
}

public class OpenApiContact
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class OpenApiServer
{
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class OpenApiPathItem
{
    public OpenApiOperation? Get { get; set; }
    public OpenApiOperation? Post { get; set; }
}

public class OpenApiOperation
{
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public Dictionary<string, OpenApiResponse> Responses { get; set; } = new();
}

public class OpenApiResponse
{
    public string Description { get; set; } = string.Empty;
}
