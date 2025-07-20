# ProductsOrdersManagement task list

## Foundation
- [x] Architecture diagram (microservices, Dapr, Service Bus)
- [x] Data model design for ProductsService
- [x] Data model design for OrdersService
- [x] Service orchestration plan (service components and communications)
- [x] Scaffold .net API
- [x] Init git repo
- [x] Establish README
- [x] Remove default WeatherForecast elements
- [ ] Rationalise launchSettings.json for both projects
- [x] Logging/observability plan

## ProductsService
- [x] Basic scaffolding for ProductsService
- [x] Get Products
- [x] Get Product by id
- [x] Logic for stock update for product + publish `stock-available` event
- [x] Subscribe to and handle `stock-updated` event after an order is fulfilled
- [x] Subscribe to and handle `order-placed` event
- [x] Logic for order fulfilled for order + publish `order-fulfilled` event
- [x] Logic for back ordered orders + publish `order-backordered` event
- [x] Update Azure pubsub.yaml component definition to allow both productsservice & ordersservice to access the same topics
- [x] Integrate Dapr for pubsub
- [x] Docker: Containerise basic ProductsService
- [x] Docker Compose: Configure containerisation for ProductsService stack (service + Dapr sidecar + redis streams)
- [x] Logging locally to console
- [x] Logging to App Insights via Dapr Observability for ACA deployment

## OrdersService
- [x] Basic scaffolding for OrdersService
- [x] Get Orders
- [x] Get Order by id
- [x] Create new order
- [x] Logic for newly `Placed` order + publish `order-placed` event
- [x] Subscribe to and handle `stock-available` event to fill orders
- [x] Publish `stock-updated` event when order is fulfilled after `stock-available` event
- [x] Subscribe to and handle `order-fulfilled` event to update `Fulfilled` order
- [x] Subcsribe to and handle `order-backordered` event to update `Pending` order
- [x] Integrate Dapr for pubsub
- [x] Docker: Containerise basic OrdersService
- [x] Docker Compose: Configure containerisation for OrdersService stack (service + Dapr sidecar + redis streams)
- [x] Logging locally to console
- [x] Logging to App Insights via Dapr Observability for ACA deployment

## DevOps
- [x] Explore `azd`
- [x] Explore `az cli`
- [x] Create script to deploy infrastructure
- [x] Create script to deploy containerised ProductsService + Dapr sidecar
- [x] Update infrastructure script to create Service Bus topics for all event topics
- [x] Create script to deploy containerised OrdersService + Dapr sidecar