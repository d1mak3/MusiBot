FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /sources

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish --output /app/ --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=builder /app .
CMD ["dotnet", "MusiBotProd.dll"]