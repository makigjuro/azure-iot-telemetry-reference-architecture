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

variable "enable_public_access" {
  description = "Enable public network access (set to false for production)"
  type        = bool
  default     = true
}

variable "storage_connection_string" {
  description = "Storage account connection string for file uploads"
  type        = string
  sensitive   = true
}

variable "eventhub_endpoint_uri" {
  description = "Event Hub namespace endpoint URI (e.g., sb://namespace.servicebus.windows.net/)"
  type        = string
}

variable "eventhub_name" {
  description = "Event Hub name for telemetry routing"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault ID for storing connection strings"
  type        = string
}

variable "enable_diagnostics" {
  description = "Enable diagnostic settings"
  type        = bool
  default     = true
}

variable "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID for diagnostics"
  type        = string
  default     = null
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
