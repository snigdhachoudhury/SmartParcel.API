# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . . 
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Use the runtime-only image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port your app will run on
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "SmartParcel.API.dll"]
