# NOTE: should be run from solution folder

# This stage can be used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["./Directory.Build.props", "."]
COPY ["./Coinpedia.WebApi/Coinpedia.WebApi.csproj", "./Coinpedia.WebApi/"]
COPY ["./Coinpedia.Core/Coinpedia.Core.csproj", "./Coinpedia.Core/"]
RUN dotnet restore "./Coinpedia.WebApi/Coinpedia.WebApi.csproj"

COPY ["./Coinpedia.WebApi", "./Coinpedia.WebApi"]
COPY ["./Coinpedia.Core", "./Coinpedia.Core"]
WORKDIR "/src/Coinpedia.WebApi"
RUN dotnet build "./Coinpedia.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build


# publish the project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Coinpedia.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Coinpedia.WebApi.dll"]