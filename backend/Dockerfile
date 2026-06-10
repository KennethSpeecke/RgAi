FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend.csproj", "./"]
RUN dotnet restore "backend.csproj"

COPY . ./
RUN dotnet publish "backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /app/data/logs /app/data/cache /app/data/uploads

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8000/health || exit 1

ENTRYPOINT ["dotnet", "backend.dll"]
