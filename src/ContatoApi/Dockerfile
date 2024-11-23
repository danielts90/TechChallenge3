# Etapa Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ContatoApi.csproj", "./"]
RUN dotnet restore "ContatoApi.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContatoApi.dll"]
