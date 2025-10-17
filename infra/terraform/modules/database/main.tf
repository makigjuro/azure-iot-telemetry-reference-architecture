# Database Module - PostgreSQL Flexible Server
# Cost: Burstable B1ms tier ~$12/month (1 vCore, 2GB RAM)

resource "azurerm_postgresql_flexible_server" "main" {
  name                = "psql-${var.naming_prefix}"
  resource_group_name = var.resource_group_name
  location            = var.location

  # Burstable B1ms - cheapest tier (1 vCore, 2GB RAM)
  sku_name   = "B_Standard_B1ms"
  storage_mb = 32768  # 32GB minimum

  # Availability zone (keep existing zone)
  zone = "2"

  # PostgreSQL version
  version = "16"

  # Administrator credentials
  administrator_login    = var.admin_username
  administrator_password = var.admin_password

  # VNet integration (delegated subnet)
  delegated_subnet_id = var.delegated_subnet_id
  private_dns_zone_id = azurerm_private_dns_zone.postgres.id

  # Disable public access when using VNet integration
  public_network_access_enabled = false

  # Backup configuration
  backup_retention_days        = 7   # Minimum
  geo_redundant_backup_enabled = false  # Disabled for cost savings

  # High availability (disabled for dev to save cost)
  # Note: Omit high_availability block entirely to disable

  # Maintenance window
  maintenance_window {
    day_of_week  = 0  # Sunday
    start_hour   = 2
    start_minute = 0
  }

  tags = var.tags

  depends_on = [azurerm_private_dns_zone_virtual_network_link.postgres]
}

# Private DNS Zone for PostgreSQL
resource "azurerm_private_dns_zone" "postgres" {
  name                = "privatelink.postgres.database.azure.com"
  resource_group_name = var.resource_group_name

  tags = var.tags
}

# Link Private DNS Zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  name                  = "pdns-link-postgres"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgres.name
  virtual_network_id    = var.virtual_network_id

  tags = var.tags
}

# Database: IoT Device Metadata
resource "azurerm_postgresql_flexible_server_database" "iot_metadata" {
  name      = "iot_metadata"
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# NOTE: Firewall rules are NOT compatible with VNet integration
# Since we use delegated_subnet_id for VNet integration, we cannot use firewall rules
# Access is controlled via VNet/subnet instead of public firewall rules

# Configuration: Enable pgcrypto extension (for UUIDs)
resource "azurerm_postgresql_flexible_server_configuration" "extensions" {
  name      = "azure.extensions"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = "PGCRYPTO,UUID-OSSP"
}

# Store connection string in Key Vault
resource "azurerm_key_vault_secret" "postgres_connection_string" {
  name         = "postgres-connection-string"
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=iot_metadata;Username=${var.admin_username};Password=${var.admin_password};SslMode=Require"
  key_vault_id = var.key_vault_id

  depends_on = [var.key_vault_id]
}

# Store admin password in Key Vault
resource "azurerm_key_vault_secret" "postgres_admin_password" {
  name         = "postgres-admin-password"
  value        = var.admin_password
  key_vault_id = var.key_vault_id

  depends_on = [var.key_vault_id]
}

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "postgres" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-postgres"
  target_resource_id         = azurerm_postgresql_flexible_server.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "PostgreSQLLogs"
  }

  metric {
    category = "AllMetrics"
  }
}
