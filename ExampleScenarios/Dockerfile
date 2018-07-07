FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY ExampleScenarios/ExampleScenarios.csproj ExampleScenarios/
COPY DistributedPrimitives/DistributedPrimitives.csproj DistributedPrimitives/
COPY DistributedMonitor/DistributedMonitor.csproj DistributedMonitor/
RUN dotnet restore ExampleScenarios/ExampleScenarios.csproj
COPY . .
WORKDIR /src/ExampleScenarios
RUN dotnet build ExampleScenarios.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ExampleScenarios.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ExampleScenarios.dll"]
