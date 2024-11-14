FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["README.md", "README.md"]
COPY ["assets/images/transparent_logomark_256.png", "assets/images/transparent_logomark_256.png"]
COPY ["src/api/Synapse.Api.Server/Synapse.Api.Server.csproj", "src/api/Synapse.Api.Server/"]
RUN dotnet restore "./src/api/Synapse.Api.Server/Synapse.Api.Server.csproj"
COPY . .
WORKDIR "/src/src/api/Synapse.Api.Server"
RUN dotnet build "./Synapse.Api.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Synapse.Api.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Api.Server.dll"]