# Prerequisites

Complete requirements for deploying the Azure IoT Telemetry Reference Architecture.

---

## 1. Azure Subscription

### Required Access
- **Azure Subscription** with one of:
  - Owner role (recommended)
  - Contributor + User Access Administrator roles
  - Custom role with permissions for resource creation and RBAC assignments

### Verify Access
```bash
az login
az account show
az role assignment list --assignee $(az ad signed-in-user show --query id -o tsv) --scope /subscriptions/$(az account show --query id -o tsv)
```

### Required Quotas
Ensure sufficient quotas for:
- **vCPUs:** 4+ (for PostgreSQL, Container Apps)
- **Public IP addresses:** 2+
- **Virtual networks:** 1
- **Storage accounts:** 1

Check quotas:
```bash
az vm list-usage --location eastus --output table
```

---

## 2. Development Tools

### Required Tools

#### Terraform
- **Version:** >= 1.5.0
- **Installation:**
  ```bash
  # macOS
  brew install terraform

  # Linux
  wget https://releases.hashicorp.com/terraform/1.6.0/terraform_1.6.0_linux_amd64.zip
  unzip terraform_1.6.0_linux_amd64.zip
  sudo mv terraform /usr/local/bin/

  # Windows (Chocolatey)
  choco install terraform

  # Verify
  terraform --version
  ```

#### Azure CLI
- **Version:** >= 2.50.0
- **Installation:**
  ```bash
  # macOS
  brew install azure-cli

  # Linux
  curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

  # Windows
  # Download from: https://aka.ms/installazurecliwindows

  # Verify
  az --version
  ```

#### Git (Optional)
```bash
git --version
```

### Optional Tools

#### jq (JSON processor)
```bash
# macOS
brew install jq

# Linux
sudo apt-get install jq

# Windows
choco install jq
```

#### Azure CLI IoT Extension
```bash
az extension add --name azure-iot
az extension update --name azure-iot
```

---

## 3. Azure CLI Configuration

### Login
```bash
az login

# If you have multiple tenants
az login --tenant <tenant-id>
```

### Select Subscription
```bash
# List subscriptions
az account list --output table

# Set active subscription
az account set --subscription "<subscription-name-or-id>"

# Verify
az account show
```

### Configure Defaults (Optional)
```bash
az configure --defaults location=eastus
az configure --defaults group=rg-iot-dev
```

---

## 4. Network Access

### Firewall & Proxy
Ensure outbound HTTPS (443) access to:
- `*.azure.com`
- `*.microsoft.com`
- `*.azurecr.io` (for Container Registry)
- `*.azurewebsites.net`
- `releases.hashicorp.com` (for Terraform)

### Test Connectivity
```bash
curl -I https://portal.azure.com
curl -I https://management.azure.com
```

---

## 5. Permissions & RBAC

### Service Principal (Optional - for CI/CD)
If using automated deployments, create a service principal:

```bash
# Create service principal
az ad sp create-for-rbac \
  --name "sp-terraform-iot" \
  --role Contributor \
  --scopes /subscriptions/<subscription-id>

# Save output (clientId, clientSecret, tenantId)

# Test login
az login --service-principal \
  --username <clientId> \
  --password <clientSecret> \
  --tenant <tenantId>
```

### Required Azure Permissions
The deployment creates:
- Resource groups
- Virtual networks and subnets
- Storage accounts
- Key Vault instances
- IoT Hub and DPS
- Event Hubs and Event Grid
- Stream Analytics jobs
- PostgreSQL servers
- Azure Digital Twins
- Container Apps
- Role assignments (RBAC)

---

## 6. Configuration Requirements

### Secrets Management

#### PostgreSQL Password
Must meet Azure requirements:
- **Length:** 8-128 characters
- **Include:** Uppercase, lowercase, numbers, special characters
- **Exclude:** `@`, `#`, `$` in username

#### Generate Secure Password
```bash
# Linux/macOS
openssl rand -base64 16

# PowerShell
-join ((65..90) + (97..122) + (48..57) + (33,35,37,38,42) | Get-Random -Count 16 | % {[char]$_})
```

### Resource Naming
Default naming convention: `{resource-type}-{project}-{environment}`
- Example: `iot-iot-dev`, `psql-iot-dev`

Ensure names are:
- **Globally unique** (for Storage, IoT Hub)
- **Lowercase with hyphens** (for most resources)
- **No special characters** (for Storage Accounts)

---

## 7. Cost Considerations

### Minimum Azure Credits/Budget
- **Dev/Test:** $200-250/month
- **Production:** $500-1000/month

### Cost Alerts
Set up budget alerts before deployment:
```bash
# Via Azure Portal
# Cost Management + Billing > Budgets > Add

# Or via CLI (requires cost management permissions)
az consumption budget create \
  --budget-name "IoT-Dev-Budget" \
  --amount 250 \
  --category Cost \
  --time-grain Monthly \
  --start-date 2025-01-01 \
  --end-date 2025-12-31
```

---

## 8. Terraform Backend (Optional)

For team collaboration, configure remote state:

```bash
# Create storage for Terraform state
az group create --name rg-terraform-state --location eastus

az storage account create \
  --name sttfstateiot \
  --resource-group rg-terraform-state \
  --location eastus \
  --sku Standard_LRS

az storage container create \
  --name tfstate \
  --account-name sttfstateiot
```

Update `envs/dev/provider.tf`:
```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "sttfstateiot"
    container_name       = "tfstate"
    key                  = "dev.terraform.tfstate"
  }
}
```

---

## 9. Pre-Deployment Checklist

Before running `terraform apply`, verify:

- [ ] Azure CLI logged in
- [ ] Correct subscription selected
- [ ] Terraform >= 1.5.0 installed
- [ ] Network connectivity to Azure
- [ ] Sufficient quotas available
- [ ] PostgreSQL password generated
- [ ] `terraform.tfvars` configured
- [ ] Budget alerts configured (optional)
- [ ] Terraform backend configured (optional)

### Verification Script
```bash
#!/bin/bash
echo "=== Prerequisites Check ==="

# Check Terraform
if command -v terraform &> /dev/null; then
    echo "✅ Terraform: $(terraform --version | head -1)"
else
    echo "❌ Terraform not found"
fi

# Check Azure CLI
if command -v az &> /dev/null; then
    echo "✅ Azure CLI: $(az --version | head -1)"
else
    echo "❌ Azure CLI not found"
fi

# Check Azure login
if az account show &> /dev/null; then
    echo "✅ Azure: Logged in as $(az account show --query user.name -o tsv)"
    echo "   Subscription: $(az account show --query name -o tsv)"
else
    echo "❌ Azure: Not logged in"
fi

# Check terraform.tfvars
if [ -f terraform.tfvars ]; then
    echo "✅ terraform.tfvars exists"
else
    echo "⚠️  terraform.tfvars not found (copy from terraform.tfvars.example)"
fi

echo ""
echo "=== Ready to Deploy ==="
```

---

## 10. Common Issues & Solutions

### Issue: Insufficient Permissions
**Solution:** Request Owner or Contributor + User Access Administrator roles

### Issue: Quota Exceeded
**Solution:** Request quota increase via Azure Portal (Support > New Support Request)

### Issue: Region Not Available
**Solution:** Change `location` variable to: `eastus`, `westus2`, or `northeurope`

### Issue: Terraform State Locked
**Solution:**
```bash
# If using remote backend
az storage blob lease break --container-name tfstate --blob-name dev.terraform.tfstate --account-name sttfstateiot
```

---

## Next Steps

Once all prerequisites are met:

1. ✅ Continue to **[Quick Start Guide](QUICK_START.md)**
2. ✅ Or see **[Cookbook](COOKBOOK.md)** for step-by-step recipes
3. ✅ Or read **[Deployment Guide](envs/dev/DEPLOYMENT_GUIDE.md)** for detailed options
