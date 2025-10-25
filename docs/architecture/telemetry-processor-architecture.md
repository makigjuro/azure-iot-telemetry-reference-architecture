# TelemetryProcessor Service Architecture

**Service:** TelemetryProcessor
**Type:** Worker Service (Background Service)
**Pattern:** Hexagonal Architecture + CQRS
**Status:** ✅ Fully Implemented

## Table of Contents

1. [High-Level Architecture](#high-level-architecture)
2. [Detailed Component Diagram](#detailed-component-diagram)
3. [Processing Flow](#processing-flow)
4. [Data Flow](#data-flow)
5. [Hexagonal Architecture Layers](#hexagonal-architecture-layers)
6. [Medallion Architecture](#medallion-architecture)
7. [CQRS Pattern with Wolverine](#cqrs-pattern-with-wolverine)
8. [Sequence Diagrams](#sequence-diagrams)

---

## High-Level Architecture

```mermaid
graph TB
    subgraph "Azure Cloud"
        EH[Event Hubs<br/>telemetry stream]
        ADLS[ADLS Gen2<br/>Data Lake]
        PG[PostgreSQL<br/>Device Metadata]
        BLOB[Blob Storage<br/>Checkpoints]
    end

    subgraph "TelemetryProcessor Service"
        CONSUMER[EventHubConsumerService<br/>Infrastructure]
        BUS[Wolverine Bus<br/>In-Memory CQRS]
        HANDLERS[Command Handlers<br/>Application]
        STORAGE[DataLakeStorageService<br/>Infrastructure]
    end

    EH -->|Consume| CONSUMER
    CONSUMER -->|Checkpoint| BLOB
    CONSUMER -->|Publish Commands| BUS
    BUS -->|Route| HANDLERS
    HANDLERS -->|Query| PG
    HANDLERS -->|Write| STORAGE
    STORAGE -->|Persist| ADLS

    style CONSUMER fill:#e1f5ff
    style BUS fill:#fff4e6
    style HANDLERS fill:#e8f5e9
    style STORAGE fill:#e1f5ff
```

---

## Detailed Component Diagram

```mermaid
graph TB
    subgraph "Host Layer"
        PROGRAM[Program.cs<br/>DI Container Setup]
        WORKER[Worker.cs<br/>Background Service]
        CONFIG[appsettings.json<br/>Configuration]
    end

    subgraph "Application Layer"
        subgraph "Ports"
            IEHC[IEventHubConsumer]
            ITS[ITelemetryStorage]
            ITV[ITelemetryValidator]
            IDMR[IDeviceMetadataRepository]
        end

        subgraph "Commands"
            PTC[ProcessTelemetryCommand]
            VTC[ValidateTelemetryCommand]
            ETC[EnrichTelemetryCommand]
            STC[StoreTelemetryCommand]
        end

        subgraph "Handlers"
            PTH[ProcessTelemetryHandler]
            VTH[ValidateTelemetryHandler]
            ETH[EnrichTelemetryHandler]
            STH[StoreTelemetryHandler]
        end

        subgraph "Validators"
            TV[TelemetryValidator]
        end
    end

    subgraph "Infrastructure Layer"
        subgraph "Event Hubs"
            EHCS[EventHubConsumerService]
            EHCO[EventHubConsumerOptions]
        end

        subgraph "Storage"
            DLSS[DataLakeStorageService]
            DLO[DataLakeOptions]
        end

        subgraph "Database"
            DMR[DeviceMetadataRepository]
        end
    end

    subgraph "Domain Layer"
        TR[TelemetryReading<br/>Aggregate]
        TV_VO[TelemetryValue<br/>Value Object]
        DID[DeviceId<br/>Value Object]
        EVENTS[Domain Events]
    end

    PROGRAM --> WORKER
    PROGRAM --> CONFIG
    WORKER --> EHCS

    PTH -.implements.-> IEHC
    VTH -.uses.-> ITV
    ETH -.uses.-> IDMR
    STH -.uses.-> ITS

    EHCS -.implements.-> IEHC
    DLSS -.implements.-> ITS
    TV -.implements.-> ITV
    DMR -.implements.-> IDMR

    PTH --> PTC
    VTH --> VTC
    ETH --> ETC
    STH --> STC

    PTH --> TR
    VTH --> TR
    ETH --> TR
    STH --> TR

    TR --> TV_VO
    TR --> DID
    TR --> EVENTS

    style PROGRAM fill:#f9f9f9
    style EHCS fill:#e1f5ff
    style DLSS fill:#e1f5ff
    style PTH fill:#e8f5e9
    style VTH fill:#e8f5e9
    style ETH fill:#e8f5e9
    style STH fill:#e8f5e9
```

---

## Processing Flow

```mermaid
sequenceDiagram
    participant EH as Event Hubs
    participant EHCS as EventHubConsumerService
    participant WB as Wolverine Bus
    participant PTH as ProcessTelemetryHandler
    participant VTH as ValidateTelemetryHandler
    participant ETH as EnrichTelemetryHandler
    participant STH as StoreTelemetryHandler
    participant DLSS as DataLakeStorageService
    participant DMR as DeviceMetadataRepository
    participant ADLS as ADLS Gen2

    EH->>EHCS: EventData received
    EHCS->>EHCS: Deserialize to TelemetryReading
    EHCS->>WB: Publish ProcessTelemetryCommand

    WB->>PTH: Route command
    PTH->>DLSS: StoreBronzeAsync(reading)
    DLSS->>ADLS: Write raw JSON
    PTH->>WB: Return ValidateTelemetryCommand

    WB->>VTH: Route command
    VTH->>VTH: Validate (quality, age, count)

    alt Valid
        VTH->>WB: Raise TelemetryValidatedEvent
        VTH->>WB: Return EnrichTelemetryCommand

        WB->>ETH: Route command
        ETH->>DMR: GetMetadataAsync(deviceId)
        DMR-->>ETH: Device metadata
        ETH->>WB: Raise TelemetryEnrichedEvent
        ETH->>WB: Return StoreTelemetryCommand

        WB->>STH: Route command
        STH->>DLSS: StoreSilverAsync(reading, metadata)
        DLSS->>ADLS: Write enriched JSON
    else Invalid
        VTH->>WB: Raise TelemetryValidatedEvent (isValid=false)
        Note over VTH,WB: Pipeline stops here
    end

    EHCS->>EH: Update checkpoint
```

---

## Data Flow

```mermaid
graph LR
    subgraph "Input"
        RAW[Raw Telemetry<br/>JSON from IoT Hub]
    end

    subgraph "Bronze Layer"
        BRONZE[/bronze/<br/>yyyy/MM/dd/HH/<br/>deviceId_timestamp.json]
    end

    subgraph "Validation"
        VAL{Validate<br/>Quality, Age,<br/>Measurement Count}
    end

    subgraph "Enrichment"
        ENR[Lookup Device<br/>Metadata from<br/>PostgreSQL]
    end

    subgraph "Silver Layer"
        SILVER[/silver/<br/>yyyy/MM/dd/HH/<br/>deviceId_timestamp.json]
    end

    subgraph "Gold Layer (Future)"
        GOLD[/gold/<br/>yyyy/MM/dd/<br/>hourly_aggregates_HH.json]
    end

    RAW --> BRONZE
    BRONZE --> VAL
    VAL -->|Valid| ENR
    VAL -->|Invalid| STOP[Stop Processing]
    ENR --> SILVER
    SILVER -.->|Future| GOLD

    style BRONZE fill:#cd7f32
    style SILVER fill:#c0c0c0
    style GOLD fill:#ffd700
    style STOP fill:#ffcccc
```

---

## Hexagonal Architecture Layers

### 1. Domain Layer (Core)
**Location:** `Shared/IoTTelemetry.Domain`

```mermaid
classDiagram
    class TelemetryReading {
        <<Aggregate Root>>
        +Guid Id
        +DeviceId DeviceId
        +Timestamp Timestamp
        +Dictionary~string, TelemetryValue~ Measurements
        +bool IsValid
        +string ValidationError
        +Create() TelemetryReading
        +MarkAsInvalid(reason)
        +GetMeasurement(key) TelemetryValue
        +HasBadQuality() bool
        +GetAge() TimeSpan
    }

    class TelemetryValue {
        <<Value Object>>
        +double Value
        +string Unit
        +TelemetryQuality Quality
        +Create() TelemetryValue
    }

    class DeviceId {
        <<Value Object>>
        +string Value
        +Create(value) DeviceId
    }

    class TelemetryQuality {
        <<Enumeration>>
        Good
        Uncertain
        Bad
    }

    class IDomainEvent {
        <<Interface>>
        +DateTimeOffset OccurredAt
    }

    class TelemetryReceivedEvent {
        +DeviceId DeviceId
        +Timestamp Timestamp
        +int MeasurementCount
    }

    class TelemetryValidatedEvent {
        +DeviceId DeviceId
        +Guid TelemetryId
        +bool IsValid
        +string ValidationError
    }

    class TelemetryEnrichedEvent {
        +DeviceId DeviceId
        +Guid TelemetryId
        +Dictionary~string,string~ Metadata
    }

    TelemetryReading --> TelemetryValue
    TelemetryReading --> DeviceId
    TelemetryValue --> TelemetryQuality
    TelemetryReceivedEvent ..|> IDomainEvent
    TelemetryValidatedEvent ..|> IDomainEvent
    TelemetryEnrichedEvent ..|> IDomainEvent
```

**Principles:**
- ✅ Pure C# - no infrastructure dependencies
- ✅ Rich domain models with behavior
- ✅ Value objects for type safety
- ✅ Domain events for side effects

---

### 2. Application Layer (Use Cases)
**Location:** `TelemetryProcessor.Application`

```mermaid
graph TB
    subgraph "Ports (Interfaces)"
        IEC[IEventHubConsumer<br/>Port]
        ITS[ITelemetryStorage<br/>Port]
        ITV[ITelemetryValidator<br/>Port]
        IDMR[IDeviceMetadataRepository<br/>Port]
    end

    subgraph "Commands"
        PTC[ProcessTelemetryCommand<br/>Entry Point]
        VTC[ValidateTelemetryCommand]
        ETC[EnrichTelemetryCommand]
        STC[StoreTelemetryCommand]
    end

    subgraph "Handlers (Use Cases)"
        PTH[ProcessTelemetryHandler<br/>writes bronze, cascades]
        VTH[ValidateTelemetryHandler<br/>validates, cascades if valid]
        ETH[EnrichTelemetryHandler<br/>enriches, cascades]
        STH[StoreTelemetryHandler<br/>writes silver]
    end

    PTC --> PTH
    VTC --> VTH
    ETC --> ETH
    STC --> STH

    PTH -.uses.-> ITS
    VTH -.uses.-> ITV
    ETH -.uses.-> IDMR
    STH -.uses.-> ITS

    style IEC fill:#fff4e6
    style ITS fill:#fff4e6
    style ITV fill:#fff4e6
    style IDMR fill:#fff4e6
    style PTH fill:#e8f5e9
    style VTH fill:#e8f5e9
    style ETH fill:#e8f5e9
    style STH fill:#e8f5e9
```

**Principles:**
- ✅ No infrastructure code - only interfaces (ports)
- ✅ Pure business logic
- ✅ Commands define intent
- ✅ Handlers orchestrate workflows

---

### 3. Infrastructure Layer (Adapters)
**Location:** `TelemetryProcessor.Infrastructure`

```mermaid
graph TB
    subgraph "Application Ports"
        IEC[IEventHubConsumer]
        ITS[ITelemetryStorage]
        IDMR[IDeviceMetadataRepository]
    end

    subgraph "Infrastructure Adapters"
        EHCS[EventHubConsumerService<br/>implements IEventHubConsumer]
        DLSS[DataLakeStorageService<br/>implements ITelemetryStorage]
        DMR[DeviceMetadataRepository<br/>implements IDeviceMetadataRepository]
    end

    subgraph "Azure SDKs"
        EHSDK[Azure.Messaging.EventHubs]
        BLOBSDK[Azure.Storage.Blobs]
        EFSDK[EF Core + Npgsql]
        IDSDK[Azure.Identity]
    end

    IEC -.-> EHCS
    ITS -.-> DLSS
    IDMR -.-> DMR

    EHCS --> EHSDK
    EHCS --> IDSDK
    DLSS --> BLOBSDK
    DLSS --> IDSDK
    DMR --> EFSDK
    DMR --> IDSDK

    style EHCS fill:#e1f5ff
    style DLSS fill:#e1f5ff
    style DMR fill:#e1f5ff
```

**Principles:**
- ✅ Implements application ports
- ✅ Contains all Azure SDK code
- ✅ Managed identity for authentication
- ✅ Resilience with Polly policies

---

## Medallion Architecture

```mermaid
graph LR
    subgraph "Bronze - Raw Data"
        B1[Raw Telemetry<br/>As-Received]
        B2[Partitioned by<br/>yyyy/MM/dd/HH]
        B3[JSON Format<br/>Audit Trail]
    end

    subgraph "Silver - Validated & Enriched"
        S1[Validated Telemetry<br/>Quality Checked]
        S2[Enriched with<br/>Device Metadata]
        S3[Ready for Analytics]
    end

    subgraph "Gold - Aggregated"
        G1[Hourly Aggregates<br/>Future Implementation]
        G2[Avg, Min, Max, Count<br/>per Device/Metric]
        G3[Optimized for<br/>BI Tools]
    end

    B1 --> S1
    S1 --> G1

    style B1 fill:#cd7f32,color:#fff
    style B2 fill:#cd7f32,color:#fff
    style B3 fill:#cd7f32,color:#fff
    style S1 fill:#c0c0c0
    style S2 fill:#c0c0c0
    style S3 fill:#c0c0c0
    style G1 fill:#ffd700
    style G2 fill:#ffd700
    style G3 fill:#ffd700
```

### Layer Details

| Layer | Purpose | Format | Retention | Use Case |
|-------|---------|--------|-----------|----------|
| **Bronze** | Landing zone, audit trail | Raw JSON | 365 days | Compliance, reprocessing |
| **Silver** | Curated, business-ready | Enriched JSON | 180 days | Analytics, ML training |
| **Gold** | Aggregated, optimized | Aggregates JSON | 730 days | BI dashboards, reporting |

---

## CQRS Pattern with Wolverine

```mermaid
graph TB
    subgraph "Wolverine Message Bus"
        direction TB
        MB[Message Bus<br/>In-Process]

        subgraph "Command Pipeline"
            PC[Publish Command]
            ROUTE[Route to Handler]
            EXEC[Execute Handler]
            CASCADE[Cascade Commands]
        end

        subgraph "Event Pipeline"
            PE[Publish Event]
            NOTIFY[Notify Subscribers]
        end
    end

    subgraph "Commands (Write)"
        CMD1[ProcessTelemetryCommand]
        CMD2[ValidateTelemetryCommand]
        CMD3[EnrichTelemetryCommand]
        CMD4[StoreTelemetryCommand]
    end

    subgraph "Events (Notifications)"
        EVT1[TelemetryValidatedEvent]
        EVT2[TelemetryEnrichedEvent]
    end

    subgraph "Handlers"
        H1[ProcessTelemetryHandler]
        H2[ValidateTelemetryHandler]
        H3[EnrichTelemetryHandler]
        H4[StoreTelemetryHandler]
    end

    CMD1 --> PC
    PC --> ROUTE
    ROUTE --> H1
    H1 --> EXEC
    EXEC --> CASCADE
    CASCADE --> CMD2

    H2 --> PE
    PE --> EVT1

    style MB fill:#fff4e6
    style PC fill:#e3f2fd
    style PE fill:#f3e5f5
```

**Wolverine Features Used:**
- ✅ Source-generated handlers (zero reflection)
- ✅ Automatic command routing
- ✅ Cascading messages (return next command)
- ✅ Event publishing
- ✅ Built-in OpenTelemetry

---

## Sequence Diagrams

### Success Path: Valid Telemetry

```mermaid
sequenceDiagram
    autonumber
    participant IoT as IoT Device
    participant EH as Event Hubs
    participant EHCS as EventHubConsumer
    participant WB as Wolverine
    participant Bronze as Bronze Layer
    participant Val as Validator
    participant Enrich as Enricher
    participant Silver as Silver Layer

    IoT->>EH: Send telemetry (MQTT/AMQP)
    EH->>EHCS: Deliver EventData
    EHCS->>EHCS: Deserialize to TelemetryReading
    EHCS->>WB: ProcessTelemetryCommand

    WB->>Bronze: Write raw JSON
    Bronze-->>WB: Success
    WB->>Val: ValidateTelemetryCommand

    Val->>Val: Check quality, age, count
    Val-->>WB: Valid ✓
    WB->>Enrich: EnrichTelemetryCommand

    Enrich->>Enrich: Lookup device metadata
    Enrich-->>WB: Metadata added
    WB->>Silver: Write enriched JSON
    Silver-->>WB: Success

    WB->>EHCS: Pipeline complete
    EHCS->>EH: Update checkpoint
```

### Error Path: Invalid Telemetry

```mermaid
sequenceDiagram
    autonumber
    participant IoT as IoT Device
    participant EH as Event Hubs
    participant EHCS as EventHubConsumer
    participant WB as Wolverine
    participant Bronze as Bronze Layer
    participant Val as Validator

    IoT->>EH: Send telemetry (bad quality)
    EH->>EHCS: Deliver EventData
    EHCS->>EHCS: Deserialize to TelemetryReading
    EHCS->>WB: ProcessTelemetryCommand

    WB->>Bronze: Write raw JSON
    Note over Bronze: Still persisted<br/>for audit
    Bronze-->>WB: Success
    WB->>Val: ValidateTelemetryCommand

    Val->>Val: Check quality, age, count
    Val-->>WB: Invalid ✗ (bad quality)
    Note over WB: Pipeline stops<br/>No enrichment<br/>No silver write

    WB->>EHCS: Pipeline complete
    EHCS->>EH: Update checkpoint
```

---

## Summary

The TelemetryProcessor service implements a **clean architecture** with clear separation of concerns:

1. **Domain Layer** - Pure business logic, no infrastructure
2. **Application Layer** - Use cases and ports (interfaces)
3. **Infrastructure Layer** - Azure SDK adapters
4. **Host Layer** - Composition root and configuration

Key architectural patterns:
- ✅ **Hexagonal Architecture** - Ports & Adapters for testability
- ✅ **CQRS** - Commands for writes, Events for notifications
- ✅ **Medallion Architecture** - Bronze/Silver/Gold data layers
- ✅ **Domain-Driven Design** - Rich domain models with behavior
- ✅ **Event-Driven** - Domain events for observability

This design ensures:
- **Testability** - Mock ports, test handlers in isolation
- **Maintainability** - Clear boundaries, single responsibility
- **Scalability** - Stateless, horizontally scalable
- **Observability** - OpenTelemetry tracing through entire pipeline
