# Azure Setup Guide

This guide walks through setting up Azure API Center and configuring GitHub Actions integration.

## Prerequisites

- Azure subscription with appropriate permissions
- Azure CLI installed (`az cli`)
- GitHub repository admin access
- Owner or Contributor role on Azure subscription

## Part 1: Create Azure API Center

### Step 1: Login to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### Step 2: Create Resource Group (if needed)

```bash
az group create \
  --name "rg-mcp-registry" \
  --location "eastus"
```

### Step 3: Create API Center Instance

```bash
az apic create \
  --resource-group "rg-mcp-registry" \
  --name "apic-mcp-registry" \
  --location "eastus"
```

**Note**: API Center name must be globally unique. Choose a name specific to your organization.

### Step 4: Verify Creation

```bash
az apic show \
  --resource-group "rg-mcp-registry" \
  --name "apic-mcp-registry"
```

## Part 2: Configure Metadata Schema

Azure API Center supports built-in and custom metadata. Configure the metadata fields:

### Step 5: Create Custom Metadata (Optional)

For additional fields beyond built-in metadata:

```bash
# Example: Add custom "company" property
az apic metadata create \
  --resource-group "rg-mcp-registry" \
  --service-name "apic-mcp-registry" \
  --metadata-name "company" \
  --schema '{
    "type": "string",
    "title": "Company",
    "description": "Organization owning the API"
  }'

# Example: Add custom "owner" property
az apic metadata create \
  --resource-group "rg-mcp-registry" \
  --service-name "apic-mcp-registry" \
  --metadata-name "owner" \
  --schema '{
    "type": "string",
    "title": "Owner",
    "description": "Team or person responsible for the API"
  }'

# Example: Add custom "authMethod" property
az apic metadata create \
  --resource-group "rg-mcp-registry" \
  --service-name "apic-mcp-registry" \
  --metadata-name "authMethod" \
  --schema '{
    "type": "string",
    "title": "Authentication Method",
    "description": "Authentication method used by the API",
    "enum": ["none", "api-key", "oauth2", "entra-id"]
  }'
```

## Part 3: Set Up GitHub Actions Authentication

### Option A: Service Principal with Client Secret (Simpler)

#### Step 6a: Create Service Principal

```bash
az ad sp create-for-rbac \
  --name "sp-mcp-registry-github" \
  --role "Contributor" \
  --scopes /subscriptions/<subscription-id>/resourceGroups/rg-mcp-registry \
  --sdk-auth
```

**Output** (save this securely):
```json
{
  "clientId": "<client-id>",
  "clientSecret": "<client-secret>",
  "subscriptionId": "<subscription-id>",
  "tenantId": "<tenant-id>",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

### Option B: Federated Credentials with OIDC (Recommended for Production)

#### Step 6b: Create Service Principal

```bash
# Create the service principal
APP_ID=$(az ad sp create-for-rbac \
  --name "sp-mcp-registry-github" \
  --role "Contributor" \
  --scopes /subscriptions/<subscription-id>/resourceGroups/rg-mcp-registry \
  --query "appId" \
  --output tsv)

echo "Application ID: $APP_ID"
```

#### Step 7b: Configure Federated Identity Credential

Create a file `credential.json`:

```json
{
  "name": "github-actions-mcp-registry",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<your-github-org>/<your-repo>:ref:refs/heads/main",
  "description": "GitHub Actions for MCP Registry",
  "audiences": ["api://AzureADTokenExchange"]
}
```

Apply the credential:

```bash
az ad app federated-credential create \
  --id $APP_ID \
  --parameters credential.json
```

## Part 4: Configure GitHub Repository Secrets

### Step 8: Add Secrets to GitHub

Navigate to your GitHub repository → Settings → Secrets and variables → Actions → New repository secret

Add the following secrets:

| Secret Name | Value | Notes |
|-------------|-------|-------|
| `AZURE_CLIENT_ID` | `<client-id>` | From Step 6 |
| `AZURE_TENANT_ID` | `<tenant-id>` | From Step 6 |
| `AZURE_SUBSCRIPTION_ID` | `<subscription-id>` | Your Azure subscription |
| `AZURE_RESOURCE_GROUP` | `rg-mcp-registry` | Resource group name |
| `AZURE_API_CENTER_NAME` | `apic-mcp-registry` | API Center instance name |

**If using Option A (Client Secret)**:
| Secret Name | Value |
|-------------|-------|
| `AZURE_CLIENT_SECRET` | `<client-secret>` |

**If using Option B (OIDC)**: No client secret needed.

### Step 9: Update GitHub Actions Workflow (if using OIDC)

The workflow is already configured for OIDC. No changes needed if you followed Option B.

## Part 5: Deploy Self-Hosted API Center Portal

### Step 10: Clone Portal Repository

```bash
git clone https://github.com/Azure/APICenter-Portal-Starter.git
cd APICenter-Portal-Starter
```

### Step 11: Configure Portal Settings

Edit `appsettings.json`:

```json
{
  "AzureApiCenter": {
    "SubscriptionId": "<subscription-id>",
    "ResourceGroupName": "rg-mcp-registry",
    "ServiceName": "apic-mcp-registry"
  },
  "Authentication": {
    "Enabled": false
  }
}
```

**Note**: For production, enable Entra ID authentication.

### Step 12: Deploy to Azure App Service

```bash
# Create App Service Plan
az appservice plan create \
  --name "plan-mcp-portal" \
  --resource-group "rg-mcp-registry" \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name "app-mcp-portal-<unique-suffix>" \
  --resource-group "rg-mcp-registry" \
  --plan "plan-mcp-portal" \
  --runtime "DOTNETCORE:8.0"

# Deploy the portal
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
cd ..

az webapp deployment source config-zip \
  --resource-group "rg-mcp-registry" \
  --name "app-mcp-portal-<unique-suffix>" \
  --src deploy.zip
```

### Step 13: Configure Managed Identity (Recommended)

```bash
# Enable managed identity for the Web App
az webapp identity assign \
  --name "app-mcp-portal-<unique-suffix>" \
  --resource-group "rg-mcp-registry"

# Get the principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name "app-mcp-portal-<unique-suffix>" \
  --resource-group "rg-mcp-registry" \
  --query "principalId" \
  --output tsv)

# Grant the managed identity access to API Center
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "API Management Service Reader" \
  --scope /subscriptions/<subscription-id>/resourceGroups/rg-mcp-registry/providers/Microsoft.ApiCenter/services/apic-mcp-registry
```

### Step 14: Access the Portal

```bash
# Get the portal URL
az webapp show \
  --name "app-mcp-portal-<unique-suffix>" \
  --resource-group "rg-mcp-registry" \
  --query "defaultHostName" \
  --output tsv
```

Navigate to `https://<hostname>` to access your portal.

## Part 6: Test the Integration

### Step 15: Test GitHub Actions Manually

Trigger the workflow manually:

```bash
# Navigate to your repository
cd copilot-agent-mcp-registry

# Make a small change to trigger workflow
git checkout -b test-workflow
echo "# Test" >> apis/sample-data-mcp/README.md
git add .
git commit -m "Test workflow"
git push -u origin test-workflow

# Create and merge PR, or push to main directly (for testing)
```

### Step 16: Verify in Azure API Center

```bash
# List registered APIs
az apic api list \
  --resource-group "rg-mcp-registry" \
  --service-name "apic-mcp-registry" \
  --output table
```

### Step 17: Verify in Portal

Open the portal URL from Step 14 and verify the API appears in the list.

## Troubleshooting

### Issue: Service Principal lacks permissions

**Solution**: Grant additional roles:

```bash
az role assignment create \
  --assignee <client-id> \
  --role "API Management Service Contributor" \
  --scope /subscriptions/<subscription-id>/resourceGroups/rg-mcp-registry
```

### Issue: GitHub Actions authentication fails

**Solution**: Verify secrets are correctly set in GitHub and match Azure values.

### Issue: Portal shows "No APIs found"

**Solution**: 
1. Check API Center has APIs registered
2. Verify Managed Identity has correct permissions
3. Check portal configuration points to correct API Center

### Issue: API Center CLI commands fail

**Solution**: Install/update the API Center extension:

```bash
az extension add --name apic-extension
az extension update --name apic-extension
```

## Cost Considerations

- **Azure API Center**: Pay-as-you-go pricing based on API calls
- **App Service**: Basic tier (~$13/month) or Free tier for development
- **GitHub Actions**: 2,000 free minutes/month for private repos

## Security Best Practices

1. ✅ Use OIDC federated credentials (no secrets in GitHub)
2. ✅ Use Managed Identity for portal access to API Center
3. ✅ Enable Entra ID authentication for portal in production
4. ✅ Restrict Service Principal to minimum required scope
5. ✅ Regularly rotate client secrets (if using Option A)
6. ✅ Enable Azure API Center audit logging
7. ✅ Use separate resource groups for different environments

## Next Steps

1. ✅ Set up dev/staging/production environments
2. ✅ Configure branch protection rules in GitHub
3. ✅ Set up automated testing for OpenAPI specs
4. ✅ Enable monitoring and alerting
5. ✅ Document internal processes and runbooks

## Additional Resources

- [Azure API Center Documentation](https://learn.microsoft.com/azure/api-center/)
- [GitHub Actions OIDC](https://docs.github.com/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)
- [Self-hosted Portal Guide](https://learn.microsoft.com/azure/api-center/self-host-api-center-portal)

---

**Last Updated**: 2026-02-18  
**Tested With**: Azure CLI 2.57.0, .NET 10.0
