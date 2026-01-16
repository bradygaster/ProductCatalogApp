#!/bin/bash

# Product Catalog Infrastructure Deployment Script
# Usage: ./deploy.sh [environment] [action]
# environment: dev, staging, prod
# action: validate, what-if, deploy, delete

set -e

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Change to script directory to ensure relative paths work
cd "$SCRIPT_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed. Please install it first."
    exit 1
fi

# Get parameters
ENVIRONMENT=${1:-dev}
ACTION=${2:-validate}

# Validate environment
if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|prod)$ ]]; then
    print_error "Invalid environment. Must be: dev, staging, or prod"
    exit 1
fi

# Validate action
if [[ ! "$ACTION" =~ ^(validate|what-if|deploy|delete)$ ]]; then
    print_error "Invalid action. Must be: validate, what-if, deploy, or delete"
    exit 1
fi

# Define resource group name
RESOURCE_GROUP="rg-productcatalog-${ENVIRONMENT}"
LOCATION="eastus"
DEPLOYMENT_NAME="productcatalog-${ENVIRONMENT}-$(date +%Y%m%d-%H%M%S)"

print_info "Environment: ${ENVIRONMENT}"
print_info "Action: ${ACTION}"
print_info "Resource Group: ${RESOURCE_GROUP}"

# Execute action
case $ACTION in
    validate)
        print_info "Validating Bicep templates..."
        
        # Validate main template
        print_info "Validating main.bicep..."
        az bicep build --file main.bicep
        
        # Validate modules
        print_info "Validating module templates..."
        for module in modules/*.bicep; do
            print_info "Validating $(basename $module)..."
            az bicep build --file "$module"
        done
        
        print_info "✓ All templates validated successfully!"
        ;;
        
    what-if)
        print_info "Running what-if deployment for ${ENVIRONMENT}..."
        
        # Check if logged in
        if ! az account show &> /dev/null; then
            print_error "Not logged into Azure. Please run 'az login' first."
            exit 1
        fi
        
        # Check if resource group exists
        if ! az group show --name "${RESOURCE_GROUP}" &> /dev/null; then
            print_warning "Resource group ${RESOURCE_GROUP} does not exist."
            print_info "Creating resource group..."
            az group create --name "${RESOURCE_GROUP}" --location "${LOCATION}"
        fi
        
        # Run what-if
        az deployment group what-if \
            --resource-group "${RESOURCE_GROUP}" \
            --template-file main.bicep \
            --parameters parameters/${ENVIRONMENT}.bicepparam \
            --name "${DEPLOYMENT_NAME}"
        ;;
        
    deploy)
        print_info "Deploying infrastructure for ${ENVIRONMENT}..."
        
        # Check if logged in
        if ! az account show &> /dev/null; then
            print_error "Not logged into Azure. Please run 'az login' first."
            exit 1
        fi
        
        # Check if resource group exists
        if ! az group show --name "${RESOURCE_GROUP}" &> /dev/null; then
            print_info "Creating resource group ${RESOURCE_GROUP}..."
            az group create --name "${RESOURCE_GROUP}" --location "${LOCATION}"
        fi
        
        # Deploy
        print_info "Starting deployment..."
        az deployment group create \
            --resource-group "${RESOURCE_GROUP}" \
            --template-file main.bicep \
            --parameters parameters/${ENVIRONMENT}.bicepparam \
            --name "${DEPLOYMENT_NAME}"
        
        print_info "✓ Deployment completed!"
        
        # Show outputs
        print_info "Deployment Outputs:"
        az deployment group show \
            --resource-group "${RESOURCE_GROUP}" \
            --name "${DEPLOYMENT_NAME}" \
            --query properties.outputs \
            --output table
        ;;
        
    delete)
        print_warning "This will DELETE all resources in ${RESOURCE_GROUP}!"
        read -p "Are you sure? Type 'yes' to confirm: " confirm
        
        if [ "$confirm" != "yes" ]; then
            print_info "Deletion cancelled."
            exit 0
        fi
        
        print_info "Deleting resource group ${RESOURCE_GROUP}..."
        az group delete --name "${RESOURCE_GROUP}" --yes --no-wait
        print_info "✓ Deletion initiated (running in background)"
        ;;
esac

print_info "Done!"
