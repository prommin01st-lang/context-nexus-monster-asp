# Base image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["DevContextNexus.sln", "."]
COPY ["DevContextNexus.API/DevContextNexus.API.csproj", "DevContextNexus.API/"]

# Restore dependencies
RUN dotnet restore "DevContextNexus.API/DevContextNexus.API.csproj"

# Copy the rest of the code
COPY . .

# Build and Publish
WORKDIR "/src/DevContextNexus.API"
RUN dotnet build "DevContextNexus.API.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "DevContextNexus.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port (Cloud providers often inject PORT env var)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DevContextNexus.API.dll"]
