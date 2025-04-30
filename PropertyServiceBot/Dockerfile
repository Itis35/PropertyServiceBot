# Используем официальный образ .NET для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Копируем файлы проекта и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем всё остальное и собираем проект
COPY . ./
RUN dotnet publish -c Release -o out

# Используем runtime-образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копируем собранные файлы из промежуточного образа
COPY --from=build-env /app/out .

# Устанавливаем переменные окружения (токен бота и другие настройки)
ENV BOT_TOKEN=your_telegram_bot_token
ENV ASPNETCORE_ENVIRONMENT=Production

# Запускаем приложение при старте контейнера
ENTRYPOINT ["dotnet", "PropertyServiceBot.dll"]

# Добавляем healthcheck для мониторинга
HEALTHCHECK --interval=30s --timeout=3s \
  CMD curl -f http://localhost:5000/health || exit 1