# Container Apps Module - Microservices Runtime
# Cost: Consumption plan ~$0-5/month (pay per use, scales to 0)

# Container Apps Environment
resource "azurerm_container_app_environment" "main" {
  name                       = "cae-${var.naming_prefix}"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = var.log_analytics_workspace_id

  # VNet integration for outbound traffic
  infrastructure_subnet_id = var.infrastructure_subnet_id

  tags = var.tags
}

# Container App: Telemetry Processor (Cold Path - Event Hubs consumer)
resource "azurerm_container_app" "telemetry_processor" {
  name                         = "ca-telemetry-processor-${var.naming_prefix}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  # System-assigned managed identity
  identity {
    type = "SystemAssigned"
  }

  template {
    # Scale to zero when no events (cost optimization!)
    min_replicas = 0
    max_replicas = 10

    container {
      name   = "telemetry-processor"
      image  = var.telemetry_processor_image
      cpu    = 0.25
      memory = "0.5Gi"

      # Environment variables from Key Vault
      env {
        name        = "EVENTHUB_CONNECTION_STRING"
        secret_name = "eventhub-connection-string"
      }

      env {
        name        = "STORAGE_CONNECTION_STRING"
        secret_name = "storage-connection-string"
      }

      env {
        name  = "CONSUMER_GROUP"
        value = var.eventhub_consumer_group_name
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.application_insights_connection_string
      }
    }

    # Scale rule: Event Hub trigger
    custom_scale_rule {
      name             = "eventhub-scale"
      custom_rule_type = "azure-eventhub"

      metadata = {
        consumerGroup = var.eventhub_consumer_group_name
        unprocessedEventThreshold = "10"
      }

      authentication {
        secret_name       = "eventhub-connection-string"
        trigger_parameter = "connection"
      }
    }
  }

  # Secrets from Key Vault (referenced via managed identity)
  secret {
    name  = "eventhub-connection-string"
    value = var.eventhub_connection_string
  }

  secret {
    name  = "storage-connection-string"
    value = var.storage_connection_string
  }

  tags = var.tags
}

# Container App: Alert Handler (receives alerts from Stream Analytics)
resource "azurerm_container_app" "alert_handler" {
  name                         = "ca-alert-handler-${var.naming_prefix}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  template {
    min_replicas = 0
    max_replicas = 5

    container {
      name   = "alert-handler"
      image  = var.alert_handler_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "STORAGE_QUEUE_CONNECTION_STRING"
        secret_name = "storage-connection-string"
      }

      env {
        name  = "QUEUE_NAME"
        value = "stream-alerts"
      }

      env {
        name        = "IOTHUB_CONNECTION_STRING"
        secret_name = "iothub-connection-string"
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.application_insights_connection_string
      }
    }

    # Scale rule: Storage Queue trigger
    custom_scale_rule {
      name             = "queue-scale"
      custom_rule_type = "azure-queue"

      metadata = {
        queueName    = "stream-alerts"
        queueLength  = "5"
      }

      authentication {
        secret_name       = "storage-connection-string"
        trigger_parameter = "connection"
      }
    }
  }

  secret {
    name  = "storage-connection-string"
    value = var.storage_connection_string
  }

  secret {
    name  = "iothub-connection-string"
    value = var.iothub_connection_string
  }

  tags = var.tags
}

# Container App: Event Grid Subscriber (device lifecycle events)
resource "azurerm_container_app" "event_subscriber" {
  name                         = "ca-event-subscriber-${var.naming_prefix}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  template {
    min_replicas = 1  # Keep at least 1 for webhook endpoint
    max_replicas = 3

    container {
      name   = "event-subscriber"
      image  = var.event_subscriber_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "POSTGRES_CONNECTION_STRING"
        secret_name = "postgres-connection-string"
      }

      env {
        name        = "DIGITAL_TWINS_ENDPOINT"
        value       = var.digital_twins_hostname
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.application_insights_connection_string
      }
    }
  }

  # Ingress for Event Grid webhook
  ingress {
    external_enabled = true
    target_port      = 8080

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  secret {
    name  = "postgres-connection-string"
    value = var.postgres_connection_string
  }

  tags = var.tags
}

# Diagnostic settings for Container Apps Environment
resource "azurerm_monitor_diagnostic_setting" "container_apps_env" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-container-apps-env"
  target_resource_id         = azurerm_container_app_environment.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "ContainerAppConsoleLogs"
  }

  enabled_log {
    category = "ContainerAppSystemLogs"
  }

  metric {
    category = "AllMetrics"
  }
}
