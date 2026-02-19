---
name: MCP Registration Request
about: Request to register a new MCP server
title: '[MCP] Register <mcp-name>'
labels: ['registration', 'needs-review']
assignees: ''
---

## MCP Information

**MCP Name**: <!-- e.g., customer-insights-mcp -->

**Description**: <!-- Brief description of what this MCP does -->

**Company/Organization**: <!-- e.g., Contoso Corporation -->

**Owner**: <!-- Team or person responsible -->

**Contact Email**: <!-- Support email address -->

## Technical Details

**Version**: <!-- e.g., 1.0.0 -->

**Status**: <!-- active / deprecated / planned -->

**Lifecycle Stage**: <!-- design / development / testing / preview / production / deprecated / retired -->

**Authentication Method**: <!-- none / api-key / oauth2 / entra-id -->

**Endpoint URL**: <!-- https://your-endpoint.example.com (if available) -->

**Documentation URL**: <!-- https://docs.example.com (if available) -->

## Registration Method

How do you plan to register this MCP?

- [ ] I will use the CLI tool (`dotnet run --project src/McpRegistration.Cli`)
- [ ] I need assistance with registration
- [ ] I have questions about the process

## Additional Information

**Tags**: <!-- Comma-separated tags for categorization -->

**Custom Properties**: <!-- Any additional metadata (optional) -->

## Pre-submission Checklist

Before submitting this issue:

- [ ] I have read the [README.md](../README.md)
- [ ] I have reviewed the [RUNBOOK.md](../docs/RUNBOOK.md)
- [ ] I have appropriate approval to register this MCP
- [ ] I have the necessary information to complete registration
- [ ] I understand the registration workflow (CLI → PR → Review → Merge → API Center)

## Questions or Concerns

<!-- Any questions about the registration process or requirements -->

---

**Next Steps**: 
1. Maintainers will review this request
2. Upon approval, you'll receive instructions to proceed with CLI registration
3. You'll create a PR with generated files
4. After PR approval and merge, the MCP will be registered to Azure API Center
