# MCP Server Registry with Azure API Center

> Automated MCP (Model Context Protocol) server registration system using Git-based workflow (Pull Requests), Copilot SDK, and Azure API Center integration.
>
> [!WARNING]
> This repository is an experimental sample/PoC for MCP server registration workflows.
> It is intentionally minimal and not production-ready.

## 🎯 Overview

This repository serves as a centralized registry for MCP servers within your organization. It provides:

- **📝 Git-based Registration**: Submit MCP servers via Pull Requests for review and approval
- **🤖 Copilot SDK Assistant**: Interactive CLI tool that guides you through the registration process
- **☁️ Azure API Center Integration**: Automatic registration to Azure API Center upon PR merge
- **📚 Self-hosted Portal**: Browse and discover MCP servers through Azure API Center Portal
- **✅ Governance**: Built-in approval workflow with audit trail

## 🏗️ Architecture

```
┌─────────────────┐
│  Developer      │
│  (CLI Tool)     │
└────────┬────────┘
         │ 1. Input repository URL
         │ 2. Copilot generates metadata (user confirms)
         │ 3. Generate files + branch + commit
         │ 4. Optional push + PR
         ▼
┌─────────────────────────┐
│  GitHub Repository      │
│  ├── apis/              │
│  │   └── <mcp-name>/    │
│  │       ├── openapi.json
│  │       ├── metadata.json
│  │       └── README.md   │
│  └── .github/workflows/ │
└────────┬────────────────┘
         │ 5. PR Merge
         │ 6. GitHub Actions
         ▼
┌─────────────────────────┐
│  Azure API Center       │
│  (System of Record)     │
└────────┬────────────────┘
         │ 7. Portal Access
         ▼
┌─────────────────────────┐
│  API Center Portal      │
│  (Self-hosted)          │
└─────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- Git
- Access to this repository
- (For deployment) Azure subscription with API Center instance

### Installation

1. Clone the repository:
```bash
git clone https://github.com/tatatatamami/copilot-agent-mcp-registry.git
cd copilot-agent-mcp-registry
```

2. Build the CLI tool:
```bash
dotnet build
```

### Register a New MCP Server

1. Run the registration assistant:
```bash
dotnet run --project src/McpRegistration.Cli
```

2. Follow the interactive flow:
   - Enter the MCP server repository URL
   - Let Copilot generate metadata from that URL
   - Review and confirm the generated metadata

3. The tool will then:
   - ✅ Validate metadata
   - ✅ Generate required files in `apis/<mcp-name>/`
   - ✅ Create a new Git branch
   - ✅ Commit the changes

4. Optionally push and create a Pull Request from the CLI prompt.
   - If you choose **No**, local files and commit are kept and you can push manually later.
   - If push or `gh pr create` fails (for example, missing auth/CLI), use manual git/PR steps.

5. After approval and merge, GitHub Actions will automatically register your MCP to Azure API Center

## 📁 Repository Structure

```
├── .github/
│   ├── workflows/
│   │   └── register-to-api-center.yml    # Automated registration workflow
│   └── PULL_REQUEST_TEMPLATE/
│       └── mcp-registration.md           # PR template for reviews
├── apis/
│   └── <mcp-name>/                       # One directory per MCP server
│       ├── openapi.json                  # OpenAPI 3.0 specification
│       ├── metadata.json                 # Metadata for API Center
│       └── README.md                     # Documentation
├── catalog/
│   └── index.yaml                        # Optional: catalog index
├── src/
│   ├── McpRegistration.Cli/              # Interactive CLI tool
│   └── McpRegistration.Core/             # Core libraries
│       ├── Models/                       # Data models
│       └── Services/                     # Business logic
└── README.md                             # This file
```

> [!NOTE]
> Several entries currently under `apis/` are sample/demo artifacts used to demonstrate this PoC flow.

## 📋 Metadata Schema

Each MCP server requires the following metadata:

### Required Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `name` | string | MCP server name (lowercase, hyphens) | `my-awesome-mcp` |
| `description` | string | Brief description | `Provides data analysis capabilities` |
| `version` | string | Semantic version | `1.0.0` |
| `company` | string | Owning organization | `Contoso Ltd` |
| `owner` | string | Owner name | `Platform Team` |
| `status` | string | Current status | `active`, `deprecated`, `planned` |
| `lifecycle` | string | Development stage | `production`, `development`, etc. |
| `authMethod` | string | Authentication type | `api-key`, `oauth2`, `entra-id`, `none` |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `contactEmail` | string | Contact email address |
| `endpointUrl` | string | MCP server endpoint |
| `documentationUrl` | string | Link to documentation |
| `tags` | array | Categorization tags |
| `customProperties` | object | Additional metadata |

## 🔐 Authentication Methods

### API Key (`api-key`)
Standard API key authentication using Bearer tokens. Suitable for most internal services.

### OAuth 2.0 (`oauth2`)
OAuth 2.0 flow for delegated access. Use when user authorization is required.

### Microsoft Entra ID (`entra-id`)
Entra ID (formerly Azure AD) authentication. Recommended for enterprise scenarios with existing Azure/Microsoft 365 integration.

### None (`none`)
No authentication required. **Only use for development/internal environments.**

## 🔄 Lifecycle Stages

| Stage | Description | When to Use |
|-------|-------------|-------------|
| `design` | API is being designed | Initial planning phase |
| `development` | Under active development | Default for new MCPs |
| `testing` | In testing/QA phase | Before production release |
| `preview` | Public preview/beta | Limited production use |
| `production` | Generally available | Stable, production-ready |
| `deprecated` | Marked for retirement | Supported but not recommended |
| `retired` | No longer available | Shut down |

## ⚙️ GitHub Actions Setup

### Required Secrets

Configure these secrets in your GitHub repository settings:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Service Principal Client ID |
| `AZURE_TENANT_ID` | Azure AD Tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID |
| `AZURE_API_CENTER_NAME` | API Center instance name |
| `AZURE_RESOURCE_GROUP` | Resource group name |

### Azure Setup

1. Create an Azure API Center instance:
```bash
az apic create \
  --resource-group <resource-group> \
  --name <api-center-name> \
  --location <region>
```

2. Create a Service Principal for GitHub Actions:
```bash
az ad sp create-for-rbac \
  --name "GitHub-Actions-API-Center" \
  --role "API Management Service Contributor" \
  --scopes /subscriptions/<subscription-id>/resourceGroups/<resource-group> \
  --sdk-auth
```

3. Configure federated credentials for OIDC (recommended):
```bash
az ad app federated-credential create \
  --id <app-id> \
  --parameters credential.json
```

## 📊 Self-hosted API Center Portal

To set up the self-hosted portal for browsing registered APIs:

1. Follow the Microsoft Learn guide: [Self-host API Center Portal](https://learn.microsoft.com/azure/api-center/self-host-api-center-portal)

2. Configure portal to point to your API Center instance

3. Deploy to Azure App Service or your preferred hosting platform

The portal provides:
- 🔍 Search and discovery of registered MCPs
- 📖 API documentation viewer
- 🏷️ Filtering by metadata (company, lifecycle, tags)
- 📋 OpenAPI specification viewer

## 🛠️ Development

### Build the Solution

```bash
dotnet build
```

### Run Tests (if available)

```bash
dotnet test
```

### Run the CLI Locally

```bash
cd src/McpRegistration.Cli
dotnet run
```

## 📚 References

- [Azure API Center Documentation](https://learn.microsoft.com/azure/api-center/)
- [Register APIs with GitHub Actions](https://learn.microsoft.com/azure/api-center/register-apis-github-actions)
- [API Center Metadata](https://learn.microsoft.com/azure/api-center/metadata)
- [Self-host API Center Portal](https://learn.microsoft.com/azure/api-center/self-host-api-center-portal)
- [MCP (Model Context Protocol) Specification](https://modelcontextprotocol.io/)

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Submit a Pull Request
4. Ensure all checks pass

For MCP server registrations, use the CLI tool and follow the standard registration process.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For questions or issues:
1. Check existing documentation
2. Search closed issues
3. Create a new issue with detailed description
4. Contact the platform team

---

**System of Record**: Azure API Center is the authoritative source for all registered MCP servers. This repository serves as the registration interface and audit trail.
