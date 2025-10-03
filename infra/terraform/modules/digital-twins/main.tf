# Azure Digital Twins Module
# Cost: Pay-as-you-go ~$5/month for low usage (charged per API call)

resource "azurerm_digital_twins_instance" "main" {
  name                = "dt-${var.naming_prefix}"
  resource_group_name = var.resource_group_name
  location            = var.location

  # System-assigned managed identity
  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Event Grid endpoint for twin change events
resource "azurerm_digital_twins_endpoint_eventgrid" "twin_changes" {
  name                                 = "twin-changes-endpoint"
  digital_twins_id                     = azurerm_digital_twins_instance.main.id
  eventgrid_topic_endpoint             = var.eventgrid_topic_endpoint
  eventgrid_topic_primary_access_key   = var.eventgrid_topic_primary_key
  eventgrid_topic_secondary_access_key = var.eventgrid_topic_secondary_key
}

# Data history connection to ADLS (optional, for archiving twin changes)
# Note: This requires Azure Digital Twins premium tier, commenting out for cost savings
# resource "azurerm_digital_twins_time_series_database_connection" "adls" {
#   name                 = "adls-connection"
#   digital_twins_id     = azurerm_digital_twins_instance.main.id
#   adx_database_name    = "iot_twins"
#   adx_endpoint_uri     = var.adls_endpoint_uri
#   adx_resource_id      = var.adls_resource_id
#   eventhub_consumer_group = "digital-twins"
#   eventhub_endpoint_uri = var.eventhub_endpoint_uri
#   eventhub_namespace_endpoint_uri = var.eventhub_namespace_endpoint_uri
# }

# RBAC: Grant IoT Hub permission to update digital twins
resource "azurerm_role_assignment" "iothub_to_dt" {
  count                = var.iothub_principal_id != null ? 1 : 0
  scope                = azurerm_digital_twins_instance.main.id
  role_definition_name = "Azure Digital Twins Data Owner"
  principal_id         = var.iothub_principal_id
}

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "digital_twins" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-digital-twins"
  target_resource_id         = azurerm_digital_twins_instance.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "DigitalTwinsOperation"
  }

  enabled_log {
    category = "EventRoutesOperation"
  }

  enabled_log {
    category = "ModelsOperation"
  }

  enabled_log {
    category = "QueryOperation"
  }

  metric {
    category = "AllMetrics"
  }
}
