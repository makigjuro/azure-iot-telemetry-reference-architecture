#!/usr/bin/env python3
"""
Generate Azure IoT Architecture Diagram using Python Diagrams library
with official Azure icons.

Requirements:
    pip install diagrams

Generates: architecture_diagram.png
"""

from diagrams import Diagram, Cluster, Edge
from diagrams.azure.iot import IotHub, DeviceProvisioningServices, DigitalTwins
from diagrams.azure.integration import EventGridDomains
from diagrams.azure.analytics import EventHubs, StreamAnalyticsJobs, SynapseAnalytics
from diagrams.azure.compute import ContainerInstances, VM
from diagrams.azure.database import DatabaseForPostgresqlServers
from diagrams.azure.storage import BlobStorage, DataLakeStorage
from diagrams.azure.security import KeyVaults
from diagrams.azure.identity import ManagedIdentities
from diagrams.azure.devops import ApplicationInsights
from diagrams.azure.monitor import Monitor
from diagrams.azure.network import VirtualNetworks, PrivateEndpoint

# Custom attributes
graph_attr = {
    "fontsize": "14",
    "bgcolor": "white",
    "pad": "0.5",
}

with Diagram(
    "Azure IoT Telemetry Reference Architecture",
    filename="docs/architecture_diagram",
    show=False,
    direction="LR",
    graph_attr=graph_attr,
):
    # Edge/On-Premises
    with Cluster("Edge/On-Premises"):
        iot_devices = IotHub("IoT Devices\nMQTT/AMQP")
        iot_edge = IotHub("IoT Edge\nGateway")

    # Azure Cloud
    with Cluster("Azure Cloud"):
        # IoT & Device Management
        with Cluster("IoT & Device Management"):
            iothub = IotHub("Azure IoT Hub\nD2C/C2D/Twins")
            dps = DeviceProvisioningServices("Device Provisioning\nService")
            digital_twins = DigitalTwins("Azure Digital Twins\nDTDL Models")

        # Virtual Network
        with Cluster("Virtual Network"):
            with Cluster("Subnet: Application"):
                aca = ContainerInstances("Azure Container Apps\nWorkers & APIs")
                pe_iothub = PrivateEndpoint("PE: IoT Hub")
                pe_eventhub = PrivateEndpoint("PE: Event Hubs")
                pe_keyvault = PrivateEndpoint("PE: Key Vault")
                pe_adls = PrivateEndpoint("PE: Data Lake")
                pe_synapse = PrivateEndpoint("PE: Synapse")

            with Cluster("Subnet: Management"):
                vm = VM("Virtual Machines\nManagement")
                bastion = VirtualNetworks("Azure Bastion")

            with Cluster("Subnet: Data"):
                postgres = DatabaseForPostgresqlServers("PostgreSQL\nFlexible Server")

        # Ingestion & Events
        with Cluster("Ingestion & Event Layer"):
            event_grid = EventGridDomains("Event Grid\nCloudEvents")
            event_hubs = EventHubs("Event Hubs\nTelemetry Stream")
            stream_analytics = StreamAnalyticsJobs("Stream Analytics\nHot Path")

        # Storage
        with Cluster("Storage Layer"):
            adls = DataLakeStorage("Data Lake Gen2\nRaw/Bronze/Silver/Gold")
            blob = BlobStorage("Blob Storage")

        # Analytics
        with Cluster("Analytics Platform"):
            synapse = SynapseAnalytics("Synapse Analytics\nETL/Pipelines")
            fabric = SynapseAnalytics("Microsoft Fabric\nLakehouse")

        # Security
        with Cluster("Security & Identity"):
            keyvault = KeyVaults("Key Vault\nSecrets/Certs")
            managed_id = ManagedIdentities("Entra ID\nManaged Identities")

        # Monitoring
        with Cluster("Monitoring"):
            log_analytics = Monitor("Log Analytics\nWorkspace")
            app_insights = ApplicationInsights("Application Insights\nAPM")

    # Device Connectivity
    iot_devices >> Edge(label="MQTT/AMQP") >> iot_edge
    iot_edge >> Edge(label="Telemetry") >> iothub
    iot_devices >> Edge(label="Direct") >> iothub
    dps >> Edge(label="Provision") >> iothub

    # Hot Path (Real-time)
    iothub >> Edge(label="Built-in Endpoint") >> event_hubs
    event_hubs >> Edge(label="Real-time") >> stream_analytics
    stream_analytics >> Edge(label="Alerts") >> aca
    stream_analytics >> Edge(label="Hot Path") >> adls

    # Cold Path (Batch)
    event_hubs >> Edge(label="Batch") >> aca
    aca >> Edge(label="via PE", style="dashed") >> pe_adls >> adls
    aca >> Edge(label="Metadata", style="dashed") >> postgres

    # Device Management
    iothub >> Edge(label="Twin Updates", style="dotted") >> digital_twins
    aca >> Edge(label="C2D", style="dotted") >> iothub
    vm >> Edge(label="Manage", style="dotted") >> iothub

    # Analytics Pipeline
    adls >> Edge(label="Source") >> synapse
    synapse >> Edge(label="Datasets") >> fabric
    digital_twins >> Edge(label="Context", style="dotted") >> synapse

    # Event-Driven
    iothub >> Edge(label="Events") >> event_grid
    event_grid >> Edge(label="Trigger") >> aca

    # Security & Identity
    aca >> Edge(label="MSI", style="dashed") >> managed_id
    iothub >> Edge(label="MSI", style="dashed") >> managed_id
    stream_analytics >> Edge(label="MSI", style="dashed") >> managed_id
    synapse >> Edge(label="MSI", style="dashed") >> managed_id

    aca >> Edge(label="Secrets", style="dashed") >> pe_keyvault >> keyvault

    # Monitoring
    aca >> Edge(label="Logs", style="dotted") >> log_analytics
    iothub >> Edge(label="Logs", style="dotted") >> log_analytics
    aca >> Edge(label="Traces", style="dotted") >> app_insights
    app_insights >> Edge(label="Analytics") >> log_analytics

    # Network Management
    bastion >> Edge(label="Admin", style="dashed") >> vm

print("✅ Diagram generated: docs/architecture_diagram.png")
print("   Install: pip install diagrams")
print("   Run: python docs/generate_diagram.py")
