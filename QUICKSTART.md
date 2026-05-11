# Quick Start Guide

## Local Development (5 minutes)

### 1. Prerequisites
- .NET 10.0 SDK installed
- Node.js and npm (for Azurite and concurrently)
- Visual Studio Code or Visual Studio

### 2. Setup

```bash
# Clone and navigate
git clone <repository-url>
cd AppService

# Install Node dependencies (includes Azurite and concurrently)
npm install

# Restore .NET packages
dotnet restore
```

### 3. Run Locally (Easiest Way)

One command starts both Azurite and the app:
```bash
npm start
```

This automatically starts:
- ✅ Azurite (Table Storage emulator) on `http://127.0.0.1:10002`
- ✅ .NET API on `http://localhost:5000`

Navigate to: `http://localhost:5000/swagger` to see Swagger UI

### 4. Test the API

```bash
# Create an item
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "partitionKey": "products",
    "name": "Sample Product",
    "description": "A test product",
    "price": 19.99
  }'

# Get all items
curl http://localhost:5000/api/items

# Health check
curl http://localhost:5000/api/items/health
```

## Azure Deployment (10 minutes)

### Option A: Using Azure CLI Script

```bash
# Make script executable (Linux/Mac)
chmod +x scripts/deploy.sh

# Deploy to development
./scripts/deploy.sh dev

# Deploy to production
./scripts/deploy.sh prod
```

### Option B: Using PowerShell Script

```powershell
# Deploy to development
.\scripts\deploy.ps1 -Environment dev

# Deploy to production
.\scripts\deploy.ps1 -Environment prod
```

### Option C: Manual Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create -n rg-appservice-dev -l eastus

# Deploy infrastructure
az deployment group create \
  -g rg-appservice-dev \
  -f infra/main.bicep \
  -p infra/dev.bicepparam

# Build and publish
dotnet publish -c Release -o ./publish

# Deploy to App Service (get the name from deployment output)
az webapp deployment source config-zip \
  -g rg-appservice-dev \
  -n <app-service-name> \
  --src publish.zip
```

## Verify Deployment

```bash
# Get App Service URL
az webapp show \
  -g rg-appservice-dev \
  -n <app-service-name> \
  --query defaultHostName

# Test the API
curl https://<app-service-name>.azurewebsites.net/api/items/health
```

## Configuration

### Development
Edit `appsettings.json`:
```json
{
  "TableStorageUri": "https://yourstorageaccount.table.core.windows.net",
  "ConnectionStrings": {
    "TableStorageConnection": "your-connection-string"
  }
}
```

### Production
Uses Azure Managed Identity - no connection string needed. Just set:
```json
{
  "TableStorageUri": "https://yourstorageaccount.table.core.windows.net"
}
```

## Key Files

- `Program.cs` - Application entry point and configuration
- `Controllers/ItemsController.cs` - REST API endpoints
- `Services/TableStorageService.cs` - Database logic
- `infra/main.bicep` - Infrastructure definition
- `Dockerfile` - Container image definition

## Next Steps

1. **Add Authentication** - Implement Azure AD authentication
2. **Add Database** - Consider Azure SQL Database for relational data
3. **Setup CI/CD** - Use GitHub Actions for automated deployments
4. **Setup Monitoring** - Enable Application Insights
5. **Setup Custom Domain** - Add your custom domain name

## Troubleshooting

**Application won't start?**
- Check `appsettings.json` configuration
- Verify Table Storage connection string
- Check application logs: `az webapp log tail -g <group> -n <name>`

**API returns 404?**
- Verify endpoint URL format: `/api/items`
- Check Swagger UI for available endpoints
- Ensure the App Service is running

**Table Storage connection fails?**
- Verify connection string is correct
- Check storage account access keys
- Ensure Managed Identity has Table Storage access
