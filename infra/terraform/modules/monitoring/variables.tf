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

variable "log_analytics_daily_quota_gb" {
  description = "Daily ingestion quota in GB (to control costs, -1 for unlimited)"
  type        = number
  default     = 1  # 1GB/day = ~$70/month max
}

variable "enable_cost_optimization" {
  description = "Enable cost optimization features (sampling, etc.)"
  type        = bool
  default     = true
}

variable "enable_diagnostics" {
  description = "Enable diagnostic settings for Log Analytics itself"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
