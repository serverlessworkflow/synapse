FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update
RUN apt-get install -y jq

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/runtime/Synapse.Runtime.Executor/Synapse.Runtime.Executor.csproj", "src/runtime/Synapse.Runtime.Executor/"]
COPY ["src/ports/Synapse.Ports.Grpc.Client/Synapse.Ports.Grpc.Client.csproj", "src/ports/Synapse.Ports.Grpc.Client/"]
COPY ["src/core/Synapse.Integration/Synapse.Integration.csproj", "src/core/Synapse.Integration/"]
RUN dotnet restore "src/runtime/Synapse.Runtime.Executor/Synapse.Runtime.Executor.csproj"
COPY . .
WORKDIR "/src/src/runtime/Synapse.Runtime.Executor"
RUN dotnet build "Synapse.Runtime.Executor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synapse.Runtime.Executor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Runtime.Executor.dll"]