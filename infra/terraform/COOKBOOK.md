# 📖 Cookbook - Step-by-Step Recipes

Practical recipes for deploying and using the Azure IoT Telemetry Reference Architecture.

---

## 🍳 Recipe Index

1. [Quick 5-Minute Deployment](#recipe-1-quick-5-minute-deployment)
2. [Staged Layer-by-Layer Deployment](#recipe-2-staged-layer-by-layer-deployment)
3. [Create Your First IoT Device](#recipe-3-create-your-first-iot-device)
4. [Send Telemetry and View Data Flow](#recipe-4-send-telemetry-and-view-data-flow)
5. [Optimize Costs](#recipe-5-optimize-costs)
6. [View Logs and Metrics](#recipe-6-view-logs-and-metrics)
7. [Test Hot Path (Real-Time Alerts)](#recipe-7-test-hot-path-real-time-alerts)
8. [Test Cold Path (Batch Processing)](#recipe-8-test-cold-path-batch-processing)
9. [Clean Up Resources](#recipe-9-clean-up-resources)
10. [Troubleshoot Common Issues](#recipe-10-troubleshoot-common-issues)

---

## Recipe 1: Quick 5-Minute Deployment

**Goal:** Deploy the full architecture as fast as possible

**Ingredients:**
- Azure subscription with Owner/Contributor access
- Azure CLI installed and logged in (`az login`)
- Terraform >= 1.5.0 installed

**Steps:**

```bash
# 1. Navigate to dev environment
cd infra/terraform/envs/dev

# 2. Create config file
cp terraform.tfvars.example terraform.tfvars

# 3. Set PostgreSQL password (use your own secure password!)
echo 'postgres_admin_password = "MySecure123Pass!"' >> terraform.tfvars

# 4. Initialize and deploy
terraform init
terraform apply -auto-approve

# 5. Save outputs
terraform output > outputs.txt
```

**Expected Result:**
- 70-80 resources created
- Time: 20-30 minutes
- Cost: ~$219/mo

**Verify Success:**
```bash
az resource list --resource-group rg-iot-dev --output table | wc -l
# Should show ~70-80 resources
```

---

## Recipe 2: Staged Layer-by-Layer Deployment

**Goal:** Deploy incrementally to understand each layer

**Why:** Safer, easier to debug, better understanding of architecture

### Stage 1: Foundation (10 min, ~$96/mo)

```bash
cd infra/terraform/envs/dev

# Copy config
cp terraform.tfvars.example terraform.tfvars
vim terraform.tfvars  # Set postgres_admin_password

# Initialize
terraform init

# Deploy foundation only
terraform apply \
  -target=azurerm_resource_group.main \
  -target=module.monitoring \
  -target=module.networking \
  -target=module.storage \
  -target=module.security
```

**Test Foundation:**
```bash
# Upload test file to ADLS
az storage blob upload \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name raw \
  --name test.txt \
  --file README.md \
  --auth-mode login
```

### Stage 2: IoT Services (10 min, +$25/mo)

```bash
terraform apply \
  -target=module.event_streaming \
  -target=module.iot_hub
```

**Test IoT Services:**
```bash
# Create test device
az iot hub device-identity create \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id test-device-001

# Get connection string
az iot hub device-identity connection-string show \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id test-device-001
```

### Stage 3: Data Services (15 min, +$93/mo)

```bash
terraform apply \
  -target=module.database \
  -target=module.digital_twins \
  -target=module.stream_analytics
```

**Test Database:**
```bash
# Check PostgreSQL status
az postgres flexible-server show \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw postgres_server_name) \
  --query "state"
```

### Stage 4: Compute & RBAC (5 min, +$5/mo)

```bash
terraform apply \
  -target=module.container_apps \
  -target=module.rbac
```

**Test Container Apps:**
```bash
az containerapp list \
  --resource-group rg-iot-dev \
  --query "[].{Name:name, Status:properties.runningStatus}" \
  --output table
```

---

## Recipe 3: Create Your First IoT Device

**Goal:** Register and provision an IoT device

### Option A: Manual Device Registration

```bash
# 1. Create device identity
az iot hub device-identity create \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id sensor-001

# 2. Get connection string
az iot hub device-identity connection-string show \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id sensor-001 \
  --query "connectionString" -o tsv

# 3. List all devices
az iot hub device-identity list \
  --hub-name $(terraform output -raw iothub_name) \
  --output table
```

### Option B: Auto-Provisioning via DPS

```bash
# 1. Get DPS ID Scope
DPS_SCOPE=$(terraform output -raw dps_id_scope)
echo "DPS ID Scope: $DPS_SCOPE"

# 2. Create enrollment group (for multiple devices)
az iot dps enrollment-group create \
  --dps-name $(terraform output -raw dps_name) \
  --resource-group rg-iot-dev \
  --enrollment-id "sensor-group" \
  --attestation-type symmetrickey

# 3. Get group keys
az iot dps enrollment-group show \
  --dps-name $(terraform output -raw dps_name) \
  --resource-group rg-iot-dev \
  --enrollment-id "sensor-group" \
  --show-keys
```

**Expected Result:**
- Device registered in IoT Hub
- Device provisioned via DPS (Option B)
- Event Grid triggers Event Subscriber Container App
- Device record created in PostgreSQL + Digital Twins

---

## Recipe 4: Send Telemetry and View Data Flow

**Goal:** Send device telemetry and trace it through the pipeline

### Step 1: Install Azure IoT Extension

```bash
az extension add --name azure-iot
az extension update --name azure-iot
```

### Step 2: Send Test Telemetry

```bash
# Send single message
az iot device send-d2c-message \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id sensor-001 \
  --data '{"temperature": 72.5, "humidity": 45.2, "timestamp": "2025-01-15T10:30:00Z"}'

# Send high temperature alert
az iot device send-d2c-message \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id sensor-001 \
  --data '{"temperature": 85.0, "humidity": 50.0, "timestamp": "2025-01-15T10:35:00Z"}'
```

### Step 3: Monitor IoT Hub

```bash
# Monitor device-to-cloud messages (real-time)
az iot hub monitor-events \
  --hub-name $(terraform output -raw iothub_name) \
  --device-id sensor-001 \
  --timeout 60

# View IoT Hub metrics
az monitor metrics list \
  --resource $(terraform output -raw iothub_id) \
  --metric "d2c.telemetry.ingress.success" \
  --interval PT5M
```

### Step 4: Check Event Hubs

```bash
# Check Event Hub metrics
az eventhubs eventhub show \
  --resource-group rg-iot-dev \
  --namespace-name $(terraform output -raw eventhub_namespace_name) \
  --name $(terraform output -raw eventhub_telemetry_name) \
  --query "{Name:name, PartitionCount:partitionCount, MessageRetentionInDays:messageRetentionInDays}"
```

### Step 5: Check ADLS for Processed Data

```bash
# List files in hotpath container (Stream Analytics output)
az storage blob list \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name hotpath \
  --auth-mode login \
  --output table

# List files in bronze container (Telemetry Processor output)
az storage blob list \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name bronze \
  --auth-mode login \
  --output table
```

### Step 6: Check Logs

```bash
# View Container App logs (Telemetry Processor)
az containerapp logs show \
  --name ca-telemetry-processor-iot-dev \
  --resource-group rg-iot-dev \
  --follow \
  --tail 50
```

**Expected Data Flow:**
```
Device → IoT Hub → Event Hubs → Stream Analytics → ADLS (hotpath/)
                               ↘ Container App → ADLS (bronze/)
```

---

## Recipe 5: Optimize Costs

**Goal:** Reduce monthly costs from $219 to ~$138

### Cost Reduction #1: Pause Stream Analytics (~$60/mo savings)

```bash
# Stop Stream Analytics job
az stream-analytics job stop \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name)

# Verify status
az stream-analytics job show \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name) \
  --query "jobState"
```

**When to restart:**
```bash
az stream-analytics job start \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name) \
  --output-start-mode JobStartTime
```

### Cost Reduction #2: Scale Container Apps to 0

```bash
# Check current scale settings
az containerapp show \
  --name ca-telemetry-processor-iot-dev \
  --resource-group rg-iot-dev \
  --query "properties.template.scale.{MinReplicas:minReplicas, MaxReplicas:maxReplicas}"

# Already configured to scale to 0 when idle!
# No action needed - Container Apps automatically scale to 0
```

### Cost Reduction #3: Review Log Analytics Ingestion

```bash
# Check Log Analytics usage
az monitor log-analytics workspace show \
  --resource-group rg-iot-dev \
  --workspace-name $(terraform output -raw log_analytics_workspace_name) \
  --query "{DailyQuotaGB:workspaceCapping.dailyQuotaGb, RetentionDays:retentionInDays}"

# View ingestion last 7 days
az monitor log-analytics workspace data-export list \
  --resource-group rg-iot-dev \
  --workspace-name $(terraform output -raw log_analytics_workspace_name)
```

### Cost Reduction #4: Delete Unused Resources

```bash
# Destroy only specific modules (keep foundation)
terraform destroy \
  -target=module.stream_analytics \
  -target=module.container_apps
```

### View Current Costs

```bash
# View cost analysis (via Portal)
open "https://portal.azure.com/#view/Microsoft_Azure_CostManagement/Menu/~/costanalysis/scope/%2Fsubscriptions%2F$(az account show --query id -o tsv)%2FresourceGroups%2Frg-iot-dev"
```

**Cost Summary:**
| Action | Monthly Savings |
|--------|-----------------|
| Pause Stream Analytics | ~$60 |
| Scale Container Apps to 0 | Already configured |
| 1GB/day Log Analytics cap | Already configured |
| Lifecycle policies on ADLS | Already configured |
| **Total Optimized Cost** | **~$138/mo** |

---

## Recipe 6: View Logs and Metrics

**Goal:** Monitor platform health and troubleshoot issues

### Log Analytics Queries

```bash
# Get Log Analytics Workspace URL
echo "https://portal.azure.com/#@/resource$(terraform output -raw log_analytics_workspace_id)/logs"
```

**Sample KQL Queries:**

**Query 1: IoT Hub Connections (Last 24h)**
```kql
AzureDiagnostics
| where ResourceType == "IOTHUBS"
| where Category == "Connections"
| where TimeGenerated > ago(24h)
| summarize count() by OperationName, bin(TimeGenerated, 1h)
| render timechart
```

**Query 2: Container App Logs**
```kql
ContainerAppConsoleLogs_CL
| where TimeGenerated > ago(1h)
| where ContainerAppName_s contains "telemetry-processor"
| project TimeGenerated, Log_s
| order by TimeGenerated desc
```

**Query 3: Event Hub Metrics**
```kql
AzureMetrics
| where ResourceProvider == "MICROSOFT.EVENTHUB"
| where TimeGenerated > ago(1h)
| summarize avg(IncomingMessages_Average) by bin(TimeGenerated, 5m)
| render timechart
```

**Query 4: High Temperature Alerts**
```kql
AzureDiagnostics
| where ResourceType == "STREAMANALYTICS"
| where Category == "Execution"
| where Message contains "temperature"
| project TimeGenerated, Message
```

### Application Insights

```bash
# Get Application Insights URL
echo "https://portal.azure.com/#@/resource$(terraform output -raw application_insights_id)"
```

**View:**
- Live Metrics Stream
- Failures (exceptions)
- Performance (response times)
- Application Map (dependencies)

### Container Apps Logs (CLI)

```bash
# Follow logs in real-time
az containerapp logs show \
  --name ca-telemetry-processor-iot-dev \
  --resource-group rg-iot-dev \
  --follow

# View last 100 lines
az containerapp logs show \
  --name ca-alert-handler-iot-dev \
  --resource-group rg-iot-dev \
  --tail 100
```

### IoT Hub Monitoring

```bash
# Monitor device events (real-time)
az iot hub monitor-events \
  --hub-name $(terraform output -raw iothub_name)

# View IoT Hub diagnostics
az monitor diagnostic-settings list \
  --resource $(terraform output -raw iothub_id)
```

---

## Recipe 7: Test Hot Path (Real-Time Alerts)

**Goal:** Verify real-time alert processing via Stream Analytics

### Step 1: Ensure Stream Analytics is Running

```bash
# Start Stream Analytics
az stream-analytics job start \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name) \
  --output-start-mode JobStartTime

# Check status
az stream-analytics job show \
  --resource-group rg-iot-dev \
  --name $(terraform output -raw stream_analytics_job_name) \
  --query "jobState"
```

### Step 2: Send High Temperature Telemetry

```bash
# Send temperature above threshold (75°C)
for i in {1..10}; do
  az iot device send-d2c-message \
    --hub-name $(terraform output -raw iothub_name) \
    --device-id sensor-001 \
    --data "{\"temperature\": 85.5, \"humidity\": 60.0, \"timestamp\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}"
  echo "Sent message $i"
  sleep 2
done
```

### Step 3: Check Service Bus Queue for Alerts

```bash
# Get message count
az servicebus queue show \
  --resource-group rg-iot-dev \
  --namespace-name $(terraform output -raw servicebus_namespace_name) \
  --name stream-alerts \
  --query "countDetails.activeMessageCount"

# Peek messages (non-destructive)
az servicebus queue message peek \
  --resource-group rg-iot-dev \
  --namespace-name $(terraform output -raw servicebus_namespace_name) \
  --queue-name stream-alerts \
  --max-count 5
```

### Step 4: Check Alert Handler Container App

```bash
# View logs
az containerapp logs show \
  --name ca-alert-handler-iot-dev \
  --resource-group rg-iot-dev \
  --follow
```

### Step 5: Verify ADLS Output

```bash
# List hotpath blobs
az storage blob list \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name hotpath \
  --auth-mode login \
  --query "[].{Name:name, Size:properties.contentLength, Created:properties.creationTime}" \
  --output table
```

**Expected Flow:**
```
Device → IoT Hub → Event Hubs → Stream Analytics (query) → Service Bus Queue → Alert Handler → IoT Hub C2D
                                                          ↘ ADLS (hotpath/)
```

---

## Recipe 8: Test Cold Path (Batch Processing)

**Goal:** Verify batch processing via Telemetry Processor Container App

### Step 1: Send Batch of Normal Telemetry

```bash
# Send 50 normal temperature readings
for i in {1..50}; do
  TEMP=$(( 20 + RANDOM % 30 ))  # 20-50°C
  HUMIDITY=$(( 40 + RANDOM % 30 ))  # 40-70%

  az iot device send-d2c-message \
    --hub-name $(terraform output -raw iothub_name) \
    --device-id sensor-001 \
    --data "{\"temperature\": $TEMP, \"humidity\": $HUMIDITY, \"timestamp\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}"

  if (( $i % 10 == 0 )); then
    echo "Sent $i messages"
  fi
done
```

### Step 2: Monitor Telemetry Processor

```bash
# View logs
az containerapp logs show \
  --name ca-telemetry-processor-iot-dev \
  --resource-group rg-iot-dev \
  --follow \
  --tail 100
```

### Step 3: Check ADLS Medallion Architecture

```bash
# Check bronze layer (raw ingestion)
az storage blob list \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name bronze \
  --auth-mode login \
  --output table

# Check silver layer (cleansed)
az storage blob list \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name silver \
  --auth-mode login \
  --output table

# Check gold layer (aggregated)
az storage blob list \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name gold \
  --auth-mode login \
  --output table
```

### Step 4: Download and Inspect Data

```bash
# Download bronze file
az storage blob download \
  --account-name $(terraform output -raw storage_account_name) \
  --container-name bronze \
  --name "$(az storage blob list --account-name $(terraform output -raw storage_account_name) --container-name bronze --auth-mode login --query '[0].name' -o tsv)" \
  --file bronze_data.json \
  --auth-mode login

# View contents
cat bronze_data.json | jq '.'
```

**Expected Flow:**
```
Device → IoT Hub → Event Hubs → Telemetry Processor → ADLS
                                                      ├─ bronze/ (raw)
                                                      ├─ silver/ (cleansed)
                                                      └─ gold/ (aggregated)
```

---

## Recipe 9: Clean Up Resources

**Goal:** Remove all Azure resources to stop billing

### Option A: Destroy Everything

```bash
cd infra/terraform/envs/dev

# Destroy all resources
terraform destroy

# Confirm by typing "yes"
```

**Time:** ~10-15 minutes

### Option B: Selective Destruction

**Keep foundation, remove expensive services:**
```bash
# Remove Stream Analytics only (~$60/mo savings)
terraform destroy -target=module.stream_analytics

# Remove Container Apps (~$5/mo savings)
terraform destroy -target=module.container_apps

# Remove PostgreSQL (~$12/mo savings)
terraform destroy -target=module.database
```

### Option C: Manual Cleanup (if Terraform fails)

```bash
# Delete resource group (deletes all resources)
az group delete --name rg-iot-dev --yes --no-wait

# Verify deletion
az group show --name rg-iot-dev
# Should return error: ResourceGroupNotFound
```

### Verify Cleanup

```bash
# List remaining resources
az resource list --resource-group rg-iot-dev

# Check for orphaned resources
az resource list --query "[?contains(name, 'iot-dev')]" --output table
```

**Important:** If using remote Terraform backend, remember to also clean up the state storage account if no longer needed.

---

## Recipe 10: Troubleshoot Common Issues

### Issue 1: Terraform Init Fails

**Symptom:**
```
Error: Failed to install provider
```

**Solution:**
```bash
# Clear cache and retry
rm -rf .terraform .terraform.lock.hcl
terraform init
```

### Issue 2: Key Vault Access Denied

**Symptom:**
```
Error: Forbidden: The user, group or application does not have secrets get permission
```

**Solution:**
```bash
# Grant yourself Key Vault Administrator role
az role assignment create \
  --role "Key Vault Administrator" \
  --assignee $(az ad signed-in-user show --query id -o tsv) \
  --scope $(terraform output -raw key_vault_id)

# Wait 2-3 minutes for propagation
sleep 180

# Retry
terraform apply
```

### Issue 3: PostgreSQL Deployment Timeout

**Symptom:**
```
Error: waiting for creation of PostgreSQL Flexible Server: context deadline exceeded
```

**Solution:**
```bash
# Check if server is actually creating
az postgres flexible-server show \
  --resource-group rg-iot-dev \
  --name psql-iot-dev \
  --query "state"

# If state is "Ready", just re-run terraform
terraform apply

# PostgreSQL takes 10-15 minutes - be patient!
```

### Issue 4: Container Apps Image Pull Failure

**Symptom:**
```
Container app using placeholder image
```

**Solution:**
This is expected! The infrastructure uses placeholder images. You'll replace them with real images in Phase 2 (application code).

For now, this is OK:
```bash
# Check Container App status
az containerapp show \
  --name ca-telemetry-processor-iot-dev \
  --resource-group rg-iot-dev \
  --query "properties.runningStatus"
```

### Issue 5: Stream Analytics Job Stuck in "Starting"

**Symptom:**
```
Stream Analytics job state: Starting (for more than 10 minutes)
```

**Solution:**
```bash
# Stop the job
az stream-analytics job stop \
  --resource-group rg-iot-dev \
  --name asa-iot-dev

# Wait 2 minutes
sleep 120

# Start again
az stream-analytics job start \
  --resource-group rg-iot-dev \
  --name asa-iot-dev \
  --output-start-mode JobStartTime
```

### Issue 6: No Telemetry Appearing in ADLS

**Symptom:**
Sent telemetry but no files in ADLS containers

**Debug Steps:**

**1. Check IoT Hub:**
```bash
az iot hub monitor-events \
  --hub-name $(terraform output -raw iothub_name) \
  --timeout 30
```

**2. Check Event Hubs:**
```bash
az eventhubs eventhub show \
  --resource-group rg-iot-dev \
  --namespace-name $(terraform output -raw eventhub_namespace_name) \
  --name eh-telemetry-iot-dev
```

**3. Check Container App Logs:**
```bash
az containerapp logs show \
  --name ca-telemetry-processor-iot-dev \
  --resource-group rg-iot-dev \
  --tail 50
```

**4. Check RBAC Assignments:**
```bash
# Verify Container App has ADLS permissions
az role assignment list \
  --assignee $(az containerapp show --name ca-telemetry-processor-iot-dev --resource-group rg-iot-dev --query "identity.principalId" -o tsv) \
  --scope $(terraform output -raw storage_account_id)
```

### Issue 7: High Azure Costs

**Symptom:**
Monthly bill higher than expected $219

**Debug:**
```bash
# View cost by service
az consumption usage list \
  --start-date $(date -d '30 days ago' +%Y-%m-%d) \
  --end-date $(date +%Y-%m-%d) \
  --query "[?contains(instanceName, 'iot-dev')].{Service:meterCategory, Cost:pretaxCost}" \
  --output table
```

**Solutions:**
- Pause Stream Analytics (~$60/mo savings)
- Reduce Log Analytics ingestion (change daily cap)
- Delete PostgreSQL if not using Device Registry
- Scale Container Apps to 0 (already configured)

### Issue 8: IoT Hub Throttling

**Symptom:**
```
Error: QuotaExceeded - Message count exceeded
```

**Solution:**
B1 tier allows 400K msgs/day. Upgrade to S1 for unlimited:
```bash
# Upgrade to S1 (not via Terraform - manual)
az iot hub update \
  --name $(terraform output -raw iothub_name) \
  --sku S1 \
  --unit 1

# Cost increases from $10/mo to $25/mo
```

### Issue 9: Terraform State Locked

**Symptom:**
```
Error: Error acquiring the state lock
```

**Solution:**
```bash
# If using remote backend
az storage blob lease break \
  --container-name tfstate \
  --blob-name dev.terraform.tfstate \
  --account-name sttfstateiot

# If using local state
rm .terraform.tfstate.lock.info
```

### Issue 10: Region Not Available

**Symptom:**
```
Error: Location 'westus3' is not accepting new customers
```

**Solution:**
```bash
# Edit terraform.tfvars
vim terraform.tfvars

# Change location to:
location = "eastus"  # or westus2, northeurope
```

---

## 🎓 Additional Resources

- **Prerequisites:** [PREREQUISITES.md](PREREQUISITES.md)
- **Quick Start:** [QUICK_START.md](QUICK_START.md)
- **Deployment Guide:** [envs/dev/DEPLOYMENT_GUIDE.md](envs/dev/DEPLOYMENT_GUIDE.md)
- **Architecture Details:** [INTEGRATION_COMPLETE.md](INTEGRATION_COMPLETE.md)
- **Main README:** [README.md](README.md)

---

## 💡 Pro Tips

1. **Always test in stages** - Don't deploy everything at once on first try
2. **Monitor costs daily** - Set up budget alerts in Azure Portal
3. **Use Terraform outputs** - `terraform output` saves time vs. Portal lookups
4. **Keep terraform.tfvars secure** - Never commit to Git (already in .gitignore)
5. **Document your changes** - Update this cookbook with your own recipes!

---

**Happy cooking!** 🧑‍🍳
