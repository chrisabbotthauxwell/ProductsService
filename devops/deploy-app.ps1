. "$PSScriptRoot\variables.ps1"

$acrCreds = az acr credential show --name $ACR_NAME --resource-group $RESOURCE_GROUP | ConvertFrom-Json
$ACR_USERNAME = $acrCreds.username
$ACR_PASSWORD = $acrCreds.passwords[0].value

docker login $ACR_REGISTRY_SERVER --username $ACR_USERNAME --password $ACR_PASSWORD
# Build and push ProductsService image
Write-Host "Building and pushing ProductsService image."
docker build --no-cache -t productsservice:latest ProductsService/.
docker tag productsservice:latest $PRODUCTSSERVICE_IMAGE_NAME
docker push $PRODUCTSSERVICE_IMAGE_NAME

# Build and push OrdersService image
Write-Host "Building and pushing OrdersService image."
docker build --no-cache -t ordersservice:latest OrdersService/.
docker tag ordersservice:latest $ORDERSSERVICE_IMAGE_NAME
docker push $ORDERSSERVICE_IMAGE_NAME

# Get App Insights connection string
$APP_INSIGHTS_CONNECTION_STRING = az monitor app-insights component show `
  --app $APP_INSIGHTS_NAME `
  --resource-group $RESOURCE_GROUP `
  --query connectionString `
  --output tsv

# Store connection string in Key Vault
# az keyvault secret set --vault-name $KEY_VAULT_NAME --name "AppInsightsConnectionString" --value $APP_INSIGHTS_CONNECTION_STRING

# Check if the ProductsService container app exists
$existingApp = az containerapp show --name $PRODUCTSSERVICE_APP_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null

if ([string]::IsNullOrEmpty($existingApp)) {
    # Container app does not exist, create it
    Write-Host "Container app $PRODUCTSSERVICE_APP_NAME does not exist. Creating it."
    az containerapp create `
      --name $PRODUCTSSERVICE_APP_NAME `
      --resource-group $RESOURCE_GROUP `
      --environment $ENV_NAME `
      --image $PRODUCTSSERVICE_IMAGE_NAME `
      --registry-server $ACR_REGISTRY_SERVER `
      --target-port 8080 `
      --ingress external `
      --min-replicas 1 `
      --max-replicas 1 `
      --cpu 0.25 `
      --memory 0.5Gi `
      --enable-dapr `
      --dapr-app-id $PRODUCTSSERVICE_APP_NAME `
      --dapr-app-port 8080 `
      --registry-username $ACR_USERNAME `
      --registry-password $ACR_PASSWORD `
      --env-vars APPLICATIONINSIGHTS_CONNECTION_STRING=$APP_INSIGHTS_CONNECTION_STRING
} else {
    # Container app exists, update it
    Write-Host "Container app $PRODUCTSSERVICE_APP_NAME already exists. Updating it."
    az containerapp update `
      --name $PRODUCTSSERVICE_APP_NAME `
      --resource-group $RESOURCE_GROUP `
      --image $PRODUCTSSERVICE_IMAGE_NAME `
      --registry-server $ACR_REGISTRY_SERVER `
      --registry-username $ACR_USERNAME `
      --registry-password $ACR_PASSWORD `
      --cpu 0.25 `
      --memory 0.5Gi `
      --min-replicas 1 `
      --max-replicas 1 `
      --env-vars APPLICATIONINSIGHTS_CONNECTION_STRING=$APP_INSIGHTS_CONNECTION_STRING
}

# Check if the OrdersService container app exists
$existingApp = az containerapp show --name $ORDERSSERVICE_APP_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null
if ([string]::IsNullOrEmpty($existingApp)) {
    # Container app does not exist, create it
    Write-Host "Container app $ORDERSSERVICE_APP_NAME does not exist. Creating it."
    az containerapp create `
      --name $ORDERSSERVICE_APP_NAME `
      --resource-group $RESOURCE_GROUP `
      --environment $ENV_NAME `
      --image $ORDERSSERVICE_IMAGE_NAME `
      --registry-server $ACR_REGISTRY_SERVER `
      --target-port 8081 `
      --ingress external `
      --min-replicas 1 `
      --max-replicas 1 `
      --cpu 0.25 `
      --memory 0.5Gi `
      --enable-dapr `
      --dapr-app-id $ORDERSSERVICE_APP_NAME `
      --dapr-app-port 8081 `
      --registry-username $ACR_USERNAME `
      --registry-password $ACR_PASSWORD `
      --env-vars APPLICATIONINSIGHTS_CONNECTION_STRING=$APP_INSIGHTS_CONNECTION_STRING
} else {
    # Container app exists, update it
    Write-Host "Container app $ORDERSSERVICE_APP_NAME already exists. Updating it."
    az containerapp update `
      --name $ORDERSSERVICE_APP_NAME `
      --resource-group $RESOURCE_GROUP `
      --image $ORDERSSERVICE_IMAGE_NAME `
      --registry-server $ACR_REGISTRY_SERVER `
      --registry-username $ACR_USERNAME `
      --registry-password $ACR_PASSWORD `
      --cpu 0.25 `
      --memory 0.5Gi `
      --min-replicas 1 `
      --max-replicas 1 `
      --env-vars APPLICATIONINSIGHTS_CONNECTION_STRING=$APP_INSIGHTS_CONNECTION_STRING
}