FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER root
RUN apt-get update
RUN apt-get install -y jq
RUN apt-get install -y nodejs
RUN apt-get install -y python3
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/runner/Synapse.Runner/Synapse.Runner.csproj", "src/runner/Synapse.Runner/"]
COPY ["src/api/Synapse.Api.Client.Http/Synapse.Api.Client.Http.csproj", "src/api/Synapse.Api.Client.Http/"]
COPY ["src/api/Synapse.Api.Client.Core/Synapse.Api.Client.Core.csproj", "src/api/Synapse.Api.Client.Core/"]
COPY ["src/core/Synapse.Core/Synapse.Core.csproj", "src/core/Synapse.Core/"]
COPY ["src/core/Synapse.Core.Infrastructure/Synapse.Core.Infrastructure.csproj", "src/core/Synapse.Core.Infrastructure/"]
RUN dotnet restore "./src/runner/Synapse.Runner/Synapse.Runner.csproj"
COPY . .
WORKDIR "/src/src/runner/Synapse.Runner"
RUN dotnet build "./Synapse.Runner.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Synapse.Runner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Runner.dll"]