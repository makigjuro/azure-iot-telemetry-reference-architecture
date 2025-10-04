# Prerequisites

Requirements for deploying the Azure IoT Telemetry Reference Architecture.

---

## Required

### 1. Azure Subscription
- Owner or Contributor role
- Minimum budget: $200-250/month for dev environment

### 2. Tools

**Terraform** (>= 1.5.0)
```bash
# macOS
brew install terraform

# Linux
wget https://releases.hashicorp.com/terraform/1.6.0/terraform_1.6.0_linux_amd64.zip
unzip terraform_1.6.0_linux_amd64.zip && sudo mv terraform /usr/local/bin/

# Verify
terraform --version
```

**Azure CLI** (>= 2.50.0)
```bash
# macOS
brew install azure-cli

# Linux
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Verify
az --version
```

### 3. Azure Login

```bash
az login
az account set --subscription "<your-subscription-id>"
az account show  # Verify
```

### 4. PostgreSQL Password

Generate a secure password (8-128 characters, include uppercase, lowercase, numbers, special characters):

```bash
# Linux/macOS
openssl rand -base64 16

# PowerShell
-join ((65..90) + (97..122) + (48..57) + (33,35,37,38,42) | Get-Random -Count 16 | % {[char]$_})
```

---

## Quick Check

Run this to verify you're ready:

```bash
# Check tools
terraform --version  # Should be >= 1.5.0
az --version         # Should be >= 2.50.0

# Check Azure access
az account show

# Check you're in the right directory
ls terraform.tfvars.example  # Should exist in envs/dev/
```

---

## Common Issues

**Insufficient permissions**
→ Request Owner or Contributor + User Access Administrator roles

**Quota exceeded**
→ Request quota increase in Azure Portal (Support > New Support Request)

**Region not available**
→ Change `location` in terraform.tfvars to `eastus`, `westus2`, or `northeurope`

---

## Next Steps

1. [Quick Start Guide](QUICK_START.md) - Deploy in ~30 minutes
2. [Cookbook](COOKBOOK.md) - Step-by-step recipes
3. [Deployment Guide](envs/dev/DEPLOYMENT_GUIDE.md) - Detailed options
