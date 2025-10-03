# Complete Dev Environment - All Modules Integrated
# Azure IoT Telemetry Reference Architecture
# Cost-Optimized Configuration: ~$219/mo

locals {
  environment   = "dev"
  project_name  = "iot"
  location      = var.location
  naming_prefix = "${local.project_name}-${local.environment}"

  common_tags = {
    Environment = local.environment
    Project     = local.project_name
    ManagedBy   = "Terraform"
    CostCenter  = "IoT-Platform"
  }
}

# =============================================================================
# RESOURCE GROUP
# =============================================================================

resource "azurerm_resource_group" "main" {
  name     = "rg-${local.naming_prefix}"
  location = local.location
  tags     = local.common_tags
}

# =============================================================================
# LAYER 1: FOUNDATION (Monitoring, Networking)
# =============================================================================

module "monitoring" {
  source = "../../modules/monitoring"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Cost optimization
  log_analytics_daily_quota_gb = 1     # 1GB/day cap
  enable_cost_optimization     = true  # 10% App Insights sampling

  tags = local.common_tags
}

module "networking" {
  source = "../../modules/networking"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  vnet_address_space        = ["10.0.0.0/16"]
  subnet_management_prefix  = ["10.0.1.0/24"]
  subnet_application_prefix = ["10.0.2.0/23"]
  subnet_data_prefix        = ["10.0.4.0/24"]

  tags = local.common_tags
}

# =============================================================================
# LAYER 2: SECURITY & STORAGE
# =============================================================================

module "security" {
  source = "../../modules/security"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Dev: allow public access, no purge protection
  enable_public_access    = true
  enable_purge_protection = false
  allowed_subnet_ids      = [module.networking.subnet_application_id]

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags
}

module "storage" {
  source = "../../modules/storage"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Dev: allow public access, enable lifecycle
  enable_public_access    = true
  allowed_subnet_ids      = [module.networking.subnet_application_id]
  enable_lifecycle_policy = true  # Auto-archive/delete

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags

  depends_on = [module.security]
}

# =============================================================================
# LAYER 3: IOT SERVICES (Event Hubs first, then IoT Hub)
# =============================================================================

module "event_streaming" {
  source = "../../modules/event-streaming"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Dev: allow public access
  enable_public_access = true

  # IoT Hub will be created next, placeholder for now
  iothub_id          = module.iot_hub.iothub_id
  storage_account_id = module.storage.storage_account_id
  key_vault_id       = module.security.key_vault_id

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags

  depends_on = [module.storage, module.security]
}

module "iot_hub" {
  source = "../../modules/iot-hub"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Dev: allow public access
  enable_public_access = true

  # Dependencies
  storage_connection_string = module.storage.storage_account_primary_connection_string
  eventhub_endpoint_uri     = module.event_streaming.eventhub_namespace_endpoint
  eventhub_name             = module.event_streaming.eventhub_telemetry_name
  key_vault_id              = module.security.key_vault_id

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags

  depends_on = [module.event_streaming, module.storage]
}

# =============================================================================
# LAYER 4: DATABASE & DIGITAL TWINS
# =============================================================================

module "database" {
  source = "../../modules/database"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Admin credentials
  admin_username = var.postgres_admin_username
  admin_password = var.postgres_admin_password

  # VNet integration
  delegated_subnet_id = module.networking.subnet_data_id
  virtual_network_id  = module.networking.vnet_id
  key_vault_id        = module.security.key_vault_id

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags

  depends_on = [module.networking, module.security]
}

module "digital_twins" {
  source = "../../modules/digital-twins"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Event Grid endpoint (using system topic from event_streaming module)
  eventgrid_topic_endpoint      = "https://${azurerm_resource_group.main.location}.eventgrid.azure.net"
  eventgrid_topic_primary_key   = "placeholder"  # Will be managed via managed identity
  eventgrid_topic_secondary_key = "placeholder"

  # IoT Hub RBAC
  iothub_principal_id = module.iot_hub.iothub_principal_id

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags

  depends_on = [module.iot_hub]
}

# =============================================================================
# LAYER 5: STREAM ANALYTICS
# =============================================================================

module "stream_analytics" {
  source = "../../modules/stream-analytics"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Inputs
  eventhub_namespace_name     = module.event_streaming.eventhub_namespace_name
  eventhub_name               = module.event_streaming.eventhub_telemetry_name
  eventhub_consumer_group_name = module.event_streaming.consumer_group_stream_analytics

  # Outputs
  servicebus_namespace_name    = module.event_streaming.servicebus_namespace_name
  servicebus_shared_access_key = module.event_streaming.servicebus_connection_string
  storage_account_name         = module.storage.storage_account_name
  storage_container_hotpath    = module.storage.container_hotpath_name

  # Diagnostics
  enable_diagnostics         = true
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags

  depends_on = [module.event_streaming, module.storage]
}

# =============================================================================
# LAYER 6: CONTAINER APPS
# =============================================================================

module "container_apps" {
  source = "../../modules/container-apps"

  naming_prefix       = local.naming_prefix
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Infrastructure
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id
  infrastructure_subnet_id   = module.networking.subnet_application_id

  # Container images (placeholder - will be built in Phase 2)
  telemetry_processor_image = var.telemetry_processor_image
  alert_handler_image       = var.alert_handler_image
  event_subscriber_image    = var.event_subscriber_image

  # Connection strings
  eventhub_connection_string = module.event_streaming.eventhub_connection_string
  eventhub_consumer_group_name = module.event_streaming.consumer_group_container_apps
  storage_connection_string  = module.storage.storage_account_primary_connection_string
  iothub_connection_string   = module.iot_hub.iothub_connection_string
  postgres_connection_string = module.database.postgres_connection_string
  digital_twins_hostname     = module.digital_twins.digital_twins_hostname
  application_insights_connection_string = module.monitoring.application_insights_connection_string

  # Diagnostics
  enable_diagnostics = true

  tags = local.common_tags

  depends_on = [
    module.networking,
    module.monitoring,
    module.event_streaming,
    module.iot_hub,
    module.database,
    module.digital_twins
  ]
}

# =============================================================================
# LAYER 7: RBAC ASSIGNMENTS
# =============================================================================

module "rbac" {
  source = "../../modules/rbac"

  # Resource IDs
  iothub_id              = module.iot_hub.iothub_id
  eventhub_namespace_id  = module.event_streaming.eventhub_namespace_id
  storage_account_id     = module.storage.storage_account_id
  key_vault_id           = module.security.key_vault_id
  digital_twins_id       = module.digital_twins.digital_twins_id

  # Principal IDs (Managed Identities)
  iothub_principal_id                = module.iot_hub.iothub_principal_id
  digital_twins_principal_id         = module.digital_twins.digital_twins_principal_id
  stream_analytics_principal_id      = module.stream_analytics.stream_analytics_principal_id
  telemetry_processor_principal_id   = module.container_apps.telemetry_processor_principal_id
  alert_handler_principal_id         = module.container_apps.alert_handler_principal_id
  event_subscriber_principal_id      = module.container_apps.event_subscriber_principal_id
  eventgrid_principal_id             = module.event_streaming.eventgrid_system_topic_principal_id

  depends_on = [
    module.iot_hub,
    module.event_streaming,
    module.storage,
    module.security,
    module.digital_twins,
    module.stream_analytics,
    module.container_apps
  ]
}
