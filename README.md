# Products Service

A basic product management HTTP microservice. Operates alongside a complimentary order management HTTP microservice.

The microservice uses an in-memory product catalogue, no product data persistence is implemented.

The microservices are integrated by a pub/sub flow with Dapr.

# Service endpoints
## Get all products
GET /Products/

Resturns a list of product objects as JSON.

## Get product by id
GET /Products/{id}

Returns the requested Product object as JSON.

## Update product stock
PUT /Products/{id}/stock

Updates the stock level for the given Product.

# Event-driven integrations
The products microservice integrates with event topics using a pub/sub flow with a Dapr sidecar.

## Emit: Product stock update
Each Product's stock level is above 0 then the product is in stock and any pending orders can be filled. A negative stock level indicates that the Product is on back-order.

A Warehose Manager can update the stock level for a given product. If a product's stock level is updated and it is now in stock, the service will emit a `stock-available` event.

Services subscrinbing to the `stock-available` topic can act on that event accordingly.

## Listen: Product ordered
Each Product's stock level is decreased when an order for the product is placed.

A User can place an order for a product, even if the product is out of stock. The service will listen to the `product-ordered` topic and reduce the stock count for the product accordingly.

# Tool chain
- VSCode, Docker Desktop, .NET8.0, C#
- VSCode extensions:
    - .NET Extension Pack
    - C# base language support
    - C# Dev Kit
    - Azure Resources
    - Container Tools#
    - Docker

# Hosting environment
All services deployed to Azure, using:
- Azure Container Registry
- Container Apps Environment
- Azure Container Apps

# Local development
## Run and Debug
1. Check out tip of `main`
2. VSCode -> Run and Debug -> "Docker: Launch .NET Core" debug profile
3. Containerised debug session started -> Browser opens -> Swagger UI available on `/swagger/`

## Code changes
Trunk based development -> Commit changes to `main` and push to origin

## Build container image and push to Container Registry
1. Explorer -> right-click Dockerfile -> Build Image... -> new image created
2. Containers -> Registried > Connect Reigstry
3. Containers -> Images -> productservice/lastest -> Push...

# Cloud deployment

# Future CI/CD
- Use Github PR process for merges to origin
    - Automate PRs using GenAI code review agent
    - Automate code quality checks
    - Human in the loop to approve PR before merge
- On merge to `main`
    - Automate creation new Docker image and push to ACR
    - Automate creation of new revision microservice and deploy new image