# RBAC Module - Managed Identity Role Assignments
# Grants permissions for service-to-service authentication

# IoT Hub → Event Hubs (send device messages)
resource "azurerm_role_assignment" "iothub_to_eventhub" {
  scope                = var.eventhub_namespace_id
  role_definition_name = "Azure Event Hubs Data Sender"
  principal_id         = var.iothub_principal_id
}

# Stream Analytics → Event Hubs (consume telemetry)
resource "azurerm_role_assignment" "stream_analytics_to_eventhub" {
  scope                = var.eventhub_namespace_id
  role_definition_name = "Azure Event Hubs Data Receiver"
  principal_id         = var.stream_analytics_principal_id
}

# Stream Analytics → ADLS (write hot path data)
resource "azurerm_role_assignment" "stream_analytics_to_adls" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.stream_analytics_principal_id
}

# Container App (Telemetry Processor) → Event Hubs (consume telemetry - cold path)
resource "azurerm_role_assignment" "telemetry_processor_to_eventhub" {
  scope                = var.eventhub_namespace_id
  role_definition_name = "Azure Event Hubs Data Receiver"
  principal_id         = var.telemetry_processor_principal_id
}

# Container App (Telemetry Processor) → ADLS (write to bronze/silver/gold)
resource "azurerm_role_assignment" "telemetry_processor_to_adls" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.telemetry_processor_principal_id
}

# Container App (Telemetry Processor) → Key Vault (read secrets)
resource "azurerm_role_assignment" "telemetry_processor_to_kv" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.telemetry_processor_principal_id
}

# Container App (Alert Handler) → IoT Hub (send C2D messages)
resource "azurerm_role_assignment" "alert_handler_to_iothub" {
  scope                = var.iothub_id
  role_definition_name = "IoT Hub Registry Contributor"
  principal_id         = var.alert_handler_principal_id
}

# Container App (Alert Handler) → Key Vault (read secrets)
resource "azurerm_role_assignment" "alert_handler_to_kv" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.alert_handler_principal_id
}

# Container App (Event Subscriber) → Digital Twins (update twins)
resource "azurerm_role_assignment" "event_subscriber_to_dt" {
  scope                = var.digital_twins_id
  role_definition_name = "Azure Digital Twins Data Owner"
  principal_id         = var.event_subscriber_principal_id
}

# Container App (Event Subscriber) → Key Vault (read secrets)
resource "azurerm_role_assignment" "event_subscriber_to_kv" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.event_subscriber_principal_id
}

# Digital Twins → ADLS (future: read device data for context)
resource "azurerm_role_assignment" "digital_twins_to_adls" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = var.digital_twins_principal_id
}

# Event Grid System Topic → Storage Queue (for device lifecycle events)
resource "azurerm_role_assignment" "eventgrid_to_storage" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Queue Data Message Sender"
  principal_id         = var.eventgrid_principal_id
}
