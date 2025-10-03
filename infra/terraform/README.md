# Terraform Infrastructure - Azure IoT Telemetry Reference Architecture

Production-ready infrastructure using lowest-tier Azure services for development and learning.

## Documentation

| Guide | Description |
|-------|-------------|
| [Prerequisites](PREREQUISITES.md) | Azure subscription, tools installation, permissions |
| [Quick Start](QUICK_START.md) | Deploy in ~30 minutes |
| [Cookbook](COOKBOOK.md) | Step-by-step recipes for common tasks |
| [Deployment Guide](envs/dev/DEPLOYMENT_GUIDE.md) | Detailed deployment options and troubleshooting |

---

## 📊 Estimated Monthly Costs (Dev Environment)

| Service | SKU/Tier | Estimated Cost | Notes |
|---------|----------|----------------|-------|
| **Virtual Network** | Standard | **FREE** | Data transfer charges only |
| **NSGs** | Standard | **FREE** | - |
| **Log Analytics** | Pay-as-you-go (1GB/day cap) | **~$70/mo** | First 5GB/month FREE |
| **Application Insights** | Workspace-based (10% sampling) | **~$10/mo** | Reduced by 90% with sampling |
| **Storage (ADLS Gen2)** | Standard LRS | **~$15/mo** | $0.018/GB + lifecycle mgmt |
| **Key Vault** | Standard | **~$1/mo** | $0.03 per 10K operations |
| **IoT Hub** | B1 Basic | **~$10/mo** | 400K msgs/day |
| **Device Provisioning Service** | S1 Standard | **Included** | Free with IoT Hub |
| **Event Hubs** | Basic (1 TU) | **~$11/mo** | 1M events/day |
| **Service Bus** | Basic | **~$1/mo** | 1M operations/month |
| **Stream Analytics** | 1 SU | **~$81/mo** | Can pause when not in use |
| **PostgreSQL** | Burstable B1ms | **~$12/mo** | 1 vCore, 2GB RAM |
| **Digital Twins** | Pay-as-you-go | **~$5/mo** | Low usage |
| **Container Apps** | Consumption | **~$3/mo** | Scales to 0 when idle |
| | | |
| **TOTAL (Full architecture)** | | **~$219/mo** | All 11 modules deployed |
| **TOTAL (Optimized)** | | **~$138/mo** | Stream Analytics paused |

### 💡 Cost Optimization Tips

✅ **Already Implemented:**
- Standard LRS storage (cheapest replication)
- Lifecycle policies (auto-archive/delete old data)
- Application Insights sampling (90% cost reduction)
- Burstable PostgreSQL tier
- Container Apps consumption plan (scales to 0)
- Log Analytics daily cap (1GB/day)

✅ **Manual Optimizations:**
- **Pause Stream Analytics** when not testing (~$60/mo savings)
- **Delete resources** after testing sessions
- **Use Azure Dev/Test pricing** if available
- **Set up budgets & alerts** in Azure Cost Management

---

## Module Structure

All 11 modules are complete and ready to deploy.

### Layer 1: Foundation

#### **Networking Module** (`modules/networking`)
- VNet with 3 subnets (management, application, data)
- NSGs with security rules (MQTT, HTTPS, PostgreSQL)
- Service endpoints for Storage/KeyVault/EventHub
- **Cost: FREE**

#### **Monitoring Module** (`modules/monitoring`)
- Log Analytics Workspace (PerGB2018, 30-day retention, 1GB/day cap)
- Application Insights (10% sampling)
- Diagnostic settings for all resources
- **Cost: ~$80/mo**

### Layer 2: Security & Storage

#### **Security Module** (`modules/security`)
- Key Vault (Standard tier, RBAC-based access)
- Stores all connection strings and secrets
- Network ACLs support
- **Cost: ~$1/mo**

#### **Storage Module** (`modules/storage`)
- ADLS Gen2 (Standard LRS)
- Medallion architecture: raw, bronze, silver, gold, hotpath
- Storage Queue for Event Grid
- Lifecycle management (auto-archive/delete)
- **Cost: ~$15/mo**

### Layer 3: IoT Services

#### **IoT Hub Module** (`modules/iot-hub`)
- IoT Hub B1 (400K msgs/day)
- Device Provisioning Service (S1)
- System-assigned managed identity
- Built-in Event Hub endpoint routing
- **Cost: ~$10/mo**

#### **Event Streaming Module** (`modules/event-streaming`)
- Event Hubs Namespace (Basic tier, 1 TU)
- Event Hub: telemetry (2 partitions, 1-day retention)
- Event Grid System Topic (device lifecycle events)
- Service Bus Queue (for Stream Analytics alerts)
- **Cost: ~$12/mo**

### Layer 4: Data Services

#### **Database Module** (`modules/database`)
- PostgreSQL Flexible Server (Burstable B1ms)
- VNet integration
- Device registry database
- **Cost: ~$12/mo**

#### **Digital Twins Module** (`modules/digital-twins`)
- Azure Digital Twins instance
- System-assigned managed identity
- DTDL device models
- **Cost: ~$5/mo**

### Layer 5: Analytics

#### **Stream Analytics Module** (`modules/stream-analytics`)
- 1 SU for real-time processing
- Hot path: temperature alerts
- Outputs: ADLS (hotpath) + Service Bus Queue
- **Cost: ~$81/mo** (pause when not testing)

### Layer 6: Compute

#### **Container Apps Module** (`modules/container-apps`)
- Container Apps Environment
- 3 microservices:
  - Telemetry Processor (cold path)
  - Alert Handler (hot path)
  - Event Subscriber (device lifecycle)
- Scales to 0 when idle
- **Cost: ~$3/mo**

### Layer 7: Security

#### **RBAC Module** (`modules/rbac`)
- 12 role assignments for managed identities
- Service-to-service authentication
- Least privilege access
- **Cost: FREE**

---

## Quick Start

See [Quick Start Guide](QUICK_START.md) for detailed instructions.

```bash
cd envs/dev
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars and set postgres_admin_password

terraform init
terraform apply
terraform output
```

Deploys 70-80 Azure resources in 20-30 minutes. Cost: ~$219/mo (~$138/mo with Stream Analytics paused).

---

## Directory Structure

```
infra/terraform/
├── envs/
│   ├── dev/                        # Development environment
│   │   ├── provider.tf             # Azure provider config
│   │   ├── main.tf                 # Complete orchestration (11 modules)
│   │   ├── variables.tf            # Input variables
│   │   ├── outputs.tf              # 30+ output values
│   │   ├── terraform.tfvars.example
│   │   └── DEPLOYMENT_GUIDE.md     # Detailed deployment guide
│   ├── staging/                    # Staging environment (future)
│   └── prod/                       # Production environment (future)
├── modules/
│   ├── networking/                 # VNet, Subnets, NSGs
│   ├── monitoring/                 # Log Analytics, App Insights
│   ├── storage/                    # ADLS Gen2 (medallion arch)
│   ├── security/                   # Key Vault
│   ├── iot-hub/                    # IoT Hub + DPS
│   ├── event-streaming/            # Event Hubs + Event Grid + Service Bus
│   ├── stream-analytics/           # Stream Analytics (1 SU)
│   ├── database/                   # PostgreSQL Flexible Server
│   ├── digital-twins/              # Azure Digital Twins
│   ├── container-apps/             # ACA + 3 microservices
│   └── rbac/                       # 12 role assignments
├── shared/
│   └── locals.tf                   # Naming conventions, cost tiers
├── PREREQUISITES.md                # Prerequisites checklist
├── QUICK_START.md                  # Quick start guide
├── COOKBOOK.md                     # Step-by-step recipes
└── README.md                       # This file
```

---

## 🔐 Security Features

- **System-Assigned Managed Identities** for all services (no credentials in code)
- **RBAC-based access** to Key Vault and Storage
- **Private Endpoints** support for all data services
- **Network isolation** via VNet subnets
- **Diagnostic logging** to Log Analytics for all resources

---

## What's Included

- Complete infrastructure (11 Terraform modules)
- 70-80 Azure resources deployed via single `terraform apply`
- Cost optimized with lowest tiers, auto-scaling, and lifecycle policies
- Production-ready with managed identities, RBAC, and private endpoint support
- Comprehensive documentation

## Next Steps After Deployment

1. Verify all resources are created
2. Register test device via IoT Hub or DPS
3. Test telemetry ingestion (hot and cold paths)
4. Configure budget alerts in Azure Cost Management
5. Build .NET 9 microservices (Phase 2)
6. Add Synapse pipelines and Power BI dashboards (Phase 3)

See [Cookbook](COOKBOOK.md) for detailed recipes.

---

## 🛠️ Terraform Commands Reference

```bash
# Format code
terraform fmt -recursive

# Validate configuration
terraform validate

# Plan with variables
terraform plan -var="location=westus2"

# Apply specific module
terraform apply -target=module.networking

# View outputs
terraform output

# Refresh state
terraform refresh

# Import existing resource
terraform import azurerm_resource_group.main /subscriptions/.../resourceGroups/...
```

---

## 📚 Resources

- [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)
- [Terraform Azure Provider Docs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/architecture/framework/)
- [Cost Optimization Pillar](https://learn.microsoft.com/en-us/azure/architecture/framework/cost/)
