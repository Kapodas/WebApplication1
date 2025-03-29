# Базовый образ для runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5221
ENV ASPNETCORE_URLS=http://*:5221

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ТОЛЬКО файл проекта сначала
COPY ["WebApplication1.csproj", "./"] 
RUN dotnet restore "WebApplication1.csproj"

# Копируем остальные файлы
COPY . .
RUN dotnet build "WebApplication1.csproj" -c Release -o /app/build

# Этап публикации
FROM build AS publish
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]