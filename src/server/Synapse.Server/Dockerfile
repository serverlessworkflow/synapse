#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80 8080
RUN apt-get update
RUN apt-get install -y jq

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/server/Synapse.Server/Synapse.Server.csproj", "src/server/Synapse.Server/"]
RUN dotnet restore "src/server/Synapse.Server/Synapse.Server.csproj"
COPY . .
WORKDIR "/src/src/server/Synapse.Server"
RUN dotnet build "Synapse.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synapse.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.Server.dll"]