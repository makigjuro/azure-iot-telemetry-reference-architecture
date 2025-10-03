output "iothub_id" {
  description = "IoT Hub ID"
  value       = azurerm_iothub.main.id
}

output "iothub_name" {
  description = "IoT Hub name"
  value       = azurerm_iothub.main.name
}

output "iothub_hostname" {
  description = "IoT Hub hostname"
  value       = azurerm_iothub.main.hostname
}

output "iothub_principal_id" {
  description = "IoT Hub managed identity principal ID"
  value       = azurerm_iothub.main.identity[0].principal_id
}

output "iothub_connection_string" {
  description = "IoT Hub owner connection string"
  value       = "HostName=${azurerm_iothub.main.hostname};SharedAccessKeyName=iothubowner;SharedAccessKey=${azurerm_iothub.main.shared_access_policy[0].primary_key}"
  sensitive   = true
}

output "dps_id" {
  description = "Device Provisioning Service ID"
  value       = azurerm_iothub_dps.main.id
}

output "dps_name" {
  description = "Device Provisioning Service name"
  value       = azurerm_iothub_dps.main.name
}

output "dps_hostname" {
  description = "Device Provisioning Service hostname"
  value       = azurerm_iothub_dps.main.device_provisioning_host_name
}

output "dps_id_scope" {
  description = "Device Provisioning Service ID Scope"
  value       = azurerm_iothub_dps.main.id_scope
}
