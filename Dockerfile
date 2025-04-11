# Используем SDK .NET 9 Preview для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Копируем CSPROJ и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем всё остальное и публикуем
COPY . ./
RUN dotnet publish -c Release -o /app

# Используем Runtime .NET 9 Preview для запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "gptChatOnline.dll"]
