# Azure App Service Deployment Script (PowerShell)
# This script deploys the infrastructure and application to Azure

param(
    [string]$Environment = "dev"
)

# Configuration
$ResourceGroup = "rg-appservice-${Environment}"
$Location = "eastus"
$BicepFile = "infra/main.bicep"
$BicepParams = "infra/${Environment}.bicepparam"

# Functions
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

# Check prerequisites
function Check-Prerequisites {
    Write-Info "Checking prerequisites..."
    
    $azcli = Get-Command az -ErrorAction SilentlyContinue
    if (-not $azcli) {
        Write-Error-Custom "Azure CLI is not installed"
        exit 1
    }
    
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        Write-Error-Custom ".NET CLI is not installed"
        exit 1
    }
    
    Write-Info "Prerequisites check passed"
}

# Login to Azure
function Login-Azure {
    Write-Info "Logging in to Azure..."
    az login
}

# Create resource group
function Create-ResourceGroup {
    Write-Info "Creating resource group: $ResourceGroup"
    
    $exists = az group exists --name $ResourceGroup | ConvertFrom-Json
    if ($exists) {
        Write-Warning-Custom "Resource group already exists"
    }
    else {
        az group create `
            --name $ResourceGroup `
            --location $Location
        Write-Info "Resource group created"
    }
}

# Validate Bicep
function Validate-Bicep {
    Write-Info "Validating Bicep template..."
    
    if (-not (Test-Path $BicepParams)) {
        Write-Error-Custom "Parameter file not found: $BicepParams"
        exit 1
    }
    
    az deployment group validate `
        --resource-group $ResourceGroup `
        --template-file $BicepFile `
        --parameters $BicepParams
    
    Write-Info "Bicep template validation passed"
}

# Deploy infrastructure
function Deploy-Infrastructure {
    Write-Info "Deploying infrastructure to $Environment environment..."
    
    az deployment group create `
        --resource-group $ResourceGroup `
        --template-file $BicepFile `
        --parameters $BicepParams
    
    Write-Info "Infrastructure deployment completed"
}

# Build application
function Build-Application {
    Write-Info "Building application..."
    
    dotnet build --configuration Release
    
    Write-Info "Application build completed"
}

# Publish application
function Publish-Application {
    Write-Info "Publishing application..."
    
    dotnet publish --configuration Release --output ./publish
    
    Write-Info "Application published to ./publish"
}

# Get App Service name
function Get-AppServiceName {
    Write-Info "Retrieving App Service name..."
    
    $output = az deployment group show `
        --resource-group $ResourceGroup `
        --name main `
        --query "properties.outputs.appServiceName.value" `
        -o tsv 2>$null
    
    if ([string]::IsNullOrEmpty($output)) {
        Write-Warning-Custom "Could not retrieve App Service name from deployment outputs"
        $script:AppServiceName = Read-Host "Enter App Service name"
    }
    else {
        $script:AppServiceName = $output
    }
    
    Write-Info "App Service name: $script:AppServiceName"
}

# Deploy application
function Deploy-Application {
    Write-Info "Deploying application to App Service..."
    
    Get-AppServiceName
    
    if ([string]::IsNullOrEmpty($script:AppServiceName)) {
        Write-Error-Custom "App Service name is required"
        exit 1
    }
    
    # Create zip file
    $zipPath = "./publish.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    Compress-Archive -Path ./publish/* -DestinationPath $zipPath
    
    az webapp deployment source config-zip `
        --resource-group $ResourceGroup `
        --name $script:AppServiceName `
        --src $zipPath
    
    Remove-Item $zipPath -Force
    
    Write-Info "Application deployment completed"
}

# Display summary
function Display-Summary {
    Write-Info "========================================="
    Write-Info "Deployment Summary"
    Write-Info "========================================="
    Write-Info "Environment: $Environment"
    Write-Info "Resource Group: $ResourceGroup"
    Write-Info "Location: $Location"
    Write-Info "========================================="
    
    if (-not [string]::IsNullOrEmpty($script:AppServiceName)) {
        $appUrl = az webapp show `
            --resource-group $ResourceGroup `
            --name $script:AppServiceName `
            --query defaultHostName `
            -o tsv
        Write-Info "App Service URL: https://${appUrl}"
    }
}

# Main
function Main {
    Write-Info "Starting deployment process..."
    
    Check-Prerequisites
    Login-Azure
    Create-ResourceGroup
    Validate-Bicep
    Deploy-Infrastructure
    Build-Application
    Publish-Application
    Deploy-Application
    Display-Summary
    
    Write-Info "Deployment completed successfully!"
}

# Run main
Main
