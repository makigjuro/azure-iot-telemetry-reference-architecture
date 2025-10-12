# 🚀 Quick Start - Deploy Full Architecture

## Prerequisites
- ✅ Azure subscription with Owner/Contributor access
- ✅ Azure CLI installed and logged in
- ✅ Terraform >= 1.5.0 installed

## Deploy in 5 Steps

### **1. Navigate to Dev Environment**
```bash
cd infra/terraform/envs/dev
```

### **2. Create Configuration File**
```bash
cp terraform.tfvars.example terraform.tfvars
```

### **3. Configure Variables**
Edit `terraform.tfvars` and change these values:
```hcl
unique_suffix = "mg123"  # Change to something unique (3-8 chars, lowercase)
postgres_admin_password = "YourSecurePassword123!"  # Change this!
```

**Requirements:**
- Unique suffix: 3-8 characters, lowercase letters and numbers only
- Password: Minimum 8 characters, include uppercase, lowercase, numbers, special chars

### **4. Deploy**
```bash
# Initialize Terraform
terraform init

# Review what will be created
terraform plan

# Deploy (20-30 minutes)
terraform apply
```

### **5. Verify Deployment**
```bash
# View all resources
az resource list --resource-group rg-iot-dev --output table

# Get key outputs
terraform output iothub_hostname
terraform output key_vault_name
terraform output storage_account_name
```

---

## What Gets Deployed

✅ **IoT Hub + DPS** - Device connectivity (~$10/mo)
✅ **Event Hubs + Event Grid** - Event streaming (~$15/mo)
✅ **Stream Analytics** - Real-time processing (~$81/mo)
✅ **PostgreSQL** - Device metadata (~$12/mo)
✅ **Digital Twins** - Device models (~$5/mo)
✅ **3 Container Apps** - Microservices (~$3/mo)
✅ **ADLS Gen2** - Data lake (~$15/mo)
✅ **Key Vault + Monitoring** - Security & logs (~$80/mo)
✅ **VNet + RBAC** - Network & permissions (FREE)

**Total:** ~70-80 resources
**Cost:** ~$219/mo (~$138/mo with Stream Analytics paused)
**Time:** 20-30 minutes

---

## Quick Commands

```bash
# View all outputs
terraform output

# Create test IoT device
az iot hub device-identity create \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id test-device-001

# Check Container Apps status
az containerapp list \
  --resource-group rg-iot-dev \
  --output table

# View Key Vault secrets
az keyvault secret list \
  --vault-name $(terraform output -raw key_vault_name) \
  --output table

# Pause Stream Analytics (save $60/mo)
az stream-analytics job stop \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name)

# Destroy everything
terraform destroy
```

---

## Architecture Overview

```
IoT Devices (MQTT/AMQP)
    ↓
IoT Hub + DPS
    ↓
Event Hubs
    ├─→ Stream Analytics (Hot Path) → ADLS + Alerts
    └─→ Container App (Cold Path) → ADLS (bronze/silver/gold)
            ↓
        Synapse Analytics (Phase 3)
            ↓
        Microsoft Fabric (Phase 3)
```

**Device Lifecycle:**
```
DPS → IoT Hub → Event Grid → Container App → PostgreSQL + Digital Twins
```

---

## Cost Optimization Tips

1. **⏸️ Pause Stream Analytics** when not testing (~$60/mo savings)
   ```bash
   az stream-analytics job stop --resource-group rg-iot-dev --name asa-iot-dev
   ```

2. **📉 Scale Container Apps to 0** (already configured)
   - Apps automatically scale to 0 when idle

3. **🗑️ Enable Lifecycle Policies** (already configured)
   - Data auto-archives after 30 days
   - Data auto-deletes after 365 days

4. **🔍 Monitor Costs**
   - Check Azure Cost Management daily
   - Set budget alerts at $200/mo

---

## Troubleshooting

### **Issue: PostgreSQL Takes Too Long**
PostgreSQL can take 10-15 minutes. Check status:
```bash
az postgres flexible-server show \
  --resource-group rg-iot-dev \
  --name psql-iot-dev \
  --query "state"
```

### **Issue: Key Vault Access Denied**
Grant yourself access:
```bash
az role assignment create \
  --role "Key Vault Administrator" \
  --assignee $(az ad signed-in-user show --query id -o tsv) \
  --scope $(terraform output -raw key_vault_id)
```

### **Issue: Terraform Init Fails**
Clear cache and retry:
```bash
rm -rf .terraform .terraform.lock.hcl
terraform init
```

---

## Need Help?

- [Deployment Guide](envs/dev/DEPLOYMENT_GUIDE.md) - Detailed deployment options and troubleshooting
- [Cookbook](COOKBOOK.md) - Step-by-step recipes for common tasks
- [Terraform Overview](README.md) - Module documentation and cost details
- [Prerequisites](PREREQUISITES.md) - Setup requirements
