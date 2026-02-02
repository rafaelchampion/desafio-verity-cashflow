FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Verity.Web/Verity.Web.csproj", "src/Verity.Web/"]
COPY ["src/Verity.Contracts/Verity.Contracts.csproj", "src/Verity.Contracts/"]
RUN dotnet restore "src/Verity.Web/Verity.Web.csproj"
COPY . .
WORKDIR "/src/src/Verity.Web"
RUN dotnet build "Verity.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Verity.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY src/Verity.Web/nginx.conf /etc/nginx/conf.d/default.conf
