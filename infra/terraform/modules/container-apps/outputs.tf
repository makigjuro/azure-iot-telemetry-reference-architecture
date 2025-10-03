output "container_app_environment_id" {
  description = "Container Apps Environment ID"
  value       = azurerm_container_app_environment.main.id
}

output "container_app_environment_name" {
  description = "Container Apps Environment name"
  value       = azurerm_container_app_environment.main.name
}

output "telemetry_processor_id" {
  description = "Telemetry Processor container app ID"
  value       = azurerm_container_app.telemetry_processor.id
}

output "telemetry_processor_principal_id" {
  description = "Telemetry Processor managed identity principal ID"
  value       = azurerm_container_app.telemetry_processor.identity[0].principal_id
}

output "alert_handler_id" {
  description = "Alert Handler container app ID"
  value       = azurerm_container_app.alert_handler.id
}

output "alert_handler_principal_id" {
  description = "Alert Handler managed identity principal ID"
  value       = azurerm_container_app.alert_handler.identity[0].principal_id
}

output "event_subscriber_id" {
  description = "Event Subscriber container app ID"
  value       = azurerm_container_app.event_subscriber.id
}

output "event_subscriber_principal_id" {
  description = "Event Subscriber managed identity principal ID"
  value       = azurerm_container_app.event_subscriber.identity[0].principal_id
}

output "event_subscriber_fqdn" {
  description = "Event Subscriber FQDN (for Event Grid webhook)"
  value       = azurerm_container_app.event_subscriber.ingress[0].fqdn
}
