# ���������� SDK .NET 9 Preview ��� ������
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# �������� CSPROJ � ��������������� �����������
COPY *.csproj ./
RUN dotnet restore

# �������� �� ��������� � ���������
COPY . ./
RUN dotnet publish -c Release -o /app

# ���������� Runtime .NET 9 Preview ��� �������
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "gptChatOnline.dll"]
