# Azure IoT Telemetry Reference Architecture

A professional, production-ready **reference architecture** and **template codebase** for building **IoT telemetry solutions** on **Microsoft Azure** using:

- **Azure IoT Hub** – secure device connectivity, D2C/C2D messaging, device registry
- **Device Provisioning Service (DPS)** – zero-touch device provisioning at scale
- **Azure IoT Edge** – edge computing with local processing and offline support
- **Azure Digital Twins** – DTDL device models and spatial intelligence
- **Azure Stream Analytics** – real-time stream processing and anomaly detection
- **Azure Event Grid & Event Hubs** – event routing (CloudEvents 1.0) and telemetry streaming
- **Azure Container Apps (ACA)** – microservices and worker processing
- **Azure Data Lake Storage Gen2 (ADLS)** – medallion architecture (raw/bronze/silver/gold)
- **Azure Synapse Analytics** – data pipelines, SQL pools, and notebooks
- **Microsoft Fabric** – lakehouse, semantic models, and Power BI reports
- **Azure Key Vault, Log Analytics, App Insights** – security and observability
- **PostgreSQL, VNet, Private Endpoints** – secure data storage and networking

This repository provides:
- Infrastructure as Code (**Terraform** modules, Bicep optional)
- .NET 9 samplex services (Minimal API, Workers, CloudEvents integration)
- CI/CD pipelines with **GitHub Actions**
- Security and cost-optimization best practices
- C4 diagrams and architecture documentation

---

## 📐 Architecture Overview

### Infrastructure Diagram (Detailed Technical View)

Complete infrastructure diagram with private endpoints, managed identities, VNet topology, and RBAC assignments.

📊 **[View Interactive Diagram](docs/infrastructure-diagram.mmd)** | 📄 **[Full Documentation with Data Flows & RBAC →](docs/infrastructure-diagram.md)**

![Infrastructure Diagram](docs/infrastructure-diagram.mmd)

### C1: System Context Diagram

![C1 System Context](https://www.plantuml.com/plantuml/proxy?cache=no&src=https://raw.githubusercontent.com/makigjuro/azure-iot-telemetry-reference-architecture/main/docs/architecture-c1-diagram.puml)

---

## 🚀 Features

### IoT Device Management
- **Device provisioning** with DPS (X.509 certificates, TPM, symmetric keys)
- **Device registry** in IoT Hub with per-device authentication
- **Device twins** for state synchronization and configuration management
- **Cloud-to-Device (C2D)** messages for commands and control
- **IoT Edge** deployment for local processing and offline operation
- **Azure Digital Twins** DTDL models for device validation

### Data Processing
- **Hot Path**: Real-time processing with Stream Analytics (windowing, aggregation, anomaly detection)
- **Cold Path**: Batch processing with ACA workers writing to Data Lake
- **Medallion Architecture**: raw → bronze → silver → gold telemetry zones
- **Event-driven**: CloudEvents standard with Event Grid routing

### Security & Networking
- **Private Endpoints** for all data services (IoT Hub, Event Hubs, Storage, Synapse)
- **Managed Identities** for secure, credential-less service-to-service authentication
- **VNet isolation** with dedicated subnets for management, application, and data layers
- **TLS 1.2+** enforced for all device connections
- **Entra ID RBAC** with least privilege access
- **Key Vault** for secrets, certificates, and connection strings

### Analytics & Observability
- **Synapse Analytics** for data warehousing and ETL pipelines
- **Microsoft Fabric** lakehouse with Power BI semantic models
- **Application Insights** with distributed tracing (OpenTelemetry)
- **Log Analytics** workspace with KQL queries for device telemetry
- **Diagnostic settings** enabled on all Azure resources

### Infrastructure & DevOps
- **IaC-first**: Terraform modules for all Azure resources
- **CI/CD pipelines**: GitHub Actions for infrastructure + applications
- **Cost optimization**: ACA autoscale (minScale=0), Synapse pause, storage lifecycle policies
- **PostgreSQL Flexible Server** for application metadata

---

## 📂 Repository Structure
```
azure-iot-telemetry-reference-architecture/
├─ docs/
│  ├─ infrastructure-diagram.md   # Detailed Azure infrastructure diagram
│  ├─ architecture-c1-diagram.puml
│  ├─ architecture.md
│  ├─ security.md
│  ├─ operations.md
│  └─ diagrams/
├─ infra/
│  ├─ terraform/
│  │  ├─ envs/
│  │  └─ modules/
│  └─ bicep/
├─ src/
│  ├─ gateway-api/           # .NET 9 Minimal API for IoT devices
│  ├─ telemetry-ingestor/    # Worker publishes telemetry events
│  ├─ telemetry-processor/   # Worker consumes telemetry → ADLS
│  └─ alert-service/         # Example subscriber (alerts)
├─ data/
│  ├─ synapse/
│  └─ fabric/
├─ .github/workflows/
│  ├─ ci-build-test.yml
│  ├─ cd-infra-terraform.yml
│  └─ cd-apps-aca.yml
└─ LICENSE
```

---

## 🔒 Security Baseline
- **Device Authentication**: X.509 certificates or SAS tokens per device
- **Zero public endpoints** for IoT Hub, Event Hubs, Storage, Synapse (private endpoints only)
- **System-assigned Managed Identity** for all services (ACA, IoT Hub, Stream Analytics, Synapse)
- **Secrets in Key Vault**, no plain-text configs or connection strings
- **Private Endpoints** for IoT Hub, Event Hubs, ADLS, Synapse, PostgreSQL, Key Vault
- **VNet isolation** with NSGs and service endpoints
- **TLS 1.2+** enforced for device-to-cloud connections
- **API ingress** protected by Entra ID (OIDC)
- **RBAC with least privilege**: IoT Hub Registry Contributor, Storage Blob Data Contributor, Event Hubs Data Receiver
- **Audit logging** via Log Analytics for device connections and API access
- **Defender for Cloud** baseline policies

---

## 📊 Observability
- **App Insights + OpenTelemetry** for tracing
- **Log Analytics** workspace with KQL queries for IoT telemetry
- Pre-built dashboards in `/docs/operations.md`

---

## 🛠️ Quickstart
```bash
# Deploy infrastructure (dev environment)
cd infra/terraform/envs/dev
terraform init
terraform apply

# Build and push images
cd src/gateway-api
az acr build --registry <acr_name> --image gateway:latest .

# Deploy apps via GitHub Actions or Terraform
```


## 📄 License
This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.


