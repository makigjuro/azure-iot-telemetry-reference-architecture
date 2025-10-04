# Deployment Guide

Step-by-step guide to deploy the complete Azure IoT architecture.

---

## Prerequisites

- Azure subscription with Owner or Contributor access
- Terraform >= 1.5.0
- Azure CLI >= 2.50.0
- Azure CLI logged in: `az login`

See [Prerequisites Guide](../../PREREQUISITES.md) for detailed setup.

---

## Deployment Steps

### Step 1: Navigate to Dev Environment

```bash
cd infra/terraform/envs/dev
```

### Step 2: Create Configuration

```bash
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` and set your PostgreSQL password:
```hcl
location = "eastus"
postgres_admin_password = "YourSecurePassword123!"  # Change this!
```

**Password requirements:** 8-128 characters, include uppercase, lowercase, numbers, special characters

### Step 3: Initialize Terraform

```bash
terraform init
```

Expected output: `Terraform has been successfully initialized!`

### Step 4: Review Plan

```bash
terraform plan -out=tfplan
```

Review the plan:
- Resources to create: ~70-80 resources
- Estimated time: 20-30 minutes
- Verify all resource names and regions

### Step 5: Deploy

```bash
terraform apply tfplan
```

Deployment happens in layers:
1. Resource Group, Monitoring, Networking (2-3 min)
2. Key Vault, Storage Account (3-5 min)
3. IoT Hub, Event Hubs, Service Bus (5-7 min)
4. PostgreSQL, Digital Twins (5-10 min)
5. Stream Analytics (3-5 min)
6. Container Apps + 3 microservices (5-7 min)
7. RBAC assignments (1-2 min)

Monitor progress in another terminal:
```bash
watch -n 5 'az resource list --resource-group rg-iot-dev --output table'
```

### Step 6: Capture Outputs

```bash
terraform output > outputs.txt

# View key outputs
terraform output iothub_hostname
terraform output key_vault_name
terraform output storage_account_name
```

### Step 7: Verify Deployment

```bash
# Check resource group
az group show --name rg-iot-dev

# Check IoT Hub
az iot hub show --name $(terraform output -raw iothub_name)

# Check Container Apps
az containerapp list --resource-group rg-iot-dev --output table

# Check Key Vault secrets
az keyvault secret list --vault-name $(terraform output -raw key_vault_name) --output table
```

---

## Post-Deployment Validation

### Resource Count
```bash
az resource list --resource-group rg-iot-dev --query "length(@)"
# Expected: 70-80 resources
```

### IoT Hub Health
```bash
az iot hub show --name $(terraform output -raw iothub_name) \
  --query "{State:properties.state, HostName:properties.hostName}"
```

### Container Apps Status
```bash
az containerapp list \
  --resource-group rg-iot-dev \
  --query "[].{App:name, Status:properties.runningStatus}" \
  --output table
```

### Key Vault Secrets
```bash
az keyvault secret list \
  --vault-name $(terraform output -raw key_vault_name) \
  --query "[].name" \
  --output table
```

Expected secrets:
- `iothub-connection-string`
- `dps-connection-string`
- `eventhub-connection-string`
- `postgres-connection-string`
- `postgres-admin-password`
- `servicebus-connection-string`

---

## Success Criteria

Your deployment is successful if:

- `terraform apply` completes without errors
- ~70-80 resources created
- All Key Vault secrets populated
- IoT Hub state = "Active"
- Container Apps status = "Running" or "Idle"
- PostgreSQL state = "Ready"
- Stream Analytics state = "Running"

---

## Cleanup

### Destroy Everything
```bash
terraform destroy
```

Time: ~10-15 minutes

Destruction order (automatic):
1. RBAC assignments
2. Container Apps
3. Stream Analytics
4. Digital Twins, PostgreSQL
5. IoT Hub, Event Hubs
6. Storage, Key Vault
7. Networking, Monitoring
8. Resource Group

### Cost Optimization (Alternative)

Pause Stream Analytics only (~$60/mo savings):
```bash
az stream-analytics job stop \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name)
```

---

## Troubleshooting

### Terraform Init Fails
```bash
rm -rf .terraform .terraform.lock.hcl
terraform init
```

### Key Vault Access Denied
```bash
az role assignment create \
  --role "Key Vault Administrator" \
  --assignee $(az ad signed-in-user show --query id -o tsv) \
  --scope $(terraform output -raw key_vault_id)
```

### PostgreSQL Deployment Timeout
PostgreSQL can take 10-15 minutes. Check status:
```bash
az postgres flexible-server show \
  --resource-group rg-iot-dev \
  --name psql-iot-dev \
  --query "state"

# If "Ready", just re-run terraform apply
terraform apply
```

### Container Apps Image Pull Failure
Using placeholder images is OK for infrastructure testing. Real images will be built in Phase 2.

### Insufficient Quota
```bash
# Check quotas
az vm list-usage --location eastus --output table

# Request quota increase via Azure Portal (Support > New Support Request)
```

---

## Cost Management

### View Current Costs
Azure Portal: Cost Management + Billing > Cost Analysis

Filter by resource group: `rg-iot-dev`

### Set Budget Alert
Azure Portal: Cost Management + Billing > Budgets > Add

Recommended: Set alert at $200/mo

### Cost Optimization
1. Pause Stream Analytics when not testing (~$60/mo savings)
2. Container Apps scale to 0 when idle (already configured)
3. Lifecycle policies auto-delete old data (already configured)
4. Log Analytics daily cap at 1GB (already configured)

---

## Monitoring

### Application Insights
```bash
echo "https://portal.azure.com/#@/resource$(terraform output -raw application_insights_id)"
```

### Log Analytics
```bash
echo "https://portal.azure.com/#@/resource$(terraform output -raw log_analytics_workspace_id)"
```

**Sample KQL Queries:**

IoT Hub device connections (last 24h):
```kql
AzureDiagnostics
| where ResourceType == "IOTHUBS"
| where Category == "Connections"
| where TimeGenerated > ago(24h)
| summarize count() by OperationName
```

Container Apps logs:
```kql
ContainerAppConsoleLogs_CL
| where TimeGenerated > ago(1h)
| project TimeGenerated, ContainerAppName_s, Log_s
```

---

## What's Next?

After successful deployment:

1. Create test IoT device - See [Cookbook](../../COOKBOOK.md)
2. Send telemetry and view data flow
3. Test hot path (Stream Analytics) and cold path (Container Apps)
4. Build .NET 9 microservices (Phase 2)
5. Add Synapse pipelines (Phase 3)

---

## Support

- [Quick Start Guide](../../QUICK_START.md)
- [Cookbook Recipes](../../COOKBOOK.md)
- [Prerequisites](../../PREREQUISITES.md)
- Check Azure Activity Log for errors
- Service health: https://status.azure.com

---

**Deployment Time:** 20-30 minutes
**Monthly Cost:** ~$219/mo (~$138/mo with Stream Analytics paused)
**Resources Created:** ~70-80 Azure resources
