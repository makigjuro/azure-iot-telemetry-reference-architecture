# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/Directory.Packages.props", "src/"]
COPY ["src/IoTTelemetry.sln", "src/"]
COPY ["src/Shared/IoTTelemetry.Domain/IoTTelemetry.Domain.csproj", "src/Shared/IoTTelemetry.Domain/"]
COPY ["src/Shared/IoTTelemetry.Shared.Infrastructure/IoTTelemetry.Shared.Infrastructure.csproj", "src/Shared/IoTTelemetry.Shared.Infrastructure/"]
COPY ["src/Services/TelemetryProcessor/TelemetryProcessor.Infrastructure/TelemetryProcessor.Infrastructure.csproj", "src/Services/TelemetryProcessor/TelemetryProcessor.Infrastructure/"]
COPY ["src/Services/EventSubscriber/EventSubscriber.Application/EventSubscriber.Application.csproj", "src/Services/EventSubscriber/EventSubscriber.Application/"]
COPY ["src/Services/EventSubscriber/EventSubscriber.Infrastructure/EventSubscriber.Infrastructure.csproj", "src/Services/EventSubscriber/EventSubscriber.Infrastructure/"]
COPY ["src/Services/EventSubscriber/EventSubscriber.Host/EventSubscriber.Host.csproj", "src/Services/EventSubscriber/EventSubscriber.Host/"]

# Restore dependencies
RUN dotnet restore "src/Services/EventSubscriber/EventSubscriber.Host/EventSubscriber.Host.csproj"

# Copy source code
COPY src/ src/

# Build and publish
WORKDIR "/src/src/Services/EventSubscriber/EventSubscriber.Host"
RUN dotnet build "EventSubscriber.Host.csproj" -c Release -o /app/build
RUN dotnet publish "EventSubscriber.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r eventsubscriber && useradd -r -g eventsubscriber eventsubscriber

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R eventsubscriber:eventsubscriber /app

# Switch to non-root user
USER eventsubscriber

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EventSubscriber.Host.dll"]
