# Demo Scenario: MCP Server Registration

## Overview

This document provides a step-by-step demo scenario for the MCP Server Registration system, from initial input through Azure API Center registration.

**Duration**: ~3-5 minutes  
**Goal**: Demonstrate the complete workflow from MCP registration to Portal visibility

## Demo Environment Setup

### Prerequisites (Done Before Demo)

1. ✅ Azure API Center instance created
2. ✅ GitHub repository cloned locally
3. ✅ .NET 10 SDK installed
4. ✅ GitHub Actions secrets configured
5. ✅ Solution built (`dotnet build`)
6. ✅ Self-hosted API Center Portal deployed

## Demo Script

### Act 1: Registration (1-2 minutes)

**Scenario**: A developer wants to register a new MCP server called "customer-insights-mcp"

#### Step 1: Launch the CLI Tool

```bash
cd copilot-agent-mcp-registry
dotnet run --project src/McpRegistration.Cli
```

**What to say:**
> "We have an interactive CLI tool powered by Copilot SDK that guides users through the registration process. Let's register a new MCP server for customer insights."

#### Step 2: Provide Information Interactively

The tool will prompt for each field. Provide these example values:

| Field | Value |
|-------|-------|
| MCP Server Name | `customer-insights-mcp` |
| Description | `Provides AI-powered customer behavior analytics and insights` |
| Version | `1.0.0` (default) |
| Company | `Contoso Corporation` |
| Owner | `Customer Analytics Team` |
| Contact Email | `analytics-team@contoso.com` |
| Status | `active` (default) |
| Lifecycle | `production` |
| Authentication Method | `entra-id` |
| Endpoint URL | `https://customer-insights-mcp.contoso.com` |
| Documentation URL | `https://docs.contoso.com/mcp/customer-insights` |
| Tags | `analytics, customer-insights, ai` |

**What to say:**
> "The tool asks for all required metadata. Notice how it validates inputs in real-time - the name must be lowercase with hyphens, lifecycle must be a valid stage, etc."

#### Step 3: Review Generated Files

```bash
# When prompted about committing, say 'y'
Would you like to commit these changes? (y/n): y
```

**What to say:**
> "The tool generates three files: a minimal OpenAPI specification, a complete metadata JSON file, and comprehensive README documentation. It also creates a Git branch and commits the changes automatically."

#### Step 4: Show Generated Files

```bash
cat apis/customer-insights-mcp/metadata.json | jq .
cat apis/customer-insights-mcp/README.md
```

**What to say:**
> "Here's the metadata in JSON format, ready for API Center. The README includes authentication details, usage instructions, and contact information."

### Act 2: Pull Request (1 minute)

#### Step 5: Push and Create PR

```bash
git push -u origin mcp-registration/customer-insights-mcp
```

Then navigate to GitHub UI.

**What to say:**
> "We push the branch to GitHub and create a Pull Request. The PR template provides a comprehensive checklist for reviewers."

#### Step 6: Show PR Template

Show the PR in GitHub with the template filled in.

**What to say:**
> "The PR template ensures reviewers check security, governance, and technical quality. This provides audit trail and approval workflow. In a real scenario, this would go through review, but for the demo, let's merge it."

#### Step 7: Merge PR

Click "Merge pull request" button.

**What to say:**
> "Once approved and merged, GitHub Actions automatically triggers."

### Act 3: Automation (1 minute)

#### Step 8: Show GitHub Actions

Navigate to Actions tab, show the running workflow.

**What to say:**
> "GitHub Actions detects the changes in the apis/ directory and automatically registers the MCP to Azure API Center using the Azure CLI."

#### Step 9: Show Workflow Details

Click into the running workflow, show the steps.

**What to say:**
> "The workflow authenticates to Azure, detects which APIs changed, and registers them with all metadata. It creates the API, version, and imports the OpenAPI specification."

Wait for workflow to complete (or show a pre-recorded successful run).

**What to say when complete:**
> "✓ The workflow completed successfully. Our MCP is now registered in Azure API Center."

### Act 4: Portal Verification (1 minute)

#### Step 10: Open API Center Portal

Navigate to the self-hosted API Center Portal in a browser.

**What to say:**
> "Now let's verify the registration in our self-hosted API Center Portal. This is a read-only interface where users can discover and explore registered MCPs."

#### Step 11: Search and View MCP

1. Show the list of APIs
2. Filter or search for "customer-insights-mcp"
3. Click on the MCP to view details

**What to say:**
> "Here's our newly registered MCP in the portal. Users can see the description, metadata, lifecycle stage, authentication requirements, and the full OpenAPI specification."

#### Step 12: Show Metadata and Documentation

Navigate through:
- Overview tab (description, owner, company)
- Metadata (lifecycle, status, tags)
- API Definition (OpenAPI spec)
- Documentation (if available)

**What to say:**
> "All the metadata we provided is searchable and visible here. Teams can filter by company, lifecycle, tags, or authentication method to find the right MCP for their needs."

## Demo Highlights

### Key Points to Emphasize

1. **Developer Experience**: Simple CLI tool, no complex forms
2. **Validation**: Built-in validation prevents errors
3. **Git-Based**: Audit trail, review process, version control
4. **Automation**: No manual Azure configuration needed
5. **Governance**: PR review ensures compliance
6. **Discoverability**: Self-hosted portal makes MCPs easy to find

### Value Proposition

- ✅ **Efficiency**: Minutes from idea to portal visibility
- ✅ **Quality**: Validation and review ensure consistency
- ✅ **Security**: Authentication methods enforced, no secrets in repo
- ✅ **Audit**: Complete Git history of all changes
- ✅ **Self-Service**: Developers register without ticketing systems
- ✅ **Centralized**: Azure API Center as System of Record

## Alternative Demo Flows

### Quick Demo (30 seconds)

1. Show pre-filled CLI generating files
2. Show merged PR
3. Show MCP in Portal
4. "Input → Files → PR → API Center → Portal. All automated."

### Deep-Dive Demo (10 minutes)

Include:
- Code walkthrough (Services, Models)
- Validation logic explanation
- GitHub Actions workflow details
- Azure API Center CLI commands
- Metadata schema customization
- Future enhancements (Entra ID automation)

## Common Q&A

**Q: What happens if someone registers a duplicate?**  
A: The validation checks if the directory exists. GitHub Actions handles idempotent registration.

**Q: Can I update an existing MCP?**  
A: Yes, modify the files and create a new PR. Update the version number.

**Q: What if the GitHub Actions fails?**  
A: The workflow logs show detailed error messages. Common issues are Azure auth or invalid OpenAPI.

**Q: How do I retire an MCP?**  
A: Update lifecycle to "retired" and merge. The MCP remains in API Center for audit trail.

**Q: Can I register non-MCP APIs?**  
A: Yes! The system works for any REST API. Just provide an OpenAPI spec.

**Q: Where do I get the API key / credentials?**  
A: Contact the MCP owner listed in the metadata. Future enhancement: automate via Entra ID.

## Files to Pre-prepare

For a smooth demo:

1. **Terminal window** with repository open
2. **Browser tab** with GitHub repo
3. **Browser tab** with API Center Portal  
4. **Backup**: Pre-recorded workflow run (in case of issues)
5. **Backup**: Screenshots of key steps

## Success Criteria

At the end of the demo, audience should understand:

- ✅ How to register an MCP using the CLI
- ✅ The PR-based approval workflow
- ✅ How GitHub Actions automates registration
- ✅ How to find MCPs in the Portal
- ✅ The value of centralized MCP registry

## Next Steps to Share

After the demo:

1. Repository URL: `https://github.com/tatatatamami/copilot-agent-mcp-registry`
2. Documentation: `README.md` and `docs/RUNBOOK.md`
3. Contact: Platform Team for access and questions
4. Try it: Clone, build, register your first MCP

---

**Demo Owner**: Platform Team  
**Last Updated**: 2026-02-18  
**Tested On**: .NET 10, Azure API Center, GitHub Actions
