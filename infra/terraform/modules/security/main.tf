# Security Module - Key Vault
# Cost: Standard tier ~$0.03 per 10,000 operations (no fixed cost)

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "main" {
  name                       = "kv-${var.naming_prefix}"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id

  # Standard tier (cheapest, no HSM)
  sku_name                   = "standard"

  # Soft delete (enabled by default, keeps deleted secrets for 7-90 days)
  soft_delete_retention_days = 7  # Minimum retention

  # Purge protection (optional, disable for dev to allow immediate deletion)
  purge_protection_enabled   = var.enable_purge_protection

  # RBAC for access (modern approach, recommended)
  enable_rbac_authorization  = true

  # Network ACLs
  network_acls {
    default_action             = var.enable_public_access ? "Allow" : "Deny"
    bypass                     = "AzureServices"
    ip_rules                   = var.allowed_ip_addresses
    virtual_network_subnet_ids = var.allowed_subnet_ids
  }

  tags = var.tags
}

# Grant current user/service principal access (for Terraform to create secrets)
resource "azurerm_role_assignment" "terraform_secrets_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Example secrets (will be populated by other modules)
# Secrets should be created by the modules that generate them (IoT Hub, PostgreSQL, etc.)

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "keyvault" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-keyvault"
  target_resource_id         = azurerm_key_vault.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "AuditEvent"
  }

  metric {
    category = "AllMetrics"
  }
}
