#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
RUN apt-get update
RUN apt-get install -y jq

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/apps/Synapse.Worker/Synapse.Worker.csproj", "src/apps/Synapse.Worker/"]
COPY ["src/apis/runtime/Synapse.Apis.Runtime.Grpc.Client/Synapse.Apis.Runtime.Grpc.Client.csproj", "src/apis/runtime/Synapse.Apis.Runtime.Grpc.Client/"]
COPY ["src/apis/runtime/Synapse.Apis.Runtime.Core/Synapse.Apis.Runtime.Core.csproj", "src/apis/runtime/Synapse.Apis.Runtime.Core/"]
COPY ["src/core/Synapse.Integration/Synapse.Integration.csproj", "src/core/Synapse.Integration/"]
COPY ["src/apis/management/Synapse.Apis.Management.Grpc.Client/Synapse.Apis.Management.Grpc.Client.csproj", "src/apis/management/Synapse.Apis.Management.Grpc.Client/"]
COPY ["src/apis/management/Synapse.Apis.Management.Core/Synapse.Apis.Management.Core.csproj", "src/apis/management/Synapse.Apis.Management.Core/"]
RUN dotnet restore "src/apps/Synapse.Worker/Synapse.Worker.csproj"
COPY . .
WORKDIR "/src/src/apps/Synapse.Worker"
RUN dotnet build "Synapse.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synapse.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Worker.dll"]