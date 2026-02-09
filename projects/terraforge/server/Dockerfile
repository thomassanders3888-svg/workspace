FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY TerraForgeServer/*.csproj TerraForgeServer/
RUN dotnet restore TerraForgeServer/TerraForgeServer.csproj

# Copy and build
COPY . .
RUN dotnet publish TerraForgeServer -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Install Steamworks dependencies
RUN apt-get update && apt-get install -y \
    libsteam_api \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 7777/udp 7778/tcp

ENTRYPOINT ["dotnet", "TerraForgeServer.dll"]
