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

variable "eventhub_namespace_name" {
  description = "Event Hub Namespace name"
  type        = string
}

variable "eventhub_name" {
  description = "Event Hub name for input"
  type        = string
}

variable "eventhub_consumer_group_name" {
  description = "Event Hub consumer group name for Stream Analytics"
  type        = string
}

variable "servicebus_namespace_name" {
  description = "Service Bus namespace name for alerts output"
  type        = string
}

variable "servicebus_shared_access_key" {
  description = "Service Bus shared access key"
  type        = string
  sensitive   = true
}

variable "storage_account_name" {
  description = "Storage account name for ADLS output"
  type        = string
}

variable "storage_container_hotpath" {
  description = "Storage container name for hot path data"
  type        = string
  default     = "hotpath"
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
