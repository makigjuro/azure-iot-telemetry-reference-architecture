# Monitoring Module - Log Analytics + Application Insights
# Cost: Pay-as-you-go (PerGB2018) - ~$2.30/GB ingested, first 5GB/month FREE

resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Pay-as-you-go (cheapest, no commitment)
  sku                 = "PerGB2018"

  # Retention: 30 days (minimum, FREE)
  retention_in_days   = 30

  # Daily cap to control costs (optional, in GB)
  daily_quota_gb      = var.log_analytics_daily_quota_gb

  tags = var.tags
}

# Application Insights (for distributed tracing, APM)
resource "azurerm_application_insights" "main" {
  name                = "appi-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.main.id

  # Workspace-based (modern, connects to Log Analytics)
  application_type    = "web"

  # Sampling to reduce costs (10% sampling = 90% cost reduction)
  sampling_percentage = var.enable_cost_optimization ? 10 : 100

  tags = var.tags
}

# Diagnostic settings for Log Analytics itself (optional, for auditing)
resource "azurerm_monitor_diagnostic_setting" "log_analytics" {
  count                      = var.enable_diagnostics ? 1 : 0
  name                       = "diag-log-analytics"
  target_resource_id         = azurerm_log_analytics_workspace.main.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  enabled_log {
    category = "Audit"
  }

  metric {
    category = "AllMetrics"
  }
}
