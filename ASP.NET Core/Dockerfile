FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
ENV SYNCFUSION_LICENSE_KEY=""
ENV SPELLCHECK_DICTIONARY_PATH=""
ENV SPELLCHECK_JSON_FILENAME=""
ENV SPELLCHECK_CACHE_COUNT=""
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /source
COPY ["src/EJ2APIServices.csproj", "./ej2-documenteditor-server/EJ2APIServices.csproj"]
COPY ["src/NuGet.Config", "./ej2-documenteditor-server/"]
RUN dotnet restore "./ej2-documenteditor-server/EJ2APIServices.csproj"
COPY . .
WORKDIR "/source/src"
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
RUN apk update \
    && apk --no-cache add icu-libs msttcorefonts-installer fontconfig && update-ms-fonts && fc-cache -f
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EJ2APIServices.dll"]
