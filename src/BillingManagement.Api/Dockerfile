FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BillingManagement.slnx ./
COPY src/BillingManagement.Api/BillingManagement.Api.csproj src/BillingManagement.Api/
COPY src/BillingManagement.Application/BillingManagement.Application.csproj src/BillingManagement.Application/
COPY src/BillingManagement.Application.Abstractions/BillingManagement.Application.Abstractions.csproj src/BillingManagement.Application.Abstractions/
COPY src/BillingManagement.Contracts/BillingManagement.Contracts.csproj src/BillingManagement.Contracts/
COPY src/BillingManagement.Domain/BillingManagement.Domain.csproj src/BillingManagement.Domain/
COPY src/BillingManagement.Infrastructure/BillingManagement.Infrastructure.csproj src/BillingManagement.Infrastructure/
RUN dotnet restore src/BillingManagement.Api/BillingManagement.Api.csproj

COPY src/ src/
RUN dotnet publish src/BillingManagement.Api/BillingManagement.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BillingManagement.Api.dll"]

