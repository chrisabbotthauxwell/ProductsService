. "$PSScriptRoot\variables.ps1"

az login

# Resource Group (idempotent)
Write-Host "Creating Resource Group $RESOURCE_GROUP."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Azure Key Vault
#$kvExists = az keyvault show --name $KEY_VAULT_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
#if ([string]::IsNullOrEmpty($kvExists)) {
#    Write-Host "Creating Key Vault $KEY_VAULT_NAME."
#    az keyvault create --name $KEY_VAULT_NAME --resource-group $RESOURCE_GROUP --location $LOCATION
#    Write-Host "Key Vault $KEY_VAULT_NAME created."
#} else {
#    Write-Host "Key Vault $KEY_VAULT_NAME already exists."
#}

# Azure Container Registry
$acrExists = az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($acrExists)) {
    Write-Host "Creating ACR $ACR_NAME."
    az acr create --name $ACR_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --sku Basic --admin-enabled true
    Write-Host "ACR $ACR_NAME created."

} else {
    Write-Host "ACR $ACR_NAME already exists."
}

# Log Analytics Workspace
$LOG_ANALYTICS_WORKSPACE_ID = az monitor log-analytics workspace show --resource-group $RESOURCE_GROUP --workspace-name $LOG_ANALYTICS_NAME --query customerId -o tsv 2>$null

if (-not $LOG_ANALYTICS_WORKSPACE_ID) {
    Write-Host "Creating Log Analytics Workspace $LOG_ANALYTICS_NAME."
    az monitor log-analytics workspace create --resource-group $RESOURCE_GROUP --workspace-name $LOG_ANALYTICS_NAME --location $LOCATION
    $LOG_ANALYTICS_WORKSPACE_ID = az monitor log-analytics workspace show --resource-group $RESOURCE_GROUP --workspace-name $LOG_ANALYTICS_NAME --query customerId -o tsv
} else {
    Write-Host "Log Analytics Workspace $LOG_ANALYTICS_NAME already exists."
}

# App Insights
$APP_INSIGHTS_EXISTS = az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null

if (-not $APP_INSIGHTS_EXISTS) {
    Write-Host "Creating Application Insights $APP_INSIGHTS_NAME."
    az monitor app-insights component create `
        --app $APP_INSIGHTS_NAME `
        --location $LOCATION `
        --resource-group $RESOURCE_GROUP `
        --workspace $LOG_ANALYTICS_NAME
} else {
    Write-Host "Application Insights $APP_INSIGHTS_NAME already exists."
}

# Get App Insights connection string
$APP_INSIGHTS_CONNECTION_STRING = az monitor app-insights component show `
  --app $APP_INSIGHTS_NAME `
  --resource-group $RESOURCE_GROUP `
  --query connectionString `
  --output tsv

# Container Apps Environment
$envExists = az containerapp env show --name $ENV_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($envExists)) {
    Write-Host "Creating Container Apps Environment $ENV_NAME."
    az containerapp env create --name $ENV_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --logs-workspace-id $LOG_ANALYTICS_WORKSPACE_ID --dapr-connection-string $APP_INSIGHTS_CONNECTION_STRING
    Write-Host "Container Apps Environment $ENV_NAME created."
} else {
    Write-Host "Container Apps Environment $ENV_NAME already exists. Updating existing environment."
    az containerapp env update --name $ENV_NAME --resource-group $RESOURCE_GROUP --logs-workspace-id $LOG_ANALYTICS_WORKSPACE_ID
    Write-Host "Container Apps Environment $ENV_NAME updated."
}

# Set Container Apps Environment OpenTelemetry collector
Write-Host "Setting OpenTelemetry collector for Container Apps Environment $ENV_NAME."
az containerapp env telemetry app-insights set --name $ENV_NAME --resource-group $RESOURCE_GROUP --connection-string $APP_INSIGHTS_CONNECTION_STRING --enable-open-telemetry-traces true --enable-open-telemetry-logs true
Write-Host "OpenTelemetry collector set for Container Apps Environment $ENV_NAME."

# Service Bus Namespace
$sbNamespaceExists = az servicebus namespace show --name $SB_NAMESPACE --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbNamespaceExists)) {
    Write-Host "Creating Service Bus Namespace $SB_NAMESPACE."
    az servicebus namespace create --name $SB_NAMESPACE --resource-group $RESOURCE_GROUP --location $LOCATION --sku Standard
    Write-Host "Service Bus Namespace $SB_NAMESPACE created."
} else {
    Write-Host "Service Bus Namespace $SB_NAMESPACE already exists."
}

# Service Bus Topic stock-available
$sbTopicExists = az servicebus topic show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_STOCK_AVAILABLE --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbTopicExists)) {
    Write-Host "Creating Service Bus Topic $SB_TOPIC_STOCK_AVAILABLE."
    az servicebus topic create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_STOCK_AVAILABLE
    Write-Host "Service Bus Topic $SB_TOPIC_STOCK_AVAILABLE created."
} else {
    Write-Host "Service Bus Topic $SB_TOPIC_STOCK_AVAILABLE already exists."
}

# Service Bus Topic stock-updated
$sbTopicExists = az servicebus topic show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_STOCK_UPDATED --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbTopicExists)) {
    Write-Host "Creating Service Bus Topic $SB_TOPIC_STOCK_UPDATED."
    az servicebus topic create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_STOCK_UPDATED
    Write-Host "Service Bus Topic $SB_TOPIC_STOCK_UPDATED created."
} else {
    Write-Host "Service Bus Topic $SB_TOPIC_STOCK_UPDATED already exists."
}

# Service Bus Topic order-placed
$sbTopicExists = az servicebus topic show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_ORDER_PLACED --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbTopicExists)) {  
    Write-Host "Creating Service Bus Topic $SB_TOPIC_ORDER_PLACED."
    az servicebus topic create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_ORDER_PLACED
    Write-Host "Service Bus Topic $SB_TOPIC_ORDER_PLACED created."
} else {
    Write-Host "Service Bus Topic $SB_TOPIC_ORDER_PLACED already exists."
}

# Service Bus Topic order-fulfilled
$sbTopicExists = az servicebus topic show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_ORDER_FULFILLED --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbTopicExists)) { 
    Write-Host "Creating Service Bus Topic $SB_TOPIC_ORDER_FULFILLED."
    az servicebus topic create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_ORDER_FULFILLED
    Write-Host "Service Bus Topic $SB_TOPIC_ORDER_FULFILLED created."
} else {
    Write-Host "Service Bus Topic $SB_TOPIC_ORDER_FULFILLED already exists."
}

# Service Bus Topic order-backordered
$sbTopicExists = az servicebus topic show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_ORDER_BACKORDERED --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbTopicExists)) {
    Write-Host "Creating Service Bus Topic $SB_TOPIC_ORDER_BACKORDERED."
    az servicebus topic create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_ORDER_BACKORDERED
    Write-Host "Service Bus Topic $SB_TOPIC_ORDER_BACKORDERED created."
} else {
    Write-Host "Service Bus Topic $SB_TOPIC_ORDER_BACKORDERED already exists."
}

# Service Bus Authorization Rule
$sbRuleExists = az servicebus namespace authorization-rule show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name ManageSendListenPolicy --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbRuleExists)) {
    Write-Host "Creating Service Bus Authorization Rule ManageSendListenPolicy."
    az servicebus namespace authorization-rule create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name ManageSendListenPolicy --rights Manage Send Listen
    Write-Host "Service Bus Authorization Rule ManageSendListenPolicy created."
} else {
    Write-Host "Service Bus Authorization Rule ManageSendListenPolicy already exists."
}

# Connection String for  Service Bus Authorization Rule
Write-Host "Retrieving primary connection string for Service Bus Authorization Rule ManageSendListenPolicy."
$SB_CONNECTION_STRING = az servicebus namespace authorization-rule keys list --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name ManageSendListenPolicy --query primaryConnectionString -o tsv

# Set the service bus connection string as a secret in the Container Apps Environment
# Write-Host "Setting Service Bus connection string as a secret in Container Apps Environment $ENV_NAME."
# az containerapp env secret set --name $ENV_NAME --resource-group $RESOURCE_GROUP --secrets sb-connection-string=$SB_CONNECTION_STRING

# Update pubsub.yaml with $SB_CONNECTION_STRING here
# THIS IS A HACK!!! SHOULD BE USING KEY VAULT
$pubsubPath = "components/azure/pubsub.yaml"
$yaml = Get-Content $pubsubPath

# Find the line with 'name: connectionString' and update the next line with the new value
Write-Host "Applying service bus connection string to Dapr pubsub.yaml: $SB_CONNECTION_STRING."
for ($i = 0; $i -lt $yaml.Count; $i++) {
    if ($yaml[$i] -match 'name:\s*connectionString') {
        $yaml[$i + 1] = "    value: `"$SB_CONNECTION_STRING`""
        break
    }
}
$yaml | Set-Content $pubsubPath

Write-Host "Creating Dapr component $DAPR_COMPONENT_NAME."
az containerapp env dapr-component set --name $ENV_NAME --resource-group $RESOURCE_GROUP --dapr-component-name $DAPR_COMPONENT_NAME --yaml $pubsubPath

# Find the line with 'name: connectionString' and remove the connection string
$yaml = Get-Content $pubsubPath

# THIS IS A HACK!!! SHOULD BE USING KEY VAULT
# Find the line with 'name: connectionString' and update the next line with the new value
Write-Host "Removing service bus connection string from Dapr pubsub.yaml."
for ($i = 0; $i -lt $yaml.Count; $i++) {
    if ($yaml[$i] -match 'name:\s*connectionString') {
        $yaml[$i + 1] = "    value: `"`""
        break
    }
}
$yaml | Set-Content $pubsubPath