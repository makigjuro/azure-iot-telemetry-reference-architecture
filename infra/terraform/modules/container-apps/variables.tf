variable "naming_prefix" {
  description = "Naming prefix for resources"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID for Container Apps Environment"
  type        = string
}

variable "infrastructure_subnet_id" {
  description = "Subnet ID for Container Apps infrastructure"
  type        = string
}

# Container images (will be built in Phase 2)
variable "telemetry_processor_image" {
  description = "Container image for telemetry processor"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"  # Placeholder
}

variable "alert_handler_image" {
  description = "Container image for alert handler"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"  # Placeholder
}

variable "event_subscriber_image" {
  description = "Container image for event subscriber"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"  # Placeholder
}

# Connection strings and secrets
variable "eventhub_connection_string" {
  description = "Event Hub connection string"
  type        = string
  sensitive   = true
}

variable "eventhub_consumer_group_name" {
  description = "Event Hub consumer group name for telemetry processor"
  type        = string
}

variable "storage_connection_string" {
  description = "Storage account connection string"
  type        = string
  sensitive   = true
}

variable "iothub_connection_string" {
  description = "IoT Hub connection string"
  type        = string
  sensitive   = true
}

variable "postgres_connection_string" {
  description = "PostgreSQL connection string"
  type        = string
  sensitive   = true
}

variable "digital_twins_hostname" {
  description = "Azure Digital Twins hostname"
  type        = string
}

variable "application_insights_connection_string" {
  description = "Application Insights connection string"
  type        = string
  sensitive   = true
}

variable "enable_diagnostics" {
  description = "Enable diagnostic settings"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
