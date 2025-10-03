output "vnet_id" {
  description = "Virtual Network ID"
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Virtual Network name"
  value       = azurerm_virtual_network.main.name
}

output "subnet_management_id" {
  description = "Management subnet ID"
  value       = azurerm_subnet.management.id
}

output "subnet_application_id" {
  description = "Application subnet ID"
  value       = azurerm_subnet.application.id
}

output "subnet_data_id" {
  description = "Data subnet ID"
  value       = azurerm_subnet.data.id
}

output "nsg_management_id" {
  description = "Management NSG ID"
  value       = azurerm_network_security_group.management.id
}

output "nsg_application_id" {
  description = "Application NSG ID"
  value       = azurerm_network_security_group.application.id
}

output "nsg_data_id" {
  description = "Data NSG ID"
  value       = azurerm_network_security_group.data.id
}
