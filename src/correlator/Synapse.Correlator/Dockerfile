FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
RUN apt-get update
RUN apt-get install -y jq
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/correlator/Synapse.Correlator/Synapse.Correlator.csproj", "src/correlator/Synapse.Correlator/"]
COPY ["src/core/Synapse.Core.Infrastructure/Synapse.Core.Infrastructure.csproj", "src/core/Synapse.Core.Infrastructure/"]
COPY ["src/core/Synapse.Core/Synapse.Core.csproj", "src/core/Synapse.Core/"]
RUN dotnet restore "./src/correlator/Synapse.Correlator/Synapse.Correlator.csproj"
COPY . .
WORKDIR "/src/src/correlator/Synapse.Correlator"
RUN dotnet build "./Synapse.Correlator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Synapse.Correlator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Correlator.dll"]