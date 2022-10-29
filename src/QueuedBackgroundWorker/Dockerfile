FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["QueuedBackgroundWorker/QueuedBackgroundWorker.csproj", "QueuedBackgroundWorker/"]
RUN dotnet restore "QueuedBackgroundWorker/QueuedBackgroundWorker.csproj"
COPY . .
WORKDIR "/src/QueuedBackgroundWorker"
RUN dotnet build "QueuedBackgroundWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QueuedBackgroundWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueuedBackgroundWorker.dll"]
