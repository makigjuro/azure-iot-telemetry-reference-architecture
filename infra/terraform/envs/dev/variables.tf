# Complete Variables for Dev Environment
# All required variables for the full architecture

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "eastus"  # Cheapest region for most services
}

# =============================================================================
# DATABASE CREDENTIALS
# =============================================================================

variable "postgres_admin_username" {
  description = "PostgreSQL administrator username"
  type        = string
  default     = "psqladmin"
}

variable "postgres_admin_password" {
  description = "PostgreSQL administrator password (must be complex)"
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.postgres_admin_password) >= 8
    error_message = "Password must be at least 8 characters long."
  }
}

# =============================================================================
# CONTAINER IMAGES (Phase 2)
# =============================================================================

variable "telemetry_processor_image" {
  description = "Container image for telemetry processor"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "alert_handler_image" {
  description = "Container image for alert handler"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "event_subscriber_image" {
  description = "Container image for event subscriber"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}
