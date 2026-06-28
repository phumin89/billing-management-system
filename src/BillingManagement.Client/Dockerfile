FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BillingManagement.slnx ./
COPY src/BillingManagement.Client/BillingManagement.Client.csproj src/BillingManagement.Client/
COPY src/BillingManagement.Contracts/BillingManagement.Contracts.csproj src/BillingManagement.Contracts/
RUN dotnet restore src/BillingManagement.Client/BillingManagement.Client.csproj

COPY src/ src/
RUN dotnet publish src/BillingManagement.Client/BillingManagement.Client.csproj -c Release -o /app/publish --no-restore

FROM nginx:1.27-alpine AS final
COPY src/BillingManagement.Client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80

