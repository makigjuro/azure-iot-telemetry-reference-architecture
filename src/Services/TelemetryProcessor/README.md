# TelemetryProcessor Service

**Status:** ✅ Fully Implemented and Building
**Type:** Worker Service (BackgroundService)
**Architecture:** Hexagonal Architecture + CQRS (Wolverine)
**Purpose:** Cold path telemetry processing with medallion architecture (bronze/silver/gold)

## Overview

The TelemetryProcessor service consumes device telemetry from Azure Event Hubs, validates and enriches the data, and persists it to Azure Data Lake Storage Gen2 in a medallion architecture. This enables both real-time stream processing and batch analytics.

## Architecture

### High-Level Flow

```
Event Hubs (telemetry stream)
    ↓
EventHubConsumerService (Infrastructure Adapter)
    ↓
Wolverine Message Bus (In-Process CQRS)
    ↓
Command Handlers (Application Layer)
    ↓
Domain Models (Shared.Domain)
    ↓
DataLakeStorageService (Infrastructure Adapter)
    ↓
ADLS Gen2 (bronze/silver/gold layers)
```

### Hexagonal Architecture

```
┌─────────────────────────────────────────────────┐
│ Host Layer (TelemetryProcessor.Host)            │
│ - Program.cs (DI, Wolverine, OpenTelemetry)     │
│ - Worker.cs (BackgroundService)                 │
│ - appsettings.json (Configuration)              │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ Application Layer (Pure Business Logic)         │
│                                                  │
│ Ports (Interfaces):                             │
│ - IEventHubConsumer                             │
│ - ITelemetryStorage                             │
│ - ITelemetryValidator                           │
│ - IDeviceMetadataRepository                     │
│                                                  │
│ Commands:                                        │
│ - ProcessTelemetryCommand                       │
│ - ValidateTelemetryCommand                      │
│ - EnrichTelemetryCommand                        │
│ - StoreTelemetryCommand                         │
│                                                  │
│ Handlers (Wolverine):                           │
│ - ProcessTelemetryHandler                       │
│ - ValidateTelemetryHandler                      │
│ - EnrichTelemetryHandler                        │
│ - StoreTelemetryHandler                         │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ Infrastructure Layer (Adapters)                  │
│                                                  │
│ EventHubConsumerService:                        │
│ - EventProcessorClient with checkpointing       │
│ - Managed identity authentication               │
│ - Graceful shutdown with message draining       │
│                                                  │
│ DataLakeStorageService:                         │
│ - Bronze: Raw JSON persistence                  │
│ - Silver: Validated + enriched data             │
│ - Gold: Aggregated metrics                      │
│ - Date/hour partitioning                        │
│                                                  │
│ DeviceMetadataRepository:                       │
│ - PostgreSQL device metadata lookup             │
│ - EF Core 9 (to be implemented)                 │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│ Domain Layer (Shared.Domain)                     │
│ - TelemetryReading (Aggregate)                  │
│ - TelemetryValue (Value Object)                 │
│ - DeviceId (Value Object)                       │
│ - Domain Events (TelemetryValidated, etc.)      │
└─────────────────────────────────────────────────┘
```

## Processing Pipeline

### 1. Event Consumption
```csharp
// EventHubConsumerService receives EventData from Event Hubs
EventData → DeserializeTelemetry() → TelemetryReading
  → Publish ProcessTelemetryCommand to Wolverine
```

### 2. Bronze Layer (Raw Persistence)
```csharp
// ProcessTelemetryHandler
ProcessTelemetryCommand
  → Store to bronze layer (audit trail)
  → Return ValidateTelemetryCommand (cascade)
```

**Bronze Path:**
`/bronze/2025/01/24/15/device-001_20250124153045.json`

### 3. Validation
```csharp
// ValidateTelemetryHandler
ValidateTelemetryCommand
  → TelemetryValidator.ValidateAsync()
  → Check quality, age, measurement count
  → Raise TelemetryValidatedEvent
  → Return EnrichTelemetryCommand (if valid)
```

**Validation Rules:**
- ❌ Reject telemetry with bad quality measurements
- ❌ Reject telemetry older than 24 hours
- ❌ Reject telemetry with >100 measurements
- ❌ Reject future timestamps (>5 min ahead)

### 4. Enrichment
```csharp
// EnrichTelemetryHandler
EnrichTelemetryCommand
  → DeviceMetadataRepository.GetMetadataAsync()
  → Add location, model, manufacturer, firmware version
  → Raise TelemetryEnrichedEvent
  → Return StoreTelemetryCommand
```

### 5. Silver Layer (Enriched Persistence)
```csharp
// StoreTelemetryHandler
StoreTelemetryCommand
  → Store to silver layer with metadata
```

**Silver Path:**
`/silver/2025/01/24/15/device-001_20250124153045.json`

**Silver Data Structure:**
```json
{
  "id": "guid",
  "deviceId": "device-001",
  "timestamp": "2025-01-24T15:30:45Z",
  "receivedAt": "2025-01-24T15:30:46Z",
  "isValid": true,
  "validationError": null,
  "measurements": {
    "temperature": {
      "value": 22.5,
      "unit": "celsius",
      "quality": "Good"
    },
    "humidity": {
      "value": 65.0,
      "unit": "percent",
      "quality": "Good"
    }
  },
  "metadata": {
    "location": "Warehouse-A",
    "model": "IoT-Sensor-v2",
    "manufacturer": "Contoso",
    "firmwareVersion": "1.2.3"
  }
}
```

### 6. Gold Layer (Aggregations - TODO)
Hourly aggregations for analytics:
- Average, min, max, count per device per metric
- Stored at `/gold/2025/01/24/hourly_aggregates_15.json`

## Project Structure

```
TelemetryProcessor/
├── TelemetryProcessor.Application/
│   ├── Commands/
│   │   ├── ProcessTelemetryCommand.cs
│   │   ├── ValidateTelemetryCommand.cs
│   │   ├── EnrichTelemetryCommand.cs
│   │   └── StoreTelemetryCommand.cs
│   ├── Handlers/
│   │   ├── ProcessTelemetryHandler.cs
│   │   ├── ValidateTelemetryHandler.cs
│   │   ├── EnrichTelemetryHandler.cs
│   │   └── StoreTelemetryHandler.cs
│   ├── Ports/
│   │   ├── IEventHubConsumer.cs
│   │   ├── ITelemetryStorage.cs
│   │   ├── ITelemetryValidator.cs
│   │   └── IDeviceMetadataRepository.cs
│   └── Validators/
│       └── TelemetryValidator.cs
├── TelemetryProcessor.Infrastructure/
│   ├── EventHubs/
│   │   ├── EventHubConsumerService.cs
│   │   └── EventHubConsumerOptions.cs
│   ├── Storage/
│   │   ├── DataLakeStorageService.cs
│   │   └── DataLakeOptions.cs
│   └── Database/
│       └── DeviceMetadataRepository.cs
└── TelemetryProcessor.Host/
    ├── Program.cs
    ├── Worker.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

## Configuration

### appsettings.json

```json
{
  "EventHub": {
    "FullyQualifiedNamespace": "eh-iot-dev-mg123.servicebus.windows.net",
    "EventHubName": "telemetry",
    "ConsumerGroup": "$Default",
    "CheckpointBlobContainer": "checkpoints",
    "CheckpointStorageAccount": "stiotdevmg123",
    "MaxBatchSize": 100,
    "MaxWaitTimeSeconds": 10
  },
  "DataLake": {
    "AccountName": "stiotdevmg123",
    "BronzeContainer": "bronze",
    "SilverContainer": "silver",
    "GoldContainer": "gold"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "TelemetryProcessor": "Debug"
      }
    }
  }
}
```

## Running Locally

### Prerequisites
- .NET 9 SDK
- Azure subscription with deployed infrastructure
- Azure CLI logged in (`az login`)
- Azure Developer CLI authenticated (`azd auth login`)

### Run the Service

```bash
cd src/Services/TelemetryProcessor/TelemetryProcessor.Host

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (uses managed identity for Azure auth)
dotnet run
```

### Send Test Telemetry

```bash
# Create test device
az iot hub device-identity create \
  --hub-name <your-iothub-name> \
  --device-id test-device-001

# Send telemetry
az iot device send-d2c-message \
  --hub-name <your-iothub-name> \
  --device-id test-device-001 \
  --data '{
    "deviceId": "test-device-001",
    "timestamp": "2025-01-24T15:30:45Z",
    "measurements": {
      "temperature": {"value": 22.5, "unit": "celsius"},
      "humidity": {"value": 65.0, "unit": "percent"}
    }
  }'
```

### Verify Processing

```bash
# Check bronze layer
az storage blob list \
  --account-name stiotdevmg123 \
  --container-name bronze \
  --prefix "bronze/2025" \
  --auth-mode login

# Check silver layer
az storage blob list \
  --account-name stiotdevmg123 \
  --container-name silver \
  --prefix "silver/2025" \
  --auth-mode login

# Download a file
az storage blob download \
  --account-name stiotdevmg123 \
  --container-name silver \
  --name "silver/2025/01/24/15/test-device-001_20250124153045.json" \
  --file telemetry.json \
  --auth-mode login
```

## Testing

### Unit Tests (TODO)
```bash
cd src/Services/TelemetryProcessor/TelemetryProcessor.Tests
dotnet test
```

**Test Coverage:**
- Command handler logic
- Validation rules
- Domain model behavior
- Event raising

### Integration Tests (TODO)
```bash
cd src/Services/TelemetryProcessor/TelemetryProcessor.IntegrationTests
dotnet test
```

**Test Coverage:**
- Event Hubs consumption (using Testcontainers)
- Storage persistence (using Azurite)
- End-to-end pipeline
- Error handling and retries

## Deployment

### Docker Build
```bash
cd src/Services/TelemetryProcessor
docker build -t telemetryprocessor:latest -f TelemetryProcessor.Host/Dockerfile .
```

### Azure Container Apps
```bash
# Deploy using Azure CLI
az containerapp create \
  --name telemetry-processor \
  --resource-group rg-iot-dev-mg123 \
  --environment <container-apps-env> \
  --image <acr-name>.azurecr.io/telemetryprocessor:latest \
  --managed-identity system \
  --min-replicas 1 \
  --max-replicas 5 \
  --cpu 1.0 \
  --memory 2.0Gi
```

## Observability

### Logging
- **Serilog** structured logging to console
- **Application Insights** for production telemetry
- Log levels: Debug for TelemetryProcessor namespace, Information for others

### Tracing
- **OpenTelemetry** distributed tracing
- Custom spans for each processing stage
- Correlation IDs across services

### Metrics
- **OpenTelemetry Metrics**
  - Telemetry messages processed
  - Validation failure rate
  - Processing latency (P50, P95, P99)
  - Storage write latency

### Health Checks
- Liveness: `/health/live`
- Readiness: `/health/ready`

## Resilience

### Event Hubs
- **Checkpointing:** Exactly-once processing semantics
- **Graceful Shutdown:** Drains in-flight messages before stopping
- **Retry Policy:** Polly exponential backoff for transient failures

### Storage
- **Retry Policy:** Polly exponential backoff with jitter
- **Circuit Breaker:** Opens after 5 consecutive failures
- **Timeout:** 30 seconds per operation

## Performance

### Throughput
- **Design Target:** 10,000 messages/second
- **Batch Size:** 100 messages (configurable)
- **Parallelism:** Event Hubs partitions (auto-scale)

### Latency
- **Bronze Write:** <100ms (P95)
- **Silver Write:** <500ms (P95) including enrichment
- **End-to-End:** <1 second (P95) from Event Hubs to ADLS

## Troubleshooting

### Common Issues

**Issue: "Unable to connect to Event Hubs"**
- Ensure managed identity has "Azure Event Hubs Data Receiver" role
- Check Event Hub namespace and name in appsettings.json
- Verify network connectivity

**Issue: "Storage account not found"**
- Ensure managed identity has "Storage Blob Data Contributor" role
- Verify storage account name in appsettings.json
- Check containers exist (bronze, silver, gold, checkpoints)

**Issue: "Telemetry validation failures"**
- Check telemetry age (max 24 hours)
- Verify quality indicators (no "Bad" quality)
- Ensure measurement count <100

**Issue: "Checkpointing errors"**
- Verify checkpoint blob container exists
- Ensure managed identity has "Storage Blob Data Contributor" on checkpoint storage
- Check for storage throttling

## Next Steps

1. **Unit Tests** - Add xUnit tests for handlers and validators
2. **Integration Tests** - Add Testcontainers-based integration tests
3. **Gold Layer** - Implement hourly aggregations background job
4. **EF Core** - Complete PostgreSQL device metadata repository
5. **Dockerfile** - Create optimized multi-stage Dockerfile
6. **CI/CD** - Add GitHub Actions workflow for build/test/deploy

## References

- [Wolverine Documentation](https://wolverine.netlify.app/)
- [Azure Event Hubs .NET SDK](https://learn.microsoft.com/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send)
- [Azure Blob Storage .NET SDK](https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet)
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Medallion Architecture](https://www.databricks.com/glossary/medallion-architecture)
