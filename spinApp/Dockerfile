# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj first for better caching
COPY *.csproj .
RUN dotnet restore

# Copy remaining files
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "spinApp.dll"]