# Deployment Guide - Azure IoT Telemetry Reference Architecture

Complete step-by-step guide to deploy the full architecture to Azure.

---

## 📋 Prerequisites

### 1. **Azure Subscription**
- Active Azure subscription with Owner or Contributor access
- Subscription ID ready

### 2. **Tools Installed**
```bash
# Terraform
terraform --version  # Required: >= 1.5.0

# Azure CLI
az --version  # Required: >= 2.50.0

# Optional: jq for JSON parsing
jq --version
```

### 3. **Azure CLI Login**
```bash
az login
az account set --subscription "<your-subscription-id>"
az account show  # Verify correct subscription
```

---

## 🚀 Deployment Options

You have **3 deployment options** based on your needs:

### **Option A: Full Architecture** (Recommended)
Deploy all 11 modules - complete IoT platform
**Cost:** ~$219/mo
**Time:** ~20-30 minutes
**File:** `main-complete.tf`

### **Option B: Foundational Only** (Testing/Learning)
Deploy only networking, monitoring, storage, security
**Cost:** ~$96/mo
**Time:** ~10 minutes
**File:** `main.tf` (already exists)

### **Option C: Staged Deployment** (Safest)
Deploy in stages, test each layer
**Cost:** Gradual
**Time:** ~45 minutes

---

## 🎯 Option A: Full Architecture Deployment

### **Step 1: Prepare Configuration**

```bash
# Navigate to dev environment
cd infra/terraform/envs/dev

# Rename complete files (or create symlinks)
mv main.tf main-foundational.tf.backup
mv main-complete.tf main.tf
mv variables.tf variables-foundational.tf.backup
mv variables-complete.tf variables.tf

# Create terraform.tfvars from example
cp terraform.tfvars.example terraform.tfvars
```

### **Step 2: Configure Secrets**

Edit `terraform.tfvars`:
```hcl
location = "eastus"

# IMPORTANT: Change this password!
postgres_admin_username = "psqladmin"
postgres_admin_password = "YourSecurePassword123!"
```

**Password Requirements:**
- At least 8 characters
- Include uppercase, lowercase, numbers, special chars

### **Step 3: Initialize Terraform**

```bash
terraform init
```

Expected output:
```
Terraform has been successfully initialized!
```

### **Step 4: Review Deployment Plan**

```bash
terraform plan -out=tfplan
```

This will show:
- **Resources to create:** ~70-80 resources
- **Estimated time:** 20-30 minutes
- **No changes to existing resources** (first deployment)

Review carefully:
- ✅ All resource names follow naming convention
- ✅ All services in correct region
- ✅ No unexpected deletions

### **Step 5: Deploy Infrastructure**

```bash
terraform apply tfplan
```

**What happens:**
1. **Layer 1** (2-3 min): Resource Group, Monitoring, Networking
2. **Layer 2** (3-5 min): Key Vault, Storage Account
3. **Layer 3** (5-7 min): IoT Hub, Event Hubs, Service Bus
4. **Layer 4** (5-10 min): PostgreSQL, Digital Twins
5. **Layer 5** (3-5 min): Stream Analytics
6. **Layer 6** (5-7 min): Container Apps Environment + 3 apps
7. **Layer 7** (1-2 min): RBAC assignments

**Progress monitoring:**
```bash
# In another terminal, watch progress
watch -n 5 'az resource list --resource-group rg-iot-dev --output table'
```

### **Step 6: Capture Outputs**

```bash
terraform output > outputs.txt

# View important outputs
terraform output resource_group_name
terraform output iothub_hostname
terraform output key_vault_name
terraform output storage_account_name
```

### **Step 7: Verify Deployment**

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

## 🛡️ Option B: Foundational Only Deployment

**Cost:** ~$96/mo (no IoT services)

```bash
cd infra/terraform/envs/dev

# Use existing main.tf (foundational modules only)
terraform init
terraform plan
terraform apply
```

**What's included:**
- ✅ Virtual Network (FREE)
- ✅ Log Analytics (~$80/mo)
- ✅ ADLS Gen2 (~$15/mo)
- ✅ Key Vault (~$1/mo)



## 🎬 Option C: Staged Deployment

Deploy incrementally to understand each layer.

### **Stage 1: Foundation** (10 min, ~$96/mo)
```bash
# Deploy foundational modules only
terraform apply \
  -target=azurerm_resource_group.main \
  -target=module.monitoring \
  -target=module.networking \
  -target=module.storage \
  -target=module.security
```

**Test:**
```bash
# Upload a test file to ADLS
az storage blob upload \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name raw \
  --name test.txt \
  --file /dev/null
```

### **Stage 2: IoT Services** (10 min, +$25/mo)
```bash
terraform apply \
  -target=module.event_streaming \
  -target=module.iot_hub
```

**Test:**
```bash
# Create a test device
az iot hub device-identity create \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id test-device-001
```

### **Stage 3: Data & Analytics** (10 min, +$93/mo)
```bash
terraform apply \
  -target=module.database \
  -target=module.digital_twins \
  -target=module.stream_analytics
```

**Test:**
```bash
# Check PostgreSQL connectivity
az postgres flexible-server show \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw postgres_server_name)
```

### **Stage 4: Compute & RBAC** (10 min, +$5/mo)
```bash
terraform apply \
  -target=module.container_apps \
  -target=module.rbac
```

**Test:**
```bash
# Check container apps status
az containerapp list \
  --resource-group rg-iot-dev \
  --query "[].{Name:name, Status:properties.runningStatus}" \
  --output table
```

---

## ✅ Post-Deployment Validation

### **1. Resource Count**
```bash
az resource list --resource-group rg-iot-dev --query "length(@)"
# Expected: 70-80 resources
```

### **2. IoT Hub Health**
```bash
az iot hub show --name $(terraform output -raw iothub_name) \
  --query "{State:properties.state, HostName:properties.hostName}"
```

### **3. Event Hub Metrics**
```bash
az eventhubs namespace show \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw eventhub_namespace_name) \
  --query "status"
```

### **4. Container Apps Readiness**
```bash
az containerapp list \
  --resource-group rg-iot-dev \
  --query "[].{App:name, Replicas:properties.template.scale.minReplicas, Status:properties.runningStatus}" \
  --output table
```

### **5. Key Vault Secrets**
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

## 🧹 Cleanup / Destroy

### **Destroy Everything**
```bash
terraform destroy
```

**Destruction order** (automatic):
1. RBAC assignments
2. Container Apps
3. Stream Analytics
4. Digital Twins, PostgreSQL
5. IoT Hub, Event Hubs
6. Storage, Key Vault
7. Networking, Monitoring
8. Resource Group

**Time:** ~10-15 minutes

### **Selective Destruction** (Cost Optimization)

**Pause Stream Analytics only** (~$60/mo savings):
```bash
# Stop the job (via Azure Portal or CLI)
az stream-analytics job stop \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name)
```

**Keep foundation, remove compute**:
```bash
terraform destroy \
  -target=module.container_apps \
  -target=module.stream_analytics
```

---

## 🐛 Troubleshooting

### **Issue: Terraform Init Fails**
```bash
# Clear cache
rm -rf .terraform .terraform.lock.hcl
terraform init
```

### **Issue: Key Vault Access Denied**
```bash
# Grant yourself Key Vault Administrator role
az role assignment create \
  --role "Key Vault Administrator" \
  --assignee $(az ad signed-in-user show --query id -o tsv) \
  --scope $(terraform output -raw key_vault_id)
```

### **Issue: PostgreSQL Deployment Timeout**
PostgreSQL can take 10-15 minutes. If timeout occurs:
```bash
# Check status
az postgres flexible-server show \
  --resource-group rg-iot-dev \
  --name psql-iot-dev \
  --query "state"

# If "Ready", just re-run terraform apply
terraform apply
```

### **Issue: Container Apps Image Pull Failure**
Using placeholder images is OK for infrastructure testing. Real images will be built in Phase 2.

### **Issue: Insufficient Quota**
```bash
# Check quotas
az vm list-usage --location eastus --output table

# Request quota increase via Azure Portal
```

---

## 💰 Cost Management

### **View Current Costs**
```bash
# Via Azure CLI
az consumption usage list \
  --start-date 2025-01-01 \
  --end-date 2025-01-31 \
  --query "[?contains(instanceName, 'iot-dev')]"
```

### **Set Budget Alert**
```bash
# Create budget (via Portal recommended)
# Cost Management + Billing > Budgets > Add
# Set alert at $200/mo
```

### **Cost Optimization Tips**
1. ⏸️ **Pause Stream Analytics** when not testing (~$60/mo savings)
2. 🗑️ **Enable lifecycle policies** on storage (auto-delete old data)
3. 📉 **Set Container Apps to scale to 0** (already configured)
4. 🔍 **Review Log Analytics ingestion** (1GB/day cap configured)

---

## 📊 Monitoring & Observability

### **Application Insights**
```bash
# Get Application Insights URL
echo "https://portal.azure.com/#@/resource$(terraform output -raw application_insights_id)"
```

### **Log Analytics Queries**
```bash
# Get Log Analytics workspace URL
echo "https://portal.azure.com/#@/resource$(terraform output -raw log_analytics_workspace_id)"
```

**Sample KQL Queries:**
```kql
// IoT Hub device connections (last 24h)
AzureDiagnostics
| where ResourceType == "IOTHUBS"
| where Category == "Connections"
| where TimeGenerated > ago(24h)
| summarize count() by OperationName

// Container Apps logs
ContainerAppConsoleLogs_CL
| where TimeGenerated > ago(1h)
| project TimeGenerated, ContainerAppName_s, Log_s
```

---

## 🎉 Success Criteria

Your deployment is successful if:

✅ `terraform apply` completes without errors
✅ ~70-80 resources created
✅ All Key Vault secrets populated
✅ IoT Hub state = "Active"
✅ Container Apps status = "Running" or "Idle"
✅ PostgreSQL state = "Ready"
✅ Stream Analytics state = "Running"
✅ Log Analytics receiving data

---

## 🚀 What's Next?

After successful deployment:

1. **Phase 2: Application Code** - Build .NET 9 microservices
2. **Test Workflow 1** - Device telemetry ingestion (hot + cold path)
3. **Test Workflow 2** - Device provisioning and command/control
4. **Configure dashboards** - Power BI, Grafana
5. **Add Synapse pipelines** - Bronze → Silver → Gold transformations

---

## 📞 Support

If you encounter issues:
1. Check `terraform.log` for detailed errors
2. Review Azure Activity Log in Portal
3. Check service health: https://status.azure.com
4. Review module documentation in `infra/terraform/modules/*/`

---

**Estimated Total Deployment Time:** 20-30 minutes
**Estimated Monthly Cost:** $219/mo (full architecture)
**Estimated Monthly Cost (optimized):** $138/mo (Stream Analytics paused)
