. "$PSScriptRoot\variables.ps1"

$acrCreds = az acr credential show --name $ACR_NAME --resource-group $RESOURCE_GROUP | ConvertFrom-Json
$ACR_USERNAME = $acrCreds.username
$ACR_PASSWORD = $acrCreds.passwords[0].value

docker login $ACR_REGISTRY_SERVER --username $ACR_USERNAME --password $ACR_PASSWORD
docker build --no-cache -t productsservice:latest ProductsService/.
docker tag productsservice:latest $PRODUCTSSERVICE_IMAGE_NAME
docker push $PRODUCTSSERVICE_IMAGE_NAME

$APP_INSIGHTS_CONNECTION_STRING = az monitor app-insights component show `
  --app $APP_INSIGHTS_NAME `
  --resource-group $RESOURCE_GROUP `
  --query connectionString `
  --output tsv

# Check if the container app exists
$existingApp = az containerapp show --name $PRODUCTSSERVICE_APP_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>$null

if ([string]::IsNullOrEmpty($existingApp)) {
    # Container app does not exist, create it
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
    az containerapp update `
      --name $PRODUCTSSERVICE_APP_NAME `
      --resource-group $RESOURCE_GROUP `
      --image $PRODUCTSSERVICE_IMAGE_NAME `
      --registry-server $ACR_REGISTRY_SERVER `
      --registry-username $ACR_USERNAME `
      --registry-password $ACR_PASSWORD `
      --env-vars APPLICATIONINSIGHTS_CONNECTION_STRING=$APP_INSIGHTS_CONNECTION_STRING
}