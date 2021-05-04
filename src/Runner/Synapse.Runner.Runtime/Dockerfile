FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Runner/Synapse.Runner.Runtime/Synapse.Runner.Runtime.csproj", "Runner/Synapse.Runner.Runtime/"]
RUN dotnet restore "Runner/Synapse.Runner.Runtime/Synapse.Runner.Runtime.csproj"
COPY . .
WORKDIR "/src/Runner/Synapse.Runner.Runtime"
RUN dotnet build "Synapse.Runner.Runtime.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synapse.Runner.Runtime.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update
RUN apt-get install -y jq
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Runner.Runtime.dll"]