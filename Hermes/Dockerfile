FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY Hermes/*.csproj Hermes/
RUN dotnet restore "Hermes/Hermes.csproj"

# Copy the rest of the code
COPY . .
WORKDIR /source/Hermes
RUN dotnet build "Hermes.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hermes.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081

# Set the entry point
ENTRYPOINT ["dotnet", "Hermes.dll"]