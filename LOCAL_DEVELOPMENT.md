# Local Development Setup

## Prerequisites

### Azure Table Storage Emulator (Azurite)

For local development without Azure resources, you need to install and run Azurite.

### Option 1: Install Azurite via NPM

```bash
# Install globally
npm install -g azurite

# Run Azurite (starts Table Storage on http://127.0.0.1:10002)
azurite --tables --loose --location ./data
```

### Option 2: Docker

```bash
docker run -p 10002:10002 mcr.microsoft.com/azure-storage/azurite:latest azurite-table --blobHost 127.0.0.1 --tableHost 127.0.0.1
```

### Option 3: Visual Studio

If you have Visual Studio with Azure Storage tools, it includes Azurite emulator. You can start it from Server Explorer.

## Running the Application Locally

### Quick Start (Recommended)

Install dependencies once:
```bash
npm install
```

Then run both Azurite and the app with one command:
```bash
npm start
# or
npm run dev
```

This will start:
- ✅ Azurite on `http://127.0.0.1:10002`
- ✅ .NET app on `http://localhost:5000`

Press `Ctrl+C` to stop both services.

### Manual Setup (Alternative)

If you prefer to manage services manually:

**Terminal 1 - Start Azurite:**
```bash
azurite --tables --loose --location ./data
```

**Terminal 2 - Start the App:**
```bash
dotnet run
```

### Access the API

- API: http://localhost:5000 (HTTP) or https://localhost:5001 (HTTPS)
- Swagger UI: http://localhost:5000/swagger or https://localhost:5001/swagger
- Health Check: http://localhost:5000/api/items/health

## Using with Azure Storage

To use a real Azure Storage Account instead of Azurite:

1. Create an Azure Storage Account
2. Update `appsettings.Production.json`:
   ```json
   {
     "TableStorageUri": "https://yourstorageaccount.table.core.windows.net"
   }
   ```

3. Set the environment variable:
   ```bash
   $env:ASPNETCORE_ENVIRONMENT = "Production"
   dotnet run
   ```

## Troubleshooting

### "Invalid URI: The hostname could not be parsed"

This means the `TableStorageUri` in your configuration is invalid. Make sure:
- You have `appsettings.Development.json` configured (uses Azurite)
- Or update `appsettings.json` with a valid Azure Storage Account URI
- Or set `ASPNETCORE_ENVIRONMENT=Development` to use the development config

### Azurite Connection Refused

Make sure Azurite is running on the correct port (`10002` for tables). Check the process:

```bash
# PowerShell
Get-Process | Where-Object {$_.ProcessName -like "*azurite*"}

# Or test the connection
Test-NetConnection -ComputerName 127.0.0.1 -Port 10002
```

### DefaultAzureCredential not working with Azurite

Azurite doesn't validate credentials. You can use any credentials:
```bash
# These are ignored by Azurite
$env:AZURE_STORAGE_ACCOUNT_NAME = "devstoreaccount1"
$env:AZURE_STORAGE_ACCOUNT_KEY = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2uvTmjCMV5b9z5K3m/vs+VrDa8xQ+iP7VvHQ=="
```

## Environment Variables

You can override configuration using environment variables:

```bash
# Use Azure instead of Azurite
$env:TableStorageUri = "https://myaccount.table.core.windows.net"
$env:ASPNETCORE_ENVIRONMENT = "Production"

dotnet run
```
