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

variable "enable_purge_protection" {
  description = "Enable purge protection (prevents permanent deletion, disable for dev)"
  type        = bool
  default     = false
}

variable "enable_public_access" {
  description = "Enable public network access (set to false for production)"
  type        = bool
  default     = true
}

variable "allowed_ip_addresses" {
  description = "List of allowed IP addresses for Key Vault access"
  type        = list(string)
  default     = []
}

variable "allowed_subnet_ids" {
  description = "List of allowed subnet IDs for Key Vault access"
  type        = list(string)
  default     = []
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
