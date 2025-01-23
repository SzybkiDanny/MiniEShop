# Mini E-Shop

A microservices-based e-commerce solution built with .NET 9.

## Architecture

The solution consists of four microservices:

1. **Products Service** (port 5001)
   - Manages product catalog
   - Provides product information and search capabilities
   - Endpoints:
     * GET /api/products - List all products (supports ?nameLike filter)
     * GET /api/products/{id} - Get product details

2. **Inventory Service** (port 5002)
   - Manages product inventory and availability
   - Handles inventory reservations
   - Endpoints:
     * GET /api/inventory/{productId} - Get product availability
     * POST /api/inventory/reserve - Reserve inventory for products

3. **Orders Service** (port 5003)
   - Manages customer orders
   - Coordinates with Inventory service for availability checks
   - Endpoints:
     * GET /api/orders - List all orders
     * POST /api/orders - Create a new order

4. **Notifications Service** (port 5004)
   - Handles order notifications via Azure Service Bus
   - Processes notifications asynchronously

## Technical Stack

- .NET 9
- Azure Table Storage for persistence
- Azure Service Bus for messaging
- MediatR for CQRS pattern
- xUnit for testing

## Prerequisites

- .NET 9 SDK
- Azure Storage Account (or Azure Storage Emulator)
- Azure Service Bus namespace and queue

## Configuration

Each service has its own configuration in `appsettings.json`:

1. Products & Inventory & Orders services:
   ```json
   {
     "AzureStorage": {
       "ConnectionString": "UseDevelopmentStorage=true"
     }
   }
   ```

2. Orders Service also needs:
   ```json
   {
     "ServiceUrls": {
       "Products": "http://localhost:5001",
       "Inventory": "http://localhost:5002"
     }
   }
   ```

3. Notifications Service:
   ```json
   {
     "ServiceBus": {
       "ConnectionString": "your-service-bus-connection-string",
       "QueueName": "notifications"
     }
   }
   ```

## Running the Services

1. Start Azure Storage Emulator (if using local development)

2. Start each service (in separate terminals):
   ```bash
   cd src/MiniEShop.Products
   dotnet run

   cd src/MiniEShop.Inventory
   dotnet run

   cd src/MiniEShop.Orders
   dotnet run

   cd src/MiniEShop.Notifications
   dotnet run
   ```

3. Access the services:
   - Products API: http://localhost:5001/swagger
   - Inventory API: http://localhost:5002/swagger
   - Orders API: http://localhost:5003/swagger

## Testing

Run the tests:
```bash
dotnet test
```

## Session Management

The solution uses HTTP session cookies to track users between requests. Session IDs are automatically generated and stored in cookies.

## Data Storage

All services use Azure Table Storage:

- Products Service:
  * Table: Products
  * Blob Container: product-images

- Inventory Service:
  * Table: Inventory

- Orders Service:
  * Tables: Orders, OrderItems

## Message Flow

1. Customer creates an order (POST /api/orders)
2. Orders service checks product availability with Inventory service
3. If available, Inventory service reserves the items
4. Orders service creates the order and sends notification via Service Bus
5. Notifications service processes the message and logs notification details
