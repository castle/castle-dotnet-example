# Build and run the ASP.NET Core CastleDemo sample.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/CastleDemo/CastleDemo.csproj CastleDemo/
RUN dotnet restore CastleDemo/CastleDemo.csproj
COPY src/CastleDemo/ CastleDemo/
RUN dotnet publish CastleDemo/CastleDemo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CastleDemo.dll"]
