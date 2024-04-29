LABEL org.opencontainers.image.description="A web app for businesses to track their sales channels"

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Herta.csproj", "./"]
RUN --mount=type=secret,id=NUGET_API_KEY,required=true \
    export NUGET_API_KEY=$(cat /run/secrets/NUGET_API_KEY) && \
    dotnet nuget add source --username byroncustodio --password $NUGET_API_KEY --store-password-in-clear-text --name github "https://nuget.pkg.github.com/byroncustodio/index.json"
RUN dotnet restore "Herta.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Herta.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Herta.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Herta.dll"]
