param location string = resourceGroup().location
param appServicePlanName string = 'appserviceplan-${uniqueString(resourceGroup().id)}'
param appServiceName string = 'appservice-${uniqueString(resourceGroup().id)}'
param storageAccountName string = 'stg${uniqueString(resourceGroup().id)}'
param environment string = 'dev'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B2'
    tier: 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
  tags: {
    environment: environment
  }
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
  }
  tags: {
    environment: environment
  }
}

// App Service (Web App)
resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
  }
  tags: {
    environment: environment
  }
}

// App Service Configuration
resource appServiceConfig 'Microsoft.Web/sites/config@2022-03-01' = {
  name: '${appService.name}/web'
  properties: {
    linuxFxVersion: 'DOTNETCORE|8.0'
    appSettings: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: environment == 'prod' ? 'Production' : 'Development'
      }
      {
        name: 'TableStorageUri'
        value: 'https://${storageAccount.name}.table.core.windows.net'
      }
      {
        name: 'WEBSITE_RUN_FROM_PACKAGE'
        value: '1'
      }
    ]
    connectionStrings: [
      {
        name: 'TableStorageConnection'
        connectionString: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, '2022-09-01').keys[0].value};EndpointSuffix=core.windows.net'
        type: 'Custom'
      }
    ]
  }
  dependsOn: [
    appService
  ]
}

// Output values
output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.defaultHostName}'
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
