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

variable "vnet_address_space" {
  description = "VNet address space"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "subnet_management_prefix" {
  description = "Management subnet address prefix"
  type        = list(string)
  default     = ["10.0.1.0/24"]
}

variable "subnet_application_prefix" {
  description = "Application subnet address prefix"
  type        = list(string)
  default     = ["10.0.2.0/23"]
}

variable "subnet_data_prefix" {
  description = "Data subnet address prefix"
  type        = list(string)
  default     = ["10.0.4.0/24"]
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
