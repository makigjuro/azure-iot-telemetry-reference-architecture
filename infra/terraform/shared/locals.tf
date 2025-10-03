# Shared naming conventions and common configurations

locals {
  # Naming conventions
  naming_prefix = "${var.project_name}-${var.environment}"

  # Common tags
  common_tags = {
    Environment = var.environment
    Project     = var.project_name
    ManagedBy   = "Terraform"
    CostCenter  = "IoT-Platform"
  }

  # Cost optimization tier mapping (LOWEST COST)
  cost_tiers = {
    # IoT Hub: F1 (Free, 1 per subscription) or B1 (Basic, ~$10/month)
    iothub_sku = {
      name     = "B1"  # Basic tier - cheapest paid option
      capacity = 1
    }

    # Event Hubs: Basic tier (~$11/month for 1 TU)
    eventhub_sku = "Basic"

    # Stream Analytics: 1 SU (Streaming Unit) minimum
    stream_analytics_su = 1

    # PostgreSQL: Burstable B1ms (~$12/month)
    postgres_sku = "B_Standard_B1ms"

    # Storage: Standard LRS (cheapest)
    storage_tier = "Standard"
    storage_replication = "LRS"

    # Container Apps: Consumption plan (pay per use, can scale to 0)
    aca_sku = "Consumption"

    # Log Analytics: Pay-as-you-go (no commitment)
    log_analytics_sku = "PerGB2018"

    # Key Vault: Standard (no premium HSM)
    key_vault_sku = "standard"
  }
}

variable "project_name" {
  description = "Project name for resource naming"
  type        = string
  default     = "iot"
}

variable "environment" {
  description = "Environment (dev/staging/prod)"
  type        = string
}
