FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER root
RUN apt-get update
RUN apt-get install -y jq
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["README.md", "README.md"]
COPY ["assets/images/transparent_logomark_256.png", "assets/images/transparent_logomark_256.png"]
COPY ["src/operator/Synapse.Operator/Synapse.Operator.csproj", "src/operator/Synapse.Operator/"]
COPY ["src/core/Synapse.Core.Infrastructure/Synapse.Core.Infrastructure.csproj", "src/core/Synapse.Core.Infrastructure/"]
COPY ["src/core/Synapse.Core/Synapse.Core.csproj", "src/core/Synapse.Core/"]
COPY ["src/runtime/Synapse.Runtime.Abstractions/Synapse.Runtime.Abstractions.csproj", "src/runtime/Synapse.Runtime.Abstractions/"]
RUN dotnet restore "./src/operator/Synapse.Operator/Synapse.Operator.csproj"
COPY . .
WORKDIR "/src/src/operator/Synapse.Operator"
RUN dotnet build "./Synapse.Operator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Synapse.Operator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Operator.dll"]