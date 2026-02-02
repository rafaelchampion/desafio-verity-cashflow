# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Verity.CashFlow.API/Verity.CashFlow.API.csproj", "src/Verity.CashFlow.API/"]
COPY ["src/Verity.Core/Verity.Core.csproj", "src/Verity.Core/"]
COPY ["src/Verity.Contracts/Verity.Contracts.csproj", "src/Verity.Contracts/"]
RUN dotnet restore "src/Verity.CashFlow.API/Verity.CashFlow.API.csproj"

COPY . .
WORKDIR "/src/src/Verity.CashFlow.API"
RUN dotnet build "Verity.CashFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Verity.CashFlow.API.csproj" -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verity.CashFlow.API.dll"]
