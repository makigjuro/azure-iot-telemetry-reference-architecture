# Event Streaming Module - Event Hubs + Event Grid
# Cost: Basic tier ~$11/month (1 Throughput Unit, 1M events/day)

# Event Hubs Namespace
resource "azurerm_eventhub_namespace" "main" {
  name                = "evhns-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Basic tier - cheapest option (1 TU)
  # 1 MB/s ingress, 2 MB/s egress, 1 day retention
  sku                      = "Basic"
  capacity                 = 1
  auto_inflate_enabled     = false

  # System-assigned managed identity
  identity {
    type = "SystemAssigned"
  }

  # Network configuration
  public_network_access_enabled = var.enable_public_access
  minimum_tls_version           = "1.2"

  tags = var.tags
}

# Event Hub: Telemetry (from IoT Hub)
resource "azurerm_eventhub" "telemetry" {
  name                = "telemetry"
  namespace_name      = azurerm_eventhub_namespace.main.name
  resource_group_name = var.resource_group_name

  # Basic tier: 1 day retention (max for Basic)
  partition_count   = 2  # Minimum for Basic
  message_retention = 1  # Days
}

# Consumer Group: Stream Analytics
resource "azurerm_eventhub_consumer_group" "stream_analytics" {
  name                = "stream-analytics"
  namespace_name      = azurerm_eventhub_namespace.main.name
  eventhub_name       = azurerm_eventhub.telemetry.name
  resource_group_name = var.resource_group_name
}

# Consumer Group: Container Apps (Cold Path)
resource "azurerm_eventhub_consumer_group" "container_apps" {
  name                = "container-apps"
  namespace_name      = azurerm_eventhub_namespace.main.name
  eventhub_name       = azurerm_eventhub.telemetry.name
  resource_group_name = var.resource_group_name
}

# Event Grid System Topic (for IoT Hub device lifecycle events)
resource "azurerm_eventgrid_system_topic" "iothub" {
  name                   = "evgt-iothub-${var.naming_prefix}"
  location               = var.location
  resource_group_name    = var.resource_group_name
  source_arm_resource_id = var.iothub_id
  topic_type             = "Microsoft.Devices.IoTHubs"

  # System-assigned managed identity
  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Event Grid Subscription: Device Lifecycle Events
resource "azurerm_eventgrid_system_topic_event_subscription" "device_lifecycle" {
  name                = "device-lifecycle"
  system_topic        = azurerm_eventgrid_system_topic.iothub.name
  resource_group_name = var.resource_group_name

  # Filter for device created/deleted/connected events
  included_event_types = [
    "Microsoft.Devices.DeviceCreated",
    "Microsoft.Devices.DeviceDeleted",
    "Microsoft.Devices.DeviceConnected",
    "Microsoft.Devices.DeviceDisconnected"
  ]

  # Endpoint: Storage Queue (cheapest option for dev)
  # In production, use Azure Function or Container Apps webhook
  storage_queue_endpoint {
    storage_account_id = var.storage_account_id
    queue_name         = "device-events"
  }

  retry_policy {
    max_delivery_attempts = 10
    event_time_to_live    = 1440  # 24 hours
  }
}

# Store Event Hubs connection string in Key Vault
resource "azurerm_key_vault_secret" "eventhub_connection_string" {
  name         = "eventhub-connection-string"
  value        = azurerm_eventhub_namespace.main.default_primary_connection_string
  key_vault_id = var.key_vault_id

  depends_on = [var.key_vault_id]
}

# Diagnostic settings for Event Hub Namespace
resource "azurerm_monitor_diagnostic_setting" "eventhub_namespace" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-eventhub"
  target_resource_id         = azurerm_eventhub_namespace.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "ArchiveLogs"
  }

  enabled_log {
    category = "OperationalLogs"
  }

  metric {
    category = "AllMetrics"
  }
}

# Diagnostic settings for Event Grid System Topic
resource "azurerm_monitor_diagnostic_setting" "eventgrid" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-eventgrid"
  target_resource_id         = azurerm_eventgrid_system_topic.iothub.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "DeliveryFailures"
  }

  metric {
    category = "AllMetrics"
  }
}
