# Networking Module - VNet, Subnets, NSGs
# Cost: VNet is FREE, only charged for data transfer

resource "azurerm_virtual_network" "main" {
  name                = "vnet-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.vnet_address_space

  tags = var.tags
}

# Subnet: Management (for VMs, Bastion)
resource "azurerm_subnet" "management" {
  name                 = "snet-management"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = var.subnet_management_prefix
}

# Subnet: Application (for ACA, Private Endpoints)
resource "azurerm_subnet" "application" {
  name                 = "snet-application"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = var.subnet_application_prefix

  # Enable private endpoints
  private_endpoint_network_policies_enabled = true

  # Service endpoints (cheaper than private endpoints for some scenarios)
  service_endpoints = [
    "Microsoft.Storage",
    "Microsoft.KeyVault",
    "Microsoft.EventHub"
  ]
}

# Subnet: Data (for PostgreSQL with delegation)
resource "azurerm_subnet" "data" {
  name                 = "snet-data"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = var.subnet_data_prefix

  # PostgreSQL delegation
  delegation {
    name = "postgresql-delegation"
    service_delegation {
      name = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action"
      ]
    }
  }
}

# NSG for Management Subnet
resource "azurerm_network_security_group" "management" {
  name                = "nsg-management-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Allow Bastion inbound
  security_rule {
    name                       = "AllowBastionInbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["22", "3389"]
    source_address_prefix      = "10.0.1.0/24"
    destination_address_prefix = "*"
  }

  tags = var.tags
}

# NSG for Application Subnet
resource "azurerm_network_security_group" "application" {
  name                = "nsg-application-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Allow HTTPS from Internet (for IoT Hub, if public endpoint needed)
  security_rule {
    name                       = "AllowHTTPSInbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  # Allow MQTT for IoT devices
  security_rule {
    name                       = "AllowMQTTInbound"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["8883", "1883"]
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  tags = var.tags
}

# NSG for Data Subnet
resource "azurerm_network_security_group" "data" {
  name                = "nsg-data-${var.naming_prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # Allow PostgreSQL from Application subnet only
  security_rule {
    name                       = "AllowPostgreSQLFromApp"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "5432"
    source_address_prefix      = var.subnet_application_prefix[0]
    destination_address_prefix = "*"
  }

  tags = var.tags
}

# Associate NSGs with Subnets
resource "azurerm_subnet_network_security_group_association" "management" {
  subnet_id                 = azurerm_subnet.management.id
  network_security_group_id = azurerm_network_security_group.management.id
}

resource "azurerm_subnet_network_security_group_association" "application" {
  subnet_id                 = azurerm_subnet.application.id
  network_security_group_id = azurerm_network_security_group.application.id
}

resource "azurerm_subnet_network_security_group_association" "data" {
  subnet_id                 = azurerm_subnet.data.id
  network_security_group_id = azurerm_network_security_group.data.id
}
