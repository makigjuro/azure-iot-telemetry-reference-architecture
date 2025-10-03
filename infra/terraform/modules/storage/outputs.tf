output "storage_account_id" {
  description = "Storage Account ID"
  value       = azurerm_storage_account.adls.id
}

output "storage_account_name" {
  description = "Storage Account name"
  value       = azurerm_storage_account.adls.name
}

output "storage_account_primary_access_key" {
  description = "Storage Account primary access key"
  value       = azurerm_storage_account.adls.primary_access_key
  sensitive   = true
}

output "storage_account_primary_connection_string" {
  description = "Storage Account primary connection string"
  value       = azurerm_storage_account.adls.primary_connection_string
  sensitive   = true
}

output "storage_account_primary_blob_endpoint" {
  description = "Storage Account primary blob endpoint"
  value       = azurerm_storage_account.adls.primary_blob_endpoint
}

output "storage_account_primary_dfs_endpoint" {
  description = "Storage Account primary Data Lake endpoint"
  value       = azurerm_storage_account.adls.primary_dfs_endpoint
}

output "container_raw_name" {
  description = "Raw container name"
  value       = azurerm_storage_container.raw.name
}

output "container_bronze_name" {
  description = "Bronze container name"
  value       = azurerm_storage_container.bronze.name
}

output "container_silver_name" {
  description = "Silver container name"
  value       = azurerm_storage_container.silver.name
}

output "container_gold_name" {
  description = "Gold container name"
  value       = azurerm_storage_container.gold.name
}

output "container_hotpath_name" {
  description = "Hot path container name"
  value       = azurerm_storage_container.hotpath.name
}
