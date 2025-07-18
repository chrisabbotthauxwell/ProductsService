# Orders Service

This project is a basic .NET 8 Web API for managing orders. It includes a controller for handling HTTP requests related to weather forecasts, serving as a template for further development of the Orders Service.

## Project Structure

- **Controllers**: Contains the `WeatherForecastController` which handles HTTP requests.
- **Models**: Contains the `WeatherForecast` class that represents the weather forecast data.
- **OrdersService.csproj**: The project file that defines dependencies and build settings.
- **Program.cs**: The entry point of the application, configuring the web host and middleware.
- **Properties**: Contains launch settings for different environments.
  
## Setup Instructions

1. Ensure you have the .NET 8 SDK installed.
2. Clone the repository or download the project files.
3. Navigate to the project directory.
4. Run the application using the command:
   ```
   dotnet run
   ```
5. Access the API at `http://localhost:5000/weatherforecast` to retrieve weather forecast data.

## Usage

The API currently supports the following endpoint:

- `GET /weatherforecast`: Returns an array of `WeatherForecast` objects.

## Future Development

This project will be expanded to include additional functionality related to order management, including creating, updating, and retrieving orders.