# Running list of tasks to comlete

## Foundation
- [x] Architecture diagram (microservices, Dapr, Service Bus)
- [x] Service orchestration plan (service components and communications)
- [x] Scaffold .net API
- [x] Init git repo
- [x] Establish README
- [ ] Remove default WeatherForecast elements
- [ ] Logging/observability plan

## ProductsService
- [x] Basic scaffolding for ProductsService
- [x] Get Products
- [x] Get Product by id
- [x] Logic for stock update for product + publish `stock-available` event
- [ ] Subscribe to and handle `order-placed` event
- [ ] Logic for order fulfilled for order + publish `order-fulfilled` event
- [ ] Logic for back ordered orders + publish `order-backordered` event
- [x] Docker: Containerise basic ProductsService
- [x] Integrate Dapr for pubsub
- [x] Docker Compose: Configure containerisation for ProductsService stack (service + Dapr sidecar + redis streams)
- [ ] Logging for ProductsService

## OrdersService
- [ ] Basic scaffolding for OrdersService
- [ ] Get Orders
- [ ] Get Order by id
- [ ] Create new order
- [ ] Logic for newly `Placed` order + publish `order-placed` event
- [ ] Subscribe to and handle `stock-available` event to fill orders
- [ ] Subscribe to and handle `order-fulfilled` event to update `Fulfilled` order
- [ ] Subcsribe to and handle `order-backordered` event to update `Pending` order
- [ ] Docker: Containerise basiv OrdersService
- [ ] Integrate Dapr for pubsub
- [ ] Docker Compose: Configure containerisation for OrdersService stack (service + Dapr sidecar + redis streams)
- [ ] Logging for OrdersService

## DevOps
- [x] Explore `azd`
- [x] Explore `az cli`
- [x] Create script to deploy infrastructure
- [x] Create script to deploy containerised ProductsService + Dapr sidecar
- [ ] Create script to deploy containerised OrdersService + Dapr sidecar