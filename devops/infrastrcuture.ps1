. "$PSScriptRoot\variables.ps1"

az login

# Resource Group (idempotent)
Write-Host "Creating Resource Group $RESOURCE_GROUP."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Azure Container Registry
$acrExists = az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($acrExists)) {
    Write-Host "Creating ACR $ACR_NAME."
    az acr create --name $ACR_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --sku Basic --admin-enabled true
    Write-Host "ACR $ACR_NAME created."

} else {
    Write-Host "ACR $ACR_NAME already exists."
}

# Container Apps Environment
$envExists = az containerapp env show --name $ENV_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($envExists)) {
    Write-Host "Creating Container Apps Environment $ENV_NAME."
    az containerapp env create --name $ENV_NAME --resource-group $RESOURCE_GROUP --location $LOCATION
    Write-Host "Container Apps Environment $ENV_NAME created."
} else {
    Write-Host "Container Apps Environment $ENV_NAME already exists."
}

# Service Bus Namespace
$sbNamespaceExists = az servicebus namespace show --name $SB_NAMESPACE --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbNamespaceExists)) {
    Write-Host "Creating Service Bus Namespace $SB_NAMESPACE."
    az servicebus namespace create --name $SB_NAMESPACE --resource-group $RESOURCE_GROUP --location $LOCATION --sku Standard
    Write-Host "Service Bus Namespace $SB_NAMESPACE created."
} else {
    Write-Host "Service Bus Namespace $SB_NAMESPACE already exists."
}

# Service Bus Topic
$sbTopicExists = az servicebus topic show --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_STOCK_AVAILABLE --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($sbTopicExists)) {
    Write-Host "Creating Service Bus Topic $SB_TOPIC_STOCK_AVAILABLE."
    az servicebus topic create --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name $SB_TOPIC_STOCK_AVAILABLE
    Write-Host "Service Bus Topic $SB_TOPIC_STOCK_AVAILABLE created."
} else {
    Write-Host "Service Bus Topic $SB_TOPIC_STOCK_AVAILABLE already exists."
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

$SB_CONNECTION_STRING = az servicebus namespace authorization-rule keys list --resource-group $RESOURCE_GROUP --namespace-name $SB_NAMESPACE --name ManageSendListenPolicy --query primaryConnectionString -o tsv

# Optionally update pubsub.yaml with $SB_CONNECTION_STRING here
$pubsubPath = "components/azure/pubsub.yaml"
$yaml = Get-Content $pubsubPath

# Find the line with 'name: connectionString' and update the next line with the new value
for ($i = 0; $i -lt $yaml.Count; $i++) {
    if ($yaml[$i] -match 'name:\s*connectionString') {
        $yaml[$i + 1] = "    value: `"$SB_CONNECTION_STRING`""
        break
    }
}
$yaml | Set-Content $pubsubPath
#az monitor app-insights component create --app $APP_INSIGHTS_NAME --location $LOCATION --resource-group $RESOURCE_GROUP

Write-Host "Creating Dapr component $DAPR_COMPONENT_NAME."
az containerapp env dapr-component set --name $ENV_NAME --resource-group $RESOURCE_GROUP --dapr-component-name $DAPR_COMPONENT_NAME --yaml $pubsubPath