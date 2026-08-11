FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore ./AuditCore.slnx
RUN dotnet publish ./src/AuditCore.Api/AuditCore.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

RUN mkdir -p /var/lib/auditcore/evidence

COPY --from=build /app/publish .

EXPOSE 8080
VOLUME ["/var/lib/auditcore/evidence"]

ENTRYPOINT ["dotnet", "AuditCore.Api.dll"]
