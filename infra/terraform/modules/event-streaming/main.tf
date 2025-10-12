# Event Streaming Module - Event Hubs + Event Grid
# Cost: Standard tier ~$15/month (1 Throughput Unit, 1M events/day)

# Event Hubs Namespace
resource "azurerm_eventhub_namespace" "main" {
  name                = "evhns-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Standard tier - required for consumer groups (Stream Analytics needs this)
  # 1 MB/s ingress, 2 MB/s egress, 1 day retention
  sku                      = "Standard"
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

  # Standard tier: 1-7 days retention
  partition_count   = 2
  message_retention = 1  # Days (can increase up to 7 if needed)
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

# NOTE: Event Grid System Topic moved to main.tf to avoid circular dependency
# Event Grid System Topic requires IoT Hub ID, but IoT Hub requires Event Hub outputs
# Therefore, Event Grid is created after both modules in main.tf

# Store Event Hubs connection string in Key Vault
resource "azurerm_key_vault_secret" "eventhub_connection_string" {
  name         = "eventhub-connection-string"
  value        = "${azurerm_eventhub_namespace.main.default_primary_connection_string};EntityPath=${azurerm_eventhub.telemetry.name}"
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

# NOTE: Event Grid diagnostic settings also moved to main.tf
