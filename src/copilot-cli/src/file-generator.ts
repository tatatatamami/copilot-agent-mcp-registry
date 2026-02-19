import * as fs from "fs/promises";
import * as path from "path";
import yaml from "js-yaml";
import type { McpMetadata, OpenApiSpec } from "./models.js";

export class FileGenerator {
  private readonly baseDir: string;

  constructor(baseDir: string = process.cwd()) {
    this.baseDir = baseDir;
  }

  async generateFiles(metadata: McpMetadata): Promise<void> {
    const apiDir = path.join(this.baseDir, "apis", metadata.name);

    // Create directory
    await fs.mkdir(apiDir, { recursive: true });

    // Generate files
    await Promise.all([
      this.generateMetadataJson(metadata, apiDir),
      this.generateOpenApiJson(metadata, apiDir),
      this.generateReadme(metadata, apiDir),
    ]);
  }

  private async generateMetadataJson(metadata: McpMetadata, apiDir: string): Promise<void> {
    const content = JSON.stringify(metadata, null, 2);
    await fs.writeFile(path.join(apiDir, "metadata.json"), content, "utf-8");
  }

  private async generateOpenApiJson(metadata: McpMetadata, apiDir: string): Promise<void> {
    const spec: OpenApiSpec = {
      openapi: "3.0.0",
      info: {
        title: metadata.name,
        description: metadata.description,
        version: metadata.version,
        contact: {
          name: metadata.owner,
          ...(metadata.contactEmail && { email: metadata.contactEmail }),
        },
      },
      servers: metadata.endpointUrl
        ? [
            {
              url: metadata.endpointUrl,
              description: `${metadata.name} server`,
            },
          ]
        : [],
      paths: {
        "/health": {
          get: {
            summary: "Health check endpoint",
            description: "Returns the health status of the MCP server",
            operationId: "getHealth",
            responses: {
              "200": {
                description: "Healthy",
              },
              "503": {
                description: "Unhealthy",
              },
            },
          },
        },
        "/mcp": {
          post: {
            summary: "MCP protocol endpoint",
            description: "Main endpoint for MCP protocol communication",
            operationId: "mcpEndpoint",
            responses: {
              "200": {
                description: "Success",
              },
              "400": {
                description: "Bad Request",
              },
              "401": {
                description: "Unauthorized",
              },
            },
          },
        },
      },
    };

    const content = JSON.stringify(spec, null, 2);
    await fs.writeFile(path.join(apiDir, "openapi.json"), content, "utf-8");
  }

  private async generateReadme(metadata: McpMetadata, apiDir: string): Promise<void> {
    const authDetails = this.getAuthenticationDetails(metadata.authMethod);

    const content = `# ${metadata.name}

## Overview

${metadata.description}

## Metadata

- **Version**: ${metadata.version}
- **Owner**: ${metadata.owner}
- **Company**: ${metadata.company}
- **Status**: ${metadata.status}
- **Lifecycle**: ${metadata.lifecycle}
${metadata.contactEmail ? `- **Contact**: ${metadata.contactEmail}` : ""}
${metadata.endpointUrl ? `- **Endpoint**: ${metadata.endpointUrl}` : ""}
${metadata.documentationUrl ? `- **Documentation**: ${metadata.documentationUrl}` : ""}

## Authentication

This MCP server uses **${metadata.authMethod}** for authentication.

${authDetails}

## API Endpoints

### Health Check

\`\`\`
GET /health
\`\`\`

Returns the health status of the MCP server.

**Responses:**
- \`200\`: Healthy
- \`503\`: Unhealthy

### MCP Protocol Endpoint

\`\`\`
POST /mcp
\`\`\`

Main endpoint for MCP protocol communication.

**Responses:**
- \`200\`: Success
- \`400\`: Bad Request
- \`401\`: Unauthorized

## Contact

For questions or support regarding this MCP server, please contact:
- **Owner**: ${metadata.owner}
${metadata.contactEmail ? `- **Email**: ${metadata.contactEmail}` : ""}

## Tags

${metadata.tags && metadata.tags.length > 0 ? metadata.tags.map((tag) => `\`${tag}\``).join(" ") : "No tags"}

---

*This MCP server is registered in Azure API Center.*
`;

    await fs.writeFile(path.join(apiDir, "README.md"), content, "utf-8");
  }

  private getAuthenticationDetails(authMethod: string): string {
    switch (authMethod) {
      case "api-key":
        return `### API Key Authentication

Include your API key in the request header:

\`\`\`
Authorization: Bearer YOUR_API_KEY
\`\`\`

Contact the service owner to obtain an API key.`;

      case "oauth2":
        return `### OAuth 2.0 Authentication

This service uses OAuth 2.0 for authentication. Follow these steps:

1. Obtain client credentials from the service owner
2. Request an access token from the authorization server
3. Include the token in your requests:

\`\`\`
Authorization: Bearer ACCESS_TOKEN
\`\`\``;

      case "entra-id":
        return `### Microsoft Entra ID Authentication

This service uses Microsoft Entra ID (formerly Azure AD) for authentication.

**Required Scopes**: \`api://YOUR_APP_ID/.default\`

**Token Endpoint**: \`https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token\`

Include the Entra ID token in your requests:

\`\`\`
Authorization: Bearer ENTRA_ID_TOKEN
\`\`\``;

      case "none":
        return `### No Authentication Required

This service does not require authentication. **Use only for development or internal environments.**`;

      default:
        return "Authentication details not available.";
    }
  }
}
