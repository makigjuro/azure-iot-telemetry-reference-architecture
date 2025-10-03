# Complete Outputs for Full Architecture

output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}

output "location" {
  description = "Azure region"
  value       = azurerm_resource_group.main.location
}

# =============================================================================
# NETWORKING
# =============================================================================

output "vnet_id" {
  description = "Virtual Network ID"
  value       = module.networking.vnet_id
}

output "vnet_name" {
  description = "Virtual Network name"
  value       = module.networking.vnet_name
}

output "subnet_application_id" {
  description = "Application subnet ID"
  value       = module.networking.subnet_application_id
}

# =============================================================================
# MONITORING
# =============================================================================

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = module.monitoring.log_analytics_workspace_id
}

output "log_analytics_workspace_name" {
  description = "Log Analytics Workspace name"
  value       = module.monitoring.log_analytics_workspace_name
}

output "application_insights_id" {
  description = "Application Insights ID"
  value       = module.monitoring.application_insights_id
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = module.monitoring.application_insights_connection_string
  sensitive   = true
}

# =============================================================================
# STORAGE
# =============================================================================

output "storage_account_name" {
  description = "Storage Account name"
  value       = module.storage.storage_account_name
}

output "storage_account_id" {
  description = "Storage Account ID"
  value       = module.storage.storage_account_id
}

output "adls_endpoint" {
  description = "Data Lake endpoint"
  value       = module.storage.storage_account_primary_dfs_endpoint
}

# =============================================================================
# SECURITY
# =============================================================================

output "key_vault_name" {
  description = "Key Vault name"
  value       = module.security.key_vault_name
}

output "key_vault_id" {
  description = "Key Vault ID"
  value       = module.security.key_vault_id
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = module.security.key_vault_uri
}

# =============================================================================
# IOT HUB & DPS
# =============================================================================

output "iothub_name" {
  description = "IoT Hub name"
  value       = module.iot_hub.iothub_name
}

output "iothub_hostname" {
  description = "IoT Hub hostname"
  value       = module.iot_hub.iothub_hostname
}

output "iothub_id" {
  description = "IoT Hub ID"
  value       = module.iot_hub.iothub_id
}

output "dps_name" {
  description = "Device Provisioning Service name"
  value       = module.iot_hub.dps_name
}

output "dps_hostname" {
  description = "Device Provisioning Service hostname"
  value       = module.iot_hub.dps_hostname
}

output "dps_id_scope" {
  description = "Device Provisioning Service ID Scope"
  value       = module.iot_hub.dps_id_scope
}

# =============================================================================
# EVENT STREAMING
# =============================================================================

output "eventhub_namespace_name" {
  description = "Event Hub Namespace name"
  value       = module.event_streaming.eventhub_namespace_name
}

output "eventhub_namespace_endpoint" {
  description = "Event Hub Namespace endpoint"
  value       = module.event_streaming.eventhub_namespace_endpoint
}

output "eventhub_telemetry_name" {
  description = "Telemetry Event Hub name"
  value       = module.event_streaming.eventhub_telemetry_name
}

output "servicebus_namespace_name" {
  description = "Service Bus Namespace name"
  value       = module.event_streaming.servicebus_namespace_name
}

# =============================================================================
# STREAM ANALYTICS
# =============================================================================

output "stream_analytics_job_name" {
  description = "Stream Analytics Job name"
  value       = module.stream_analytics.stream_analytics_job_name
}

output "stream_analytics_job_id" {
  description = "Stream Analytics Job ID"
  value       = module.stream_analytics.stream_analytics_job_id
}

# =============================================================================
# DATABASE
# =============================================================================

output "postgres_server_name" {
  description = "PostgreSQL server name"
  value       = module.database.postgres_server_name
}

output "postgres_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = module.database.postgres_fqdn
}

output "postgres_database_name" {
  description = "PostgreSQL database name"
  value       = module.database.postgres_database_name
}

# =============================================================================
# DIGITAL TWINS
# =============================================================================

output "digital_twins_name" {
  description = "Azure Digital Twins instance name"
  value       = module.digital_twins.digital_twins_name
}

output "digital_twins_hostname" {
  description = "Azure Digital Twins hostname"
  value       = module.digital_twins.digital_twins_hostname
}

output "digital_twins_id" {
  description = "Azure Digital Twins ID"
  value       = module.digital_twins.digital_twins_id
}

# =============================================================================
# CONTAINER APPS
# =============================================================================

output "container_app_environment_name" {
  description = "Container Apps Environment name"
  value       = module.container_apps.container_app_environment_name
}

output "telemetry_processor_id" {
  description = "Telemetry Processor container app ID"
  value       = module.container_apps.telemetry_processor_id
}

output "alert_handler_id" {
  description = "Alert Handler container app ID"
  value       = module.container_apps.alert_handler_id
}

output "event_subscriber_id" {
  description = "Event Subscriber container app ID"
  value       = module.container_apps.event_subscriber_id
}

output "event_subscriber_fqdn" {
  description = "Event Subscriber FQDN (for Event Grid webhook)"
  value       = module.container_apps.event_subscriber_fqdn
}

# =============================================================================
# HELPFUL COMMANDS
# =============================================================================

output "helpful_commands" {
  description = "Helpful Azure CLI commands"
  value = <<-EOT
    # View all resources
    az resource list --resource-group ${azurerm_resource_group.main.name} --output table

    # Create test IoT device
    az iot hub device-identity create --hub-name ${module.iot_hub.iothub_name} --device-id test-device-001

    # Check Container Apps status
    az containerapp list --resource-group ${azurerm_resource_group.main.name} --output table

    # View Key Vault secrets
    az keyvault secret list --vault-name ${module.security.key_vault_name} --output table

    # Stop Stream Analytics (save costs)
    az stream-analytics job stop --resource-group ${azurerm_resource_group.main.name} --name ${module.stream_analytics.stream_analytics_job_name}
  EOT
}
