# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Verity.Consolidated.API/Verity.Consolidated.API.csproj", "src/Verity.Consolidated.API/"]
COPY ["src/Verity.Core/Verity.Core.csproj", "src/Verity.Core/"]
COPY ["src/Verity.Contracts/Verity.Contracts.csproj", "src/Verity.Contracts/"]
RUN dotnet restore "src/Verity.Consolidated.API/Verity.Consolidated.API.csproj"

COPY . .
WORKDIR "/src/src/Verity.Consolidated.API"
RUN dotnet build "Verity.Consolidated.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Verity.Consolidated.API.csproj" -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verity.Consolidated.API.dll"]
