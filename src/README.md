# Azure IoT Telemetry - Backend Services

.NET 9 microservices implementing Hexagonal Architecture and Domain-Driven Design.

## Architecture

### Solution Structure
```
src/
├── IoTTelemetry.sln
├── Shared/
│   ├── IoTTelemetry.Domain/                   # Pure domain logic (no dependencies)
│   └── IoTTelemetry.Shared.Infrastructure/    # Common infrastructure components
└── Services/                                   # Microservices (to be added)
    ├── TelemetryProcessor/                    # Cold path processing
    ├── AlertHandler/                          # Hot path alerts
    └── EventSubscriber/                       # Device lifecycle events
```

### Patterns
- **Hexagonal Architecture** - Ports & Adapters
- **Domain-Driven Design** - Entities, Value Objects, Aggregates
- **CQRS** - Command/Query separation with Wolverine
- **Event-Driven** - Domain events for loose coupling

## Tech Stack

- **.NET 9** + C# 13
- **Wolverine** - Messaging & CQRS
- **FluentValidation** - Input validation
- **Mapperly** - Compile-time object mapping
- **EF Core 9** + **Dapper** - PostgreSQL (write/read models)
- **Polly v8** - Resilience
- **OpenTelemetry** + **Serilog** - Observability
- **Azure SDK** - Event Hubs, IoT Hub, Digital Twins, Storage

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop

### Local Development

1. **Start dependencies**
   ```bash
   docker-compose up -d
   ```

   Services:
   - PostgreSQL: `localhost:5432` (user: `iotuser`, pass: `iotpass123!`)
   - Azurite (ADLS emulator): `localhost:10000-10002`
   - Seq (logs): http://localhost:5341
   - pgAdmin: http://localhost:5050 (user: `admin@iot.local`, pass: `admin123!`)

2. **Build solution**
   ```bash
   dotnet build
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

### Configuration

- **Directory.Build.props** - Global project settings
  - Nullable reference types enabled
  - C# 13 language version
  - Code analyzers (Roslynator, SonarAnalyzer)

- **Directory.Packages.props** - Central package management
  - All NuGet packages with version pinning

- **.editorconfig** - Code formatting rules
  - Consistent style across team
  - Enforced in build

## Development Guidelines

### Domain Layer
- Pure C# - no external dependencies
- Rich domain models with business logic
- Value objects are immutable
- Entities protect invariants

### Application Layer
- Use cases orchestrate domain logic
- Port interfaces (repository, external services)
- Wolverine handlers for commands/queries

### Infrastructure Layer
- Adapters implement ports
- Azure SDK integration
- Polly resilience policies
- EF Core + Dapper repositories

### Testing
- Unit tests for domain & application
- Integration tests with Testcontainers
- Architecture tests with NetArchTest
- Target: >80% code coverage

## Build & Test

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run with hot reload
dotnet watch run --project Services/EventSubscriber/EventSubscriber.Host
```

## Docker

```bash
# Start local environment
docker-compose up -d

# View logs
docker-compose logs -f

# Stop environment
docker-compose down

# Remove volumes (clean slate)
docker-compose down -v
```

## Next Steps

1. Implement domain models (#5)
2. Implement services (#6, #7, #8)
3. Add EF Core migrations (#11)
4. Set up CI/CD (#12)
