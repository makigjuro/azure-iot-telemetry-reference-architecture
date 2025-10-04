output "eventhub_namespace_id" {
  description = "Event Hub Namespace ID"
  value       = azurerm_eventhub_namespace.main.id
}

output "eventhub_namespace_name" {
  description = "Event Hub Namespace name"
  value       = azurerm_eventhub_namespace.main.name
}

output "eventhub_namespace_endpoint" {
  description = "Event Hub Namespace endpoint URI"
  value       = "sb://${azurerm_eventhub_namespace.main.name}.servicebus.windows.net/"
}

output "eventhub_namespace_principal_id" {
  description = "Event Hub Namespace managed identity principal ID"
  value       = azurerm_eventhub_namespace.main.identity[0].principal_id
}

output "eventhub_telemetry_name" {
  description = "Telemetry Event Hub name"
  value       = azurerm_eventhub.telemetry.name
}

output "eventhub_telemetry_id" {
  description = "Telemetry Event Hub ID"
  value       = azurerm_eventhub.telemetry.id
}

output "eventhub_connection_string" {
  description = "Event Hub Namespace connection string"
  value       = azurerm_eventhub_namespace.main.default_primary_connection_string
  sensitive   = true
}

output "consumer_group_stream_analytics" {
  description = "Stream Analytics consumer group name"
  value       = azurerm_eventhub_consumer_group.stream_analytics.name
}

output "consumer_group_container_apps" {
  description = "Container Apps consumer group name"
  value       = azurerm_eventhub_consumer_group.container_apps.name
}

output "servicebus_namespace_id" {
  description = "Service Bus Namespace ID"
  value       = azurerm_servicebus_namespace.alerts.id
}

output "servicebus_namespace_name" {
  description = "Service Bus Namespace name"
  value       = azurerm_servicebus_namespace.alerts.name
}

output "servicebus_connection_string" {
  description = "Service Bus connection string"
  value       = azurerm_servicebus_namespace.alerts.default_primary_connection_string
  sensitive   = true
}

output "servicebus_queue_name" {
  description = "Service Bus alerts queue name"
  value       = azurerm_servicebus_queue.stream_alerts.name
}
