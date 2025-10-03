output "stream_analytics_job_id" {
  description = "Stream Analytics Job ID"
  value       = azurerm_stream_analytics_job.main.id
}

output "stream_analytics_job_name" {
  description = "Stream Analytics Job name"
  value       = azurerm_stream_analytics_job.main.name
}

output "stream_analytics_principal_id" {
  description = "Stream Analytics managed identity principal ID"
  value       = azurerm_stream_analytics_job.main.identity[0].principal_id
}
