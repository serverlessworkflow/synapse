#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Operator/Synapse.Operator.Runtime/Synapse.Operator.Runtime.csproj", "Operator/Synapse.Operator.Runtime/"]
COPY ["Operator/Synapse.Operator.Application/Synapse.Operator.Application.csproj", "Operator/Synapse.Operator.Application/"]
COPY ["BuildingBlocks/Synapse.Core/Synapse.Core.csproj", "BuildingBlocks/Synapse.Core/"]
RUN dotnet restore "Operator/Synapse.Operator.Runtime/Synapse.Operator.Runtime.csproj"
COPY . .
WORKDIR "/src/Operator/Synapse.Operator.Runtime"
RUN dotnet build "Synapse.Operator.Runtime.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synapse.Operator.Runtime.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Operator.Runtime.dll"]