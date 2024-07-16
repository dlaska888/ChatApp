FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["ChatAppBackend.Api/ChatAppBackend.Api.csproj", "ChatAppBackend.Api/"]
RUN dotnet restore "ChatAppBackend.Api/ChatAppBackend.Api.csproj"
COPY . .
WORKDIR "/src/ChatAppBackend.Api"
RUN dotnet build "ChatAppBackend.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatAppBackend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatAppBackend.Api.dll"]
