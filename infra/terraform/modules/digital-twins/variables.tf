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

variable "eventgrid_topic_endpoint" {
  description = "Event Grid topic endpoint for twin changes"
  type        = string
}

variable "eventgrid_topic_primary_key" {
  description = "Event Grid topic primary access key"
  type        = string
  sensitive   = true
}

variable "eventgrid_topic_secondary_key" {
  description = "Event Grid topic secondary access key"
  type        = string
  sensitive   = true
}

variable "iothub_principal_id" {
  description = "IoT Hub managed identity principal ID (for RBAC)"
  type        = string
  default     = null
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
