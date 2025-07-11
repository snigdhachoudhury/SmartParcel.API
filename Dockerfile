# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the project file and restore
COPY ./SmartParcel.API/SmartParcel.API.csproj ./SmartParcel.API/
RUN dotnet restore ./SmartParcel.API/SmartParcel.API.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/SmartParcel.API
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartParcel.API.dll"]
