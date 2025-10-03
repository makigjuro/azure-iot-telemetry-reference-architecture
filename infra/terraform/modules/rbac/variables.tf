variable "iothub_id" {
  description = "IoT Hub resource ID"
  type        = string
}

variable "iothub_principal_id" {
  description = "IoT Hub managed identity principal ID"
  type        = string
}

variable "eventhub_namespace_id" {
  description = "Event Hub Namespace resource ID"
  type        = string
}

variable "storage_account_id" {
  description = "Storage Account resource ID"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault resource ID"
  type        = string
}

variable "digital_twins_id" {
  description = "Azure Digital Twins resource ID"
  type        = string
}

variable "digital_twins_principal_id" {
  description = "Azure Digital Twins managed identity principal ID"
  type        = string
}

variable "stream_analytics_principal_id" {
  description = "Stream Analytics managed identity principal ID"
  type        = string
}

variable "telemetry_processor_principal_id" {
  description = "Telemetry Processor container app managed identity principal ID"
  type        = string
}

variable "alert_handler_principal_id" {
  description = "Alert Handler container app managed identity principal ID"
  type        = string
}

variable "event_subscriber_principal_id" {
  description = "Event Subscriber container app managed identity principal ID"
  type        = string
}

variable "eventgrid_principal_id" {
  description = "Event Grid System Topic managed identity principal ID"
  type        = string
}
