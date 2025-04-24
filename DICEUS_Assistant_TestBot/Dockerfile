# 1. Білд імедж з SDK для компіляції
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо .csproj і відновлюємо залежності
COPY ["DICEUS_Assistant_TestBot.csproj", "./"]
RUN dotnet restore "./DICEUS_Assistant_TestBot.csproj"

# Копіюємо все інше
COPY . .

# Публікуємо білд
RUN dotnet publish "DICEUS_Assistant_TestBot.csproj" -c Release -o /app/publish

# 2. Рантайм-імедж
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Запуск
ENTRYPOINT ["dotnet", "DICEUS_Assistant_TestBot.dll"]
