# Install the Castle browser SDK from npm (served at runtime from node_modules).
FROM node:20-slim AS frontend
WORKDIR /app
COPY src/CastleDemo/package.json ./
RUN npm install --omit=dev --no-audit --no-fund

# Pack Castle.Sdk from main into a local feed until 3.0.0 is on NuGet.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*
COPY scripts/set-sdk-version.sh scripts/
COPY src/CastleDemo/CastleDemo.csproj src/CastleDemo/
COPY src/CastleDemo.Framework/CastleDemo.Framework.csproj src/CastleDemo.Framework/
RUN ./scripts/set-sdk-version.sh main
COPY src/CastleDemo/ src/CastleDemo/
RUN dotnet publish src/CastleDemo/CastleDemo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend /app/node_modules ./node_modules
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CastleDemo.dll"]
