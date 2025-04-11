FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# ?? Имя файла исправлено на корректное название csproj
COPY gptChatOnline.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out ./
# ?? DLL также должна соответствовать имени проекта
ENTRYPOINT ["dotnet", "gptChatOnline.dll"]

