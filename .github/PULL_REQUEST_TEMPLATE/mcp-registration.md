## MCP Server Registration

### Summary
<!-- Brief description of the MCP server being registered -->

### MCP Details
- **Name**: 
- **Company**: 
- **Owner**: 
- **Lifecycle**: 
- **Status**: 
- **Authentication**: 

### Files Included
- [ ] `apis/<mcp-name>/openapi.json` - Minimal OpenAPI specification
- [ ] `apis/<mcp-name>/metadata.json` - Complete metadata
- [ ] `apis/<mcp-name>/README.md` - Documentation

### Pre-Merge Checklist

#### Required Validations
- [ ] MCP name follows naming convention (lowercase, hyphens only)
- [ ] All required metadata fields are complete
- [ ] Owner information is accurate and reachable
- [ ] OpenAPI specification is valid JSON
- [ ] Metadata JSON is valid and complete
- [ ] README includes authentication details
- [ ] Lifecycle stage is appropriate for current state

#### Security Review
- [ ] Authentication method is appropriate for the use case
- [ ] No sensitive data (secrets, keys, passwords) in any files
- [ ] Endpoint URL is correct and accessible
- [ ] Contact information is verified

#### Governance Review
- [ ] Company/organization approval obtained
- [ ] Owner has committed to maintaining this MCP server
- [ ] Documentation is sufficient for consumers
- [ ] Tags are appropriate and consistent with other MCPs

### Additional Notes
<!-- Any additional context, considerations, or notes for reviewers -->

### Post-Merge Actions
After this PR is merged:
1. ✅ GitHub Actions will automatically register the API to Azure API Center
2. ✅ API will be visible in the self-hosted API Center Portal
3. ✅ Consumers can discover and access the API documentation

### Reviewer Notes
<!-- For use by approvers -->
- [ ] Metadata reviewed and approved
- [ ] Authentication approach verified
- [ ] Documentation is adequate
- [ ] Ready for Azure API Center registration

---

**Important**: Once merged, this MCP server will be automatically registered to Azure API Center and made available in the catalog.
