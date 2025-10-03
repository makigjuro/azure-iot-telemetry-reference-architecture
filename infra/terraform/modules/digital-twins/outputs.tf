output "digital_twins_id" {
  description = "Azure Digital Twins instance ID"
  value       = azurerm_digital_twins_instance.main.id
}

output "digital_twins_name" {
  description = "Azure Digital Twins instance name"
  value       = azurerm_digital_twins_instance.main.name
}

output "digital_twins_hostname" {
  description = "Azure Digital Twins hostname"
  value       = azurerm_digital_twins_instance.main.host_name
}

output "digital_twins_principal_id" {
  description = "Azure Digital Twins managed identity principal ID"
  value       = azurerm_digital_twins_instance.main.identity[0].principal_id
}
