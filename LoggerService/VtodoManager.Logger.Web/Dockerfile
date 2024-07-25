FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LoggerService/VtodoManager.Logger.Web/VtodoManager.Logger.Web.csproj", "VtodoManager.Logger.Web/"]
RUN dotnet restore "VtodoManager.Logger.Web/VtodoManager.Logger.Web.csproj"
COPY . .
WORKDIR "/src/LoggerService/VtodoManager.Logger.Web"
RUN dotnet build "VtodoManager.Logger.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VtodoManager.Logger.Web.csproj" -c Release -o /app/publish

RUN dotnet tool install --global dotnet-ef --version 8.0.3
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VtodoManager.Logger.Web.dll"]
