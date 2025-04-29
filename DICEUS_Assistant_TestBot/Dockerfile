# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy and restore
COPY ["DICEUS_Assistant_TestBot.csproj", "./"]
RUN dotnet restore "./DICEUS_Assistant_TestBot.csproj"

# Copy the rest
COPY . .

# Publish
RUN dotnet publish "DICEUS_Assistant_TestBot.csproj" -c Release -o /app/publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Required by Render to detect running service
ENV PORT=10000
EXPOSE 10000

# Run the bot
ENTRYPOINT ["dotnet", "DICEUS_Assistant_TestBot.dll"]
