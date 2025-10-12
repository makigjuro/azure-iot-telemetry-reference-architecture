# Storage Module - ADLS Gen2 with Medallion Architecture
# Cost: Standard LRS ~$0.018/GB/month + transaction costs

resource "azurerm_storage_account" "adls" {
  name                     = "st${replace(var.naming_prefix, "-", "")}adls"  # Must be globally unique, no hyphens
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"  # Cheapest tier
  account_replication_type = "LRS"       # Locally Redundant Storage (cheapest)
  account_kind             = "StorageV2"

  # Enable hierarchical namespace for Data Lake Gen2
  is_hns_enabled           = true

  # Disable features to save cost
  https_traffic_only_enabled      = true
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false

  # Network rules - restrict to VNet (or allow all for dev)
  network_rules {
    default_action             = var.enable_public_access ? "Allow" : "Deny"
    ip_rules                   = var.allowed_ip_addresses
    virtual_network_subnet_ids = var.allowed_subnet_ids
    bypass                     = ["AzureServices"]  # Allow Azure services
  }

  # Lifecycle management to reduce costs (auto-delete old data)
  blob_properties {
    delete_retention_policy {
      days = 7  # Keep deleted blobs for 7 days
    }

    container_delete_retention_policy {
      days = 7
    }
  }

  tags = var.tags
}

# Container: Raw (landing zone for raw telemetry)
resource "azurerm_storage_container" "raw" {
  name                  = "raw"
  storage_account_name  = azurerm_storage_account.adls.name
  container_access_type = "private"
}

# Container: Bronze (cleansed data)
resource "azurerm_storage_container" "bronze" {
  name                  = "bronze"
  storage_account_name  = azurerm_storage_account.adls.name
  container_access_type = "private"
}

# Container: Silver (transformed/enriched data)
resource "azurerm_storage_container" "silver" {
  name                  = "silver"
  storage_account_name  = azurerm_storage_account.adls.name
  container_access_type = "private"
}

# Container: Gold (aggregated/curated data for analytics)
resource "azurerm_storage_container" "gold" {
  name                  = "gold"
  storage_account_name  = azurerm_storage_account.adls.name
  container_access_type = "private"
}

# Container: Hot Path (Stream Analytics output)
resource "azurerm_storage_container" "hotpath" {
  name                  = "hotpath"
  storage_account_name  = azurerm_storage_account.adls.name
  container_access_type = "private"
}

# Storage Queue: Device Events (Event Grid subscription endpoint)
resource "azurerm_storage_queue" "device_events" {
  name                 = "device-events"
  storage_account_name = azurerm_storage_account.adls.name
}

# Lifecycle Management Policy (auto-archive/delete old data to save costs)
resource "azurerm_storage_management_policy" "lifecycle" {
  count              = var.enable_lifecycle_policy ? 1 : 0
  storage_account_id = azurerm_storage_account.adls.id

  rule {
    name    = "archiveOldData"
    enabled = true

    filters {
      prefix_match = ["raw/", "bronze/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = 30   # Move to Cool after 30 days
        tier_to_archive_after_days_since_modification_greater_than = 90   # Archive after 90 days
        delete_after_days_since_modification_greater_than          = 365  # Delete after 1 year
      }
    }
  }

  rule {
    name    = "deleteHotPath"
    enabled = true

    filters {
      prefix_match = ["hotpath/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 7  # Keep hot path data for 7 days only
      }
    }
  }
}

# Diagnostic settings for Storage Account
# Note: Storage Account level only supports metrics, not logs
# Logs must be configured on individual services (blob, table, queue, file)
resource "azurerm_monitor_diagnostic_setting" "adls" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-adls"
  target_resource_id         = azurerm_storage_account.adls.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  metric {
    category = "Transaction"
  }

  metric {
    category = "Capacity"
  }
}

# Diagnostic settings for Blob Service (for read/write/delete logs)
resource "azurerm_monitor_diagnostic_setting" "adls_blob" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-adls-blob"
  target_resource_id         = "${azurerm_storage_account.adls.id}/blobServices/default"
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "StorageRead"
  }

  enabled_log {
    category = "StorageWrite"
  }

  enabled_log {
    category = "StorageDelete"
  }

  metric {
    category = "Transaction"
  }

  metric {
    category = "Capacity"
  }
}
