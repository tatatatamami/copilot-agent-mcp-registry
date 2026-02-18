# MCP Registration Runbook

## Overview

This runbook provides operational procedures for managing MCP server registrations in the registry.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Registration Process](#registration-process)
3. [Approval Guidelines](#approval-guidelines)
4. [Troubleshooting](#troubleshooting)
5. [Maintenance](#maintenance)

## Prerequisites

### For Registrants

- Access to this GitHub repository
- .NET 10.0 SDK or later installed
- Git configured with appropriate credentials
- Knowledge of the MCP server details

### For Approvers

- Approver role in the GitHub repository
- Understanding of organizational governance policies
- Access to verify MCP server details

### For Azure Operations

- Azure subscription access
- API Center instance created
- Service Principal with appropriate permissions
- GitHub Actions secrets configured

## Registration Process

### Step 1: Prepare Information

Collect the following information before starting:

- **Name**: MCP server identifier (lowercase, hyphens only)
- **Description**: Clear, concise description (2-3 sentences)
- **Version**: Semantic version (e.g., 1.0.0)
- **Company**: Organization name
- **Owner**: Team or person name
- **Contact Email**: Valid email address
- **Status**: active, deprecated, or planned
- **Lifecycle**: Current stage (development, production, etc.)
- **Auth Method**: none, api-key, oauth2, or entra-id
- **Endpoint URL**: (optional) Production endpoint
- **Documentation URL**: (optional) Link to docs
- **Tags**: (optional) Categorization tags

### Step 2: Run Registration Tool

```bash
# Clone repository if not already done
git clone https://github.com/tatatatamami/copilot-agent-mcp-registry.git
cd copilot-agent-mcp-registry

# Build the solution
dotnet build

# Run the registration CLI
dotnet run --project src/McpRegistration.Cli
```

### Step 3: Follow Interactive Prompts

The CLI will ask for each required field. Provide accurate information.

**Tips:**
- Use descriptive, searchable names
- Keep descriptions clear and concise
- Choose lifecycle stage honestly (don't mark as production prematurely)
- Select appropriate authentication method for security requirements

### Step 4: Review Generated Files

After the CLI completes, review the generated files:

```bash
ls -la apis/<your-mcp-name>/
# Should contain:
# - openapi.json
# - metadata.json
# - README.md
```

Verify:
- JSON files are valid (use `jq` or a JSON validator)
- README is readable and complete
- No sensitive data (passwords, keys) in any files

### Step 5: Create Pull Request

```bash
# Push your branch
git push -u origin mcp-registration/<your-mcp-name>

# Create PR on GitHub
# Use the mcp-registration.md template
```

### Step 6: Address Review Comments

Reviewers may request changes. Address feedback and update the PR as needed.

### Step 7: Merge and Verify

After approval:
1. PR is merged to main
2. GitHub Actions runs automatically
3. MCP is registered to Azure API Center
4. Verify in API Center Portal

## Approval Guidelines

### Reviewer Checklist

When reviewing MCP registration PRs:

#### Metadata Validation
- [ ] Name follows naming convention (lowercase, hyphens, descriptive)
- [ ] Description is clear and useful
- [ ] Company/owner information is accurate
- [ ] Lifecycle stage is appropriate
- [ ] Contact information is valid

#### Security Review
- [ ] Authentication method is appropriate
- [ ] No secrets or credentials in files
- [ ] Endpoint URLs use HTTPS (if provided)
- [ ] Security contact is reachable

#### Compliance
- [ ] Owner has authority to register this MCP
- [ ] Appropriate approvals obtained
- [ ] Aligns with organizational policies
- [ ] Documentation is sufficient

#### Technical Quality
- [ ] OpenAPI spec is valid JSON
- [ ] Metadata JSON is valid
- [ ] README provides necessary information
- [ ] Files follow repository conventions

### Approval Authority

| Lifecycle Stage | Required Approvers |
|----------------|-------------------|
| design, development, testing | Team lead or above |
| preview | Department head or above |
| production | Department head + Security review |

### Common Rejection Reasons

- Insufficient documentation
- Inappropriate lifecycle stage
- Missing owner approval
- Security concerns
- Non-compliant naming
- Duplicate registration

## Troubleshooting

### CLI Tool Issues

#### Problem: "Not in a git repository"
**Solution**: Run the CLI from within the cloned repository directory.

#### Problem: Build fails
**Solution**: 
```bash
dotnet clean
dotnet restore
dotnet build
```

#### Problem: Name validation fails
**Solution**: Use only lowercase letters, numbers, and hyphens. No spaces or special characters.

### GitHub Actions Issues

#### Problem: Workflow doesn't trigger
**Causes:**
- Changes not in `apis/` directory
- Pushed to wrong branch

**Solution:** Ensure changes are merged to `main` and include files in `apis/`.

#### Problem: Azure authentication fails
**Causes:**
- Missing or incorrect secrets
- Service Principal permissions insufficient

**Solution:**
1. Verify all secrets are configured in GitHub repository settings
2. Test Service Principal access

#### Problem: API registration fails
**Causes:**
- API Center instance not accessible
- Invalid OpenAPI specification
- Duplicate registration

**Solution:**
1. Verify API Center exists
2. Validate OpenAPI spec locally

### Portal Access Issues

#### Problem: MCP not visible in portal
**Causes:**
- Registration failed
- Portal cache not refreshed
- Permissions issue

**Solution:**
1. Check GitHub Actions logs for errors
2. Verify in Azure Portal that API exists in API Center
3. Clear portal cache and reload
4. Verify user permissions in API Center

## Maintenance

### Regular Tasks

#### Weekly
- Review open PRs awaiting approval
- Check GitHub Actions for failures
- Monitor API Center for orphaned entries

#### Monthly
- Audit registered MCPs for lifecycle updates
- Review deprecated MCPs for retirement
- Update documentation as needed
- Verify contact information is current

#### Quarterly
- Review and update metadata schema if needed
- Security audit of registered MCPs
- Compliance review
- Update runbook based on learnings

### Updating an Existing MCP

To update an existing MCP registration:

1. Make changes to files in `apis/<mcp-name>/`
2. Update version number in metadata.json
3. Document changes in README.md
4. Create PR with description of changes
5. Follow standard approval process

### Deprecating an MCP

1. Update `status` to `deprecated` in metadata.json
2. Update `lifecycle` to `deprecated`
3. Add deprecation notice to README.md
4. Include migration path if applicable
5. Set sunset date in custom properties

### Retiring an MCP

1. Update `lifecycle` to `retired` in metadata.json
2. Update README with retirement date
3. Remove from active catalog listings
4. Archive API in API Center (do not delete for audit trail)
5. Document replacement or migration path

## Support Escalation

| Issue Type | Contact |
|-----------|---------|
| CLI Tool Issues | Platform Team |
| GitHub Actions | DevOps Team |
| Azure API Center | Cloud Operations |
| Security Concerns | Security Team |
| Policy Questions | Governance Team |

## Additional Resources

- [Main README](../README.md)
- [Azure API Center Documentation](https://learn.microsoft.com/azure/api-center/)
- [GitHub Actions Documentation](https://docs.github.com/actions)

---

**Last Updated**: 2026-02-18  
**Document Owner**: Platform Team  
**Review Frequency**: Quarterly
