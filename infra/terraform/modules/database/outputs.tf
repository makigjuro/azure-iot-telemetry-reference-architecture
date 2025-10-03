output "postgres_server_id" {
  description = "PostgreSQL server ID"
  value       = azurerm_postgresql_flexible_server.main.id
}

output "postgres_server_name" {
  description = "PostgreSQL server name"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "postgres_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgres_database_name" {
  description = "PostgreSQL database name"
  value       = azurerm_postgresql_flexible_server_database.iot_metadata.name
}

output "postgres_connection_string" {
  description = "PostgreSQL connection string"
  value       = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=iot_metadata;Username=${var.admin_username};SslMode=Require"
  sensitive   = true
}
