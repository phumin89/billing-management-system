FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BillingManagement.slnx ./
COPY src/BillingManagement.Migrator/BillingManagement.Migrator.csproj src/BillingManagement.Migrator/
COPY src/BillingManagement.Infrastructure/BillingManagement.Infrastructure.csproj src/BillingManagement.Infrastructure/
COPY src/BillingManagement.Application.Abstractions/BillingManagement.Application.Abstractions.csproj src/BillingManagement.Application.Abstractions/
COPY src/BillingManagement.Domain/BillingManagement.Domain.csproj src/BillingManagement.Domain/
RUN dotnet restore src/BillingManagement.Migrator/BillingManagement.Migrator.csproj

COPY src/ src/
RUN dotnet publish src/BillingManagement.Migrator/BillingManagement.Migrator.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BillingManagement.Migrator.dll"]

