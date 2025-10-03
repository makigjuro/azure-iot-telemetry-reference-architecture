# IoT Hub + Device Provisioning Service Module
# Cost: B1 Basic tier ~$10/month (400,000 messages/day)

resource "azurerm_iothub" "main" {
  name                = "iot-${var.naming_prefix}"
  resource_group_name = var.resource_group_name
  location            = var.location

  # B1 Basic tier - cheapest paid option
  # 400,000 messages/day, device-to-cloud telemetry
  sku {
    name     = "B1"
    capacity = 1
  }

  # System-assigned managed identity for Event Hubs, Key Vault access
  identity {
    type = "SystemAssigned"
  }

  # Enable file upload (optional, uses storage account)
  file_upload {
    connection_string  = var.storage_connection_string
    container_name     = "device-uploads"
    notifications      = true
    max_delivery_count = 10
    default_ttl        = "PT1H"
  }

  # Network configuration
  public_network_access_enabled = var.enable_public_access
  min_tls_version               = "1.2"

  tags = var.tags
}

# Built-in Event Hub endpoint (for routing to Event Hubs)
# This is already included in IoT Hub, but we can configure routes

resource "azurerm_iothub_route" "telemetry_to_eventhub" {
  resource_group_name = var.resource_group_name
  iothub_name         = azurerm_iothub.main.name
  name                = "TelemetryToEventHub"

  source         = "DeviceMessages"
  condition      = "true"  # Route all messages
  endpoint_names = [azurerm_iothub_endpoint_eventhub.telemetry.name]
  enabled        = true
}

# Custom endpoint: Event Hub for telemetry
resource "azurerm_iothub_endpoint_eventhub" "telemetry" {
  resource_group_name = var.resource_group_name
  iothub_name         = azurerm_iothub.main.name
  name                = "telemetry-endpoint"

  # Use managed identity for authentication (no connection string needed!)
  authentication_type     = "identityBased"
  identity_id             = azurerm_iothub.main.identity[0].principal_id
  endpoint_uri            = var.eventhub_endpoint_uri
  entity_path             = var.eventhub_name
}

# Fallback route (to built-in endpoint)
resource "azurerm_iothub_fallback_route" "fallback" {
  resource_group_name = var.resource_group_name
  iothub_name         = azurerm_iothub.main.name

  enabled = true
  source  = "DeviceMessages"
}

# Device Provisioning Service (DPS)
resource "azurerm_iothub_dps" "main" {
  name                = "dps-${var.naming_prefix}"
  resource_group_name = var.resource_group_name
  location            = var.location

  # S1 tier - only option available
  sku {
    name     = "S1"
    capacity = 1
  }

  # Link to IoT Hub
  linked_hub {
    connection_string       = "HostName=${azurerm_iothub.main.hostname};SharedAccessKeyName=iothubowner;SharedAccessKey=${azurerm_iothub.main.shared_access_policy[0].primary_key}"
    location                = var.location
    allocation_weight       = 1
    apply_allocation_policy = true
  }

  # Network configuration
  public_network_access_enabled = var.enable_public_access

  tags = var.tags
}

# Store IoT Hub connection string in Key Vault
resource "azurerm_key_vault_secret" "iothub_connection_string" {
  name         = "iothub-connection-string"
  value        = "HostName=${azurerm_iothub.main.hostname};SharedAccessKeyName=iothubowner;SharedAccessKey=${azurerm_iothub.main.shared_access_policy[0].primary_key}"
  key_vault_id = var.key_vault_id

  depends_on = [var.key_vault_id]
}

# Store DPS connection string in Key Vault
resource "azurerm_key_vault_secret" "dps_connection_string" {
  name         = "dps-connection-string"
  value        = "HostName=${azurerm_iothub_dps.main.device_provisioning_host_name};SharedAccessKeyName=provisioningserviceowner;SharedAccessKey=${azurerm_iothub_dps.main.service_operations_host_name}"
  key_vault_id = var.key_vault_id

  depends_on = [var.key_vault_id]
}

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "iothub" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-iothub"
  target_resource_id         = azurerm_iothub.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "Connections"
  }

  enabled_log {
    category = "DeviceTelemetry"
  }

  enabled_log {
    category = "C2DCommands"
  }

  enabled_log {
    category = "DeviceIdentityOperations"
  }

  metric {
    category = "AllMetrics"
  }
}
