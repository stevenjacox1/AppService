# Azure App Service - REST API with Table Storage

A complete template for deploying a REST API on Azure App Service with Azure Table Storage as the database.

## Architecture

- **ASP.NET Core 10.0** - REST API framework
- **Azure App Service** - Hosting platform
- **Azure Table Storage** - NoSQL database
- **Azure Managed Identity** - Secure authentication
- **Docker** - Containerization
- **GitHub Actions** - CI/CD pipeline
- **Azurite** - Local Table Storage emulator
- **Concurrently** - Parallel process runner

## Project Structure

```
├── Controllers/              # REST API endpoints
├── Models/                   # Data models
├── Services/                 # Business logic (Table Storage integration)
├── infra/                    # Infrastructure as Code (Bicep)
├── .github/workflows/        # CI/CD pipelines
├── Dockerfile               # Container image definition
├── appsettings.json         # Development configuration
├── appsettings.Production.json # Production configuration
└── AppService.csproj        # Project file
```

## Features

### REST API Endpoints

All endpoints are prefixed with `/api/items`

- `POST /api/items` - Create a new item
- `GET /api/items` - Get all items
- `GET /api/items/partition/{partitionKey}` - Get items by partition key
- `GET /api/items/{partitionKey}/{rowKey}` - Get a specific item
- `PUT /api/items/{partitionKey}/{rowKey}` - Update an item
- `DELETE /api/items/{partitionKey}/{rowKey}` - Delete an item
- `GET /api/items/health` - Health check

### Data Model (Item)

```csharp
{
  "partitionKey": "string",
  "name": "string",
  "description": "string",
  "price": 0.00,
  "isActive": true
}
```

## Prerequisites

- .NET 10.0 SDK
- Node.js and npm (for Azurite and concurrently)
- Azure CLI
- Azure subscription
- Docker (for containerization)
- Git (for CI/CD)

## Quick Start - Local Development

Run Azurite and the API together with one command:

```bash
# Install dependencies
npm install
dotnet restore

# Start everything
npm start
```

Then open: http://localhost:5000/swagger

For detailed setup instructions, see [LOCAL_DEVELOPMENT.md](LOCAL_DEVELOPMENT.md)

## Local Development (Detailed)

### 1. Clone the repository and install dependencies

```bash
git clone <repository-url>
cd AppService
npm install          # For Azurite and concurrently
dotnet restore       # For .NET packages
```

### 2. Run with Azurite (Recommended)

```bash
npm start            # Starts both Azurite and the API
```

Or manually:
```bash
npm run azurite      # Terminal 1
npm run dotnet       # Terminal 2
```

### 3. Access the API

- Swagger UI: http://localhost:5000/swagger
- Health Check: http://localhost:5000/api/items/health
- API Base: http://localhost:5000/api/items

For more details on local development, see [LOCAL_DEVELOPMENT.md](LOCAL_DEVELOPMENT.md) and [QUICKSTART.md](QUICKSTART.md)
  }
}
```

### 4. Alternative: Use Azure Storage Account

## Deployment to Azure

### Option 1: Using Bicep (Infrastructure as Code)

#### Prerequisites
- Azure CLI installed and authenticated
- Azure subscription with necessary permissions

#### Deploy Development Environment

```bash
# Login to Azure
az login

# Set your subscription
az account set --subscription <subscription-id>

# Create resource group
az group create --name rg-appservice-dev --location eastus

# Deploy infrastructure
az deployment group create \
  --resource-group rg-appservice-dev \
  --template-file infra/main.bicep \
  --parameters infra/dev.bicepparam
```

#### Deploy Production Environment

```bash
# Create resource group
az group create --name rg-appservice-prod --location eastus

# Deploy infrastructure
az deployment group create \
  --resource-group rg-appservice-prod \
  --template-file infra/main.bicep \
  --parameters infra/prod.bicepparam
```

### Option 2: Using GitHub Actions (Recommended)

#### Setup

1. **Create Azure Service Principal**

```bash
az ad sp create-for-rbac \
  --name github-actions \
  --role Contributor \
  --scopes /subscriptions/{subscription-id}
```

2. **Add GitHub Secrets**

Go to your GitHub repository → Settings → Secrets and add:
- `AZURE_WEBAPP_PUBLISH_PROFILE` - Download from Azure Portal (App Service → Deployment Center → Publish Profile)
- Other secrets as needed

3. **Push to main branch**

The workflow will automatically build and deploy the application.

### Option 3: Using Azure Container Registry & App Service

#### Build and push Docker image

```bash
# Login to ACR
az acr login --name <your-registry-name>

# Build image
docker build -t appservice:latest .

# Tag image
docker tag appservice:latest <your-registry-name>.azurecr.io/appservice:latest

# Push to ACR
docker push <your-registry-name>.azurecr.io/appservice:latest

# Deploy to App Service
az webapp create \
  --resource-group <resource-group> \
  --plan <app-service-plan> \
  --name <app-service-name> \
  --deployment-container-image-name <your-registry-name>.azurecr.io/appservice:latest
```

## Environment Configuration

### Development (`appsettings.json`)

```json
{
  "TableStorageUri": "https://yourlocalstorageaccount.table.core.windows.net",
  "ConnectionStrings": {
    "TableStorageConnection": "your-local-connection-string"
  }
}
```

### Production (`appsettings.Production.json`)

Uses Managed Identity by default. No connection string needed if using Azure AD authentication.

```json
{
  "TableStorageUri": "https://yourstorageaccount.table.core.windows.net"
}
```

## Security Best Practices

1. **Managed Identity**: Uses `DefaultAzureCredential` for secure Azure authentication
2. **Connection Strings**: Store in Azure Key Vault, not in code
3. **HTTPS Only**: Enabled by default in App Service
4. **CORS**: Configure to allow only trusted origins
5. **Secrets Management**: Use GitHub Secrets for CI/CD credentials

## Monitoring & Logging

### Enable Application Insights

```bash
az monitor app-insights component create \
  --app <app-name> \
  --resource-group <resource-group>
```

### View Logs

```bash
# Stream application logs
az webapp log tail --name <app-service-name> --resource-group <resource-group>
```

## Scaling

The template uses Basic tier (`B2`). To scale:

```bash
# Scale up the plan
az appservice plan update \
  --name <plan-name> \
  --resource-group <resource-group> \
  --sku S1
```

## Cost Optimization

- Use **Basic** or **Standard** tiers for App Service
- Use **Standard_LRS** for Storage Account
- Consider **Consumption plan** for serverless workloads
- Enable **auto-shutdown** for non-production environments

## Troubleshooting

### Connection Issues

```bash
# Test connectivity
az storage account show-connection-string \
  --name <storage-account-name> \
  --resource-group <resource-group>
```

### View Application Logs

```bash
# Enable detailed logging
az webapp config set \
  --name <app-service-name> \
  --resource-group <resource-group> \
  --generic-configurations '{"logs": {"detailedErrorMessages": true, "failedRequestTracing": true}}'
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Submit a pull request
4. Wait for CI/CD pipeline to pass

## License

MIT
