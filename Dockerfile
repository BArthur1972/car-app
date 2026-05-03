FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
EXPOSE 443
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

## restore
COPY ["backend/Cars.csproj", "backend/"]
RUN dotnet restore "backend/Cars.csproj"

## build
COPY . .
WORKDIR "/src/backend"
RUN dotnet build "Cars.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Cars.csproj" -c $BUILD_CONFIGURATION -o /app/publish --property:UseAppHost=false

## Run
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cars.dll"]
