FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо спочатку csproj для відновлення залежностей
COPY ["DICEUS_Assistant_TestBot/DICEUS_Assistant_TestBot.csproj", "./"]
RUN dotnet restore "DICEUS_Assistant_TestBot.csproj"

# Копіюємо всі файли проєкту, крім bin та obj (які вже виключені через .dockerignore)
COPY ["DICEUS_Assistant_TestBot/", "./"]

# Очищаємо директорію obj
RUN rm -rf obj/* || true
RUN mkdir -p obj/Release/net9.0/ || true

# Публікуємо проєкт
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DICEUS_Assistant_TestBot.dll"]