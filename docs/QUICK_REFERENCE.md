# Quick Reference Guide

Quick reference for common tasks in the MCP Registry.

## Quick Start (First Time)

```bash
# 1. Clone the repository
git clone https://github.com/tatatatamami/copilot-agent-mcp-registry.git
cd copilot-agent-mcp-registry

# 2. Build the solution
dotnet build

# 3. Run the registration tool
dotnet run --project src/McpRegistration.Cli
```

## Register a New MCP

```bash
# Run the CLI assistant
dotnet run --project src/McpRegistration.Cli

# Follow prompts, then push and create PR
git push -u origin mcp-registration/<your-mcp-name>
```

## Metadata Quick Reference

### Required Fields

| Field | Format | Example |
|-------|--------|---------|
| name | lowercase-with-hyphens | `my-awesome-mcp` |
| description | 10-500 chars | `Provides data analysis` |
| version | semver | `1.0.0` |
| company | text | `Contoso Ltd` |
| owner | text | `Platform Team` |
| status | enum | `active` |
| lifecycle | enum | `production` |
| authMethod | enum | `api-key` |

### Status Options

- `active` - Currently operational
- `deprecated` - Marked for retirement
- `planned` - Not yet available

### Lifecycle Options

- `design` - Planning phase
- `development` - Under development
- `testing` - In QA
- `preview` - Public beta
- `production` - Generally available
- `deprecated` - Being phased out
- `retired` - No longer available

### Authentication Methods

- `none` - No auth (dev only)
- `api-key` - API key in header
- `oauth2` - OAuth 2.0 flow
- `entra-id` - Microsoft Entra ID

## File Locations

```
apis/<mcp-name>/
├── openapi.json      # OpenAPI 3.0 spec
├── metadata.json     # Metadata for API Center
└── README.md         # Documentation
```

## Common Commands

### Build

```bash
dotnet build                    # Build entire solution
dotnet build --configuration Release  # Release build
dotnet clean                    # Clean build artifacts
```

### Run CLI

```bash
# Interactive mode
dotnet run --project src/McpRegistration.Cli

# From published binary
dotnet publish -c Release
./src/McpRegistration.Cli/bin/Release/net10.0/McpRegistration.Cli
```

### Git Operations

```bash
# Check status
git status

# View changes
git diff

# Discard changes
git checkout -- <file>

# Delete test branch
git branch -D mcp-registration/<name>
```

### Validate JSON Files

```bash
# Using jq
jq empty apis/my-mcp/metadata.json    # Returns nothing if valid
jq . apis/my-mcp/metadata.json        # Pretty print

# Check against schema
jq -s '.[0] as $schema | .[1] | . as $data | $schema' \
  templates/metadata-schema.json \
  apis/my-mcp/metadata.json
```

## Troubleshooting Quick Fixes

### Build Fails

```bash
dotnet clean
rm -rf src/*/bin src/*/obj
dotnet restore
dotnet build
```

### CLI Won't Run

```bash
# Check .NET version
dotnet --version  # Should be 10.0+

# Rebuild
dotnet build --no-incremental
```

### Git Issues

```bash
# Reset to clean state
git reset --hard HEAD
git clean -fd

# Start over on new branch
git checkout main
git pull
git checkout -b new-branch-name
```

### Invalid Name Error

Valid characters: `a-z`, `0-9`, `-`
- ✅ `customer-api-mcp`
- ✅ `data-analysis-v2`
- ❌ `Customer_API` (uppercase, underscore)
- ❌ `my api` (space)

### Validation Fails

Check:
1. Name is lowercase with hyphens only
2. Version follows semver (x.y.z)
3. Email is valid format
4. URLs start with http:// or https://
5. Status is: active, deprecated, or planned
6. Lifecycle is valid enum value
7. Auth method is valid enum value

## Azure Quick Reference

### Check API Center

```bash
az apic api list \
  --resource-group <rg> \
  --service-name <api-center> \
  --output table
```

### View Specific API

```bash
az apic api show \
  --resource-group <rg> \
  --service-name <api-center> \
  --api-id <mcp-name>
```

### GitHub Actions Logs

1. Go to repository on GitHub
2. Click "Actions" tab
3. Click on workflow run
4. Expand steps to view logs

## File Templates

### Minimal metadata.json

```json
{
  "name": "my-mcp",
  "description": "Description here",
  "version": "1.0.0",
  "company": "Company Name",
  "status": "active",
  "lifecycle": "development",
  "owner": "Team Name",
  "authMethod": "api-key",
  "contactEmail": "",
  "endpointUrl": "",
  "documentationUrl": "",
  "tags": [],
  "customProperties": {}
}
```

### Minimal openapi.json

```json
{
  "openapi": "3.0.0",
  "info": {
    "title": "My MCP",
    "description": "Description",
    "version": "1.0.0"
  },
  "servers": [],
  "paths": {
    "/health": {
      "get": {
        "summary": "Health check",
        "responses": {
          "200": { "description": "OK" }
        }
      }
    }
  }
}
```

## Environment Variables (GitHub Actions)

Set these as repository secrets:

```bash
AZURE_CLIENT_ID           # Service principal ID
AZURE_TENANT_ID           # Azure AD tenant
AZURE_SUBSCRIPTION_ID     # Azure subscription
AZURE_RESOURCE_GROUP      # Resource group name
AZURE_API_CENTER_NAME     # API Center instance name
```

## Useful Links

- [Main README](../README.md)
- [Full Runbook](RUNBOOK.md)
- [Azure Setup](AZURE_SETUP.md)
- [Demo Scenario](DEMO_SCENARIO.md)
- [Contributing Guide](../CONTRIBUTING.md)

## Tips & Best Practices

### Naming

- Use descriptive names: `customer-insights-mcp` not `api1`
- Include service domain: `payment-processor-mcp`
- Version in name if multiple: `data-api-v2`

### Documentation

- Write clear descriptions (think: "What does this do?")
- Include authentication requirements
- Add contact information
- Link to detailed docs if available

### Lifecycle Management

- Start at `development` for new MCPs
- Move to `testing` when ready for QA
- Use `preview` for limited production
- Mark `production` only when stable
- Set `deprecated` with migration path before retirement

### Security

- Never commit secrets or keys
- Use HTTPS URLs only (production)
- Choose appropriate auth method
- Document security requirements
- Include contact for security issues

---

**Quick Help**: For detailed information, see the [full documentation](../README.md).
