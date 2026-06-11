# Install the Castle browser SDK from npm (served at runtime from node_modules).
FROM node:20-slim AS frontend
WORKDIR /app
COPY src/CastleDemo/package.json ./
RUN npm install --omit=dev --no-audit --no-fund

# Build and publish the ASP.NET Core CastleDemo sample.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/CastleDemo/CastleDemo.csproj CastleDemo/
RUN dotnet restore CastleDemo/CastleDemo.csproj
COPY src/CastleDemo/ CastleDemo/
RUN dotnet publish CastleDemo/CastleDemo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend /app/node_modules ./node_modules
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CastleDemo.dll"]
