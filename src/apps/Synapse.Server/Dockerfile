#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 42286 41387
RUN apt-get update
RUN apt-get install -y jq

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/apps/Synapse.Server/Synapse.Server.csproj", "src/apps/Synapse.Server/"]
COPY ["src/apis/runtime/Synapse.Apis.Runtime.Grpc/Synapse.Apis.Runtime.Grpc.csproj", "src/apis/runtime/Synapse.Apis.Runtime.Grpc/"]
COPY ["src/core/Synapse.Application/Synapse.Application.csproj", "src/core/Synapse.Application/"]
COPY ["src/core/Synapse.Infrastructure/Synapse.Infrastructure.csproj", "src/core/Synapse.Infrastructure/"]
COPY ["src/core/Synapse.Domain/Synapse.Domain.csproj", "src/core/Synapse.Domain/"]
COPY ["src/core/Synapse.Integration/Synapse.Integration.csproj", "src/core/Synapse.Integration/"]
COPY ["src/apis/runtime/Synapse.Apis.Runtime.Core/Synapse.Apis.Runtime.Core.csproj", "src/apis/runtime/Synapse.Apis.Runtime.Core/"]
COPY ["src/apis/runtime/Synapse.Apis.Runtime.Grpc.Client/Synapse.Apis.Runtime.Grpc.Client.csproj", "src/apis/runtime/Synapse.Apis.Runtime.Grpc.Client/"]
COPY ["src/apis/management/Synapse.Apis.Management.Http/Synapse.Apis.Management.Http.csproj", "src/apis/management/Synapse.Apis.Management.Http/"]
COPY ["src/apis/management/Synapse.Apis.Management.Http.Client/Synapse.Apis.Management.Http.Client.csproj", "src/apis/management/Synapse.Apis.Management.Http.Client/"]
COPY ["src/apis/management/Synapse.Apis.Management.Core/Synapse.Apis.Management.Core.csproj", "src/apis/management/Synapse.Apis.Management.Core/"]
COPY ["src/apis/monitoring/Synapse.Apis.Monitoring.WebSocket/Synapse.Apis.Monitoring.WebSocket.csproj", "src/apis/monitoring/Synapse.Apis.Monitoring.WebSocket/"]
COPY ["src/apis/monitoring/Synapse.Apis.Monitoring.Core/Synapse.Apis.Monitoring.Core.csproj", "src/apis/monitoring/Synapse.Apis.Monitoring.Core/"]
COPY ["src/runtime/Synapse.Runtime.Docker/Synapse.Runtime.Docker.csproj", "src/runtime/Synapse.Runtime.Docker/"]
COPY ["src/runtime/Synapse.Runtime.Kubernetes/Synapse.Runtime.Kubernetes.csproj", "src/runtime/Synapse.Runtime.Kubernetes/"]
COPY ["src/apis/management/Synapse.Apis.Management.Grpc/Synapse.Apis.Management.Grpc.csproj", "src/apis/management/Synapse.Apis.Management.Grpc/"]
COPY ["src/apis/management/Synapse.Apis.Management.Grpc.Client/Synapse.Apis.Management.Grpc.Client.csproj", "src/apis/management/Synapse.Apis.Management.Grpc.Client/"]
COPY ["src/dashboard/Synapse.Dashboard/Synapse.Dashboard.csproj", "src/dashboard/Synapse.Dashboard/"]
COPY ["src/runtime/Synapse.Runtime.Native/Synapse.Runtime.Native.csproj", "src/runtime/Synapse.Runtime.Native/"]
RUN dotnet restore "src/apps/Synapse.Server/Synapse.Server.csproj"
COPY . .
WORKDIR "/src/src/apps/Synapse.Server"
RUN dotnet build "Synapse.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synapse.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.dll"]