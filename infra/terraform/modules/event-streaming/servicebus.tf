# Service Bus for Stream Analytics alerts output
# Note: This could be in a separate module, but keeping it here for simplicity

resource "azurerm_servicebus_namespace" "alerts" {
  name                = "sb-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Basic tier - cheapest (~$0.05 per million operations)
  sku = "Basic"

  tags = var.tags
}

resource "azurerm_servicebus_queue" "stream_alerts" {
  name         = "stream-alerts"
  namespace_id = azurerm_servicebus_namespace.alerts.id

  # Basic tier limits
  max_size_in_megabytes = 1024
  default_message_ttl   = "PT1H"  # 1 hour
}

# Store Service Bus connection string in Key Vault
resource "azurerm_key_vault_secret" "servicebus_connection_string" {
  name         = "servicebus-connection-string"
  value        = azurerm_servicebus_namespace.alerts.default_primary_connection_string
  key_vault_id = var.key_vault_id

  depends_on = [var.key_vault_id]
}
