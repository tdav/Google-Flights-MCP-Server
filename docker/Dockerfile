FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY GoogleFlightsMcp.sln ./
COPY src/GoogleFlightsMcp/GoogleFlightsMcp.csproj src/GoogleFlightsMcp/
COPY src/GoogleFlightsApi/GoogleFlightsApi.csproj src/GoogleFlightsApi/
COPY tests/GoogleFlightsApi.Tests/GoogleFlightsApi.Tests.csproj tests/GoogleFlightsApi.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/src/GoogleFlightsApi
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GoogleFlightsApi.dll"]
