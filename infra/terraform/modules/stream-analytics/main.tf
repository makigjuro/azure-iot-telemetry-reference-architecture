# Stream Analytics Module - Real-time Processing (Hot Path)
# Cost: 1 SU (Streaming Unit) ~$81/month (can pause when not in use!)

resource "azurerm_stream_analytics_job" "main" {
  name                = "asa-${var.naming_prefix}"
  resource_group_name = var.resource_group_name
  location            = var.location

  # 1 Streaming Unit - minimum
  streaming_units = 1

  # V2 compatibility level (latest)
  compatibility_level = "1.2"

  # System-assigned managed identity for accessing Event Hubs, ADLS
  identity {
    type = "SystemAssigned"
  }

  # Data locale
  data_locale = "en-US"

  # Job type
  type = "Cloud"

  # Transformation query (hot path: real-time anomaly detection)
  transformation_query = <<QUERY
    -- Hot Path: Real-time anomaly detection and aggregation

    -- Output 1: Real-time alerts (temperature > threshold)
    SELECT
        IoTHub.ConnectionDeviceId AS DeviceId,
        AVG(CAST(temperature AS float)) AS AvgTemperature,
        MAX(CAST(temperature AS float)) AS MaxTemperature,
        MIN(CAST(temperature AS float)) AS MinTemperature,
        COUNT(*) AS MessageCount,
        System.Timestamp() AS WindowEnd
    INTO
        [alerts-output]
    FROM
        [iothub-input] TIMESTAMP BY EventEnqueuedUtcTime
    GROUP BY
        IoTHub.ConnectionDeviceId,
        TumblingWindow(Duration(minute, 5))
    HAVING
        MAX(CAST(temperature AS float)) > 75.0

    -- Output 2: Aggregated metrics to ADLS (hot path storage)
    SELECT
        IoTHub.ConnectionDeviceId AS DeviceId,
        AVG(CAST(temperature AS float)) AS AvgTemperature,
        AVG(CAST(humidity AS float)) AS AvgHumidity,
        COUNT(*) AS MessageCount,
        System.Timestamp() AS WindowEnd
    INTO
        [adls-output]
    FROM
        [iothub-input] TIMESTAMP BY EventEnqueuedUtcTime
    GROUP BY
        IoTHub.ConnectionDeviceId,
        TumblingWindow(Duration(minute, 1))
  QUERY

  tags = var.tags
}

# Input: Event Hub (from IoT Hub)
resource "azurerm_stream_analytics_stream_input_eventhub_v2" "iothub" {
  name                         = "iothub-input"
  stream_analytics_job_id      = azurerm_stream_analytics_job.main.id
  eventhub_name                = var.eventhub_name
  servicebus_namespace         = var.eventhub_namespace_name
  eventhub_consumer_group_name = var.eventhub_consumer_group_name

  # Use managed identity for authentication (no connection string!)
  authentication_mode = "Msi"

  # Serialization format
  serialization {
    type     = "Json"
    encoding = "UTF8"
  }
}

# Output 1: Alerts to Service Bus Queue (for Alert Handler Container App)
resource "azurerm_stream_analytics_output_servicebus_queue" "alerts" {
  name                      = "alerts-output"
  stream_analytics_job_name = azurerm_stream_analytics_job.main.name
  resource_group_name       = var.resource_group_name
  queue_name                = "stream-alerts"
  servicebus_namespace      = var.servicebus_namespace_name
  shared_access_policy_key  = var.servicebus_shared_access_key
  shared_access_policy_name = "RootManageSharedAccessKey"

  # Serialization
  serialization {
    type     = "Json"
    encoding = "UTF8"
    format   = "LineSeparated"
  }
}

# Output 2: Hot path data to ADLS
resource "azurerm_stream_analytics_output_blob" "adls" {
  name                      = "adls-output"
  stream_analytics_job_name = azurerm_stream_analytics_job.main.name
  resource_group_name       = var.resource_group_name
  storage_account_name      = var.storage_account_name
  storage_container_name    = var.storage_container_hotpath
  path_pattern              = "hotpath/{date}/{time}"
  date_format               = "yyyy-MM-dd"
  time_format               = "HH"

  # Use managed identity for authentication
  authentication_mode = "Msi"

  # Serialization (JSON instead of Parquet due to Terraform provider limitations)
  serialization {
    type     = "Json"
    encoding = "UTF8"
    format   = "LineSeparated"
  }
}

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "stream_analytics" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-stream-analytics"
  target_resource_id         = azurerm_stream_analytics_job.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "Execution"
  }

  enabled_log {
    category = "Authoring"
  }

  metric {
    category = "AllMetrics"
  }
}
