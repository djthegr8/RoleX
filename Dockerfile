# Installs latest dotnet SDK
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
RUN echo ".NET 5 successfully fetched.\nNow restoring..."
WORKDIR /source

COPY Hermes/*.csproj .
RUN dotnet restore && echo "Restored successfully."

COPY Hermes/ .
RUN dotnet build Hermes.csproj -c Release --no-restore && echo "Build successful."

FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app && echo "Published successfully."

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Hermes.dll"]
