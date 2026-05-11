#!/bin/bash

# Azure App Service Deployment Script
# This script deploys the infrastructure and application to Azure

set -e

# Configuration
ENVIRONMENT=${1:-dev}
RESOURCE_GROUP="rg-appservice-${ENVIRONMENT}"
LOCATION="eastus"
BICEP_FILE="infra/main.bicep"
BICEP_PARAMS="infra/${ENVIRONMENT}.bicepparam"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed"
        exit 1
    fi
    
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET CLI is not installed"
        exit 1
    fi
    
    print_info "Prerequisites check passed"
}

# Login to Azure
login_azure() {
    print_info "Logging in to Azure..."
    az login
}

# Create resource group
create_resource_group() {
    print_info "Creating resource group: $RESOURCE_GROUP"
    
    if az group exists --name "$RESOURCE_GROUP" | grep -q true; then
        print_warning "Resource group already exists"
    else
        az group create \
            --name "$RESOURCE_GROUP" \
            --location "$LOCATION"
        print_info "Resource group created"
    fi
}

# Validate Bicep
validate_bicep() {
    print_info "Validating Bicep template..."
    
    if [ ! -f "$BICEP_PARAMS" ]; then
        print_error "Parameter file not found: $BICEP_PARAMS"
        exit 1
    fi
    
    az deployment group validate \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "$BICEP_FILE" \
        --parameters "$BICEP_PARAMS"
    
    print_info "Bicep template validation passed"
}

# Deploy infrastructure
deploy_infrastructure() {
    print_info "Deploying infrastructure to $ENVIRONMENT environment..."
    
    DEPLOYMENT=$(az deployment group create \
        --resource-group "$RESOURCE_GROUP" \
        --template-file "$BICEP_FILE" \
        --parameters "$BICEP_PARAMS" \
        --query properties.outputs)
    
    print_info "Infrastructure deployment completed"
    echo "$DEPLOYMENT" | jq '.'
}

# Build application
build_application() {
    print_info "Building application..."
    
    dotnet build --configuration Release
    
    print_info "Application build completed"
}

# Publish application
publish_application() {
    print_info "Publishing application..."
    
    dotnet publish --configuration Release --output ./publish
    
    print_info "Application published to ./publish"
}

# Get app service name from outputs
get_app_service_name() {
    print_info "Retrieving App Service name..."
    
    APP_SERVICE_NAME=$(az deployment group show \
        --resource-group "$RESOURCE_GROUP" \
        --name main \
        --query "properties.outputs.appServiceName.value" \
        -o tsv 2>/dev/null || echo "")
    
    if [ -z "$APP_SERVICE_NAME" ]; then
        print_warning "Could not retrieve App Service name from deployment outputs"
        read -p "Enter App Service name: " APP_SERVICE_NAME
    fi
    
    print_info "App Service name: $APP_SERVICE_NAME"
}

# Deploy application
deploy_application() {
    print_info "Deploying application to App Service..."
    
    get_app_service_name
    
    if [ -z "$APP_SERVICE_NAME" ]; then
        print_error "App Service name is required"
        exit 1
    fi
    
    az webapp deployment source config-zip \
        --resource-group "$RESOURCE_GROUP" \
        --name "$APP_SERVICE_NAME" \
        --src <(cd ./publish && zip -r - .)
    
    print_info "Application deployment completed"
}

# Display summary
display_summary() {
    print_info "========================================="
    print_info "Deployment Summary"
    print_info "========================================="
    print_info "Environment: $ENVIRONMENT"
    print_info "Resource Group: $RESOURCE_GROUP"
    print_info "Location: $LOCATION"
    print_info "========================================="
    
    if [ ! -z "$APP_SERVICE_NAME" ]; then
        APP_URL=$(az webapp show \
            --resource-group "$RESOURCE_GROUP" \
            --name "$APP_SERVICE_NAME" \
            --query defaultHostName \
            -o tsv)
        print_info "App Service URL: https://${APP_URL}"
    fi
}

# Main
main() {
    print_info "Starting deployment process..."
    
    check_prerequisites
    login_azure
    create_resource_group
    validate_bicep
    deploy_infrastructure
    build_application
    publish_application
    deploy_application
    display_summary
    
    print_info "Deployment completed successfully!"
}

# Run main function
main
