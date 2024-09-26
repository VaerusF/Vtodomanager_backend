FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8084

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NewsService/VtodoManager.NewsService.Web/VtodoManager.NewsService.Web.csproj", "VtodoManager.NewsService.Web/"]
RUN dotnet restore "VtodoManager.NewsService.Web/VtodoManager.NewsService.Web.csproj"
COPY . .
WORKDIR "/src/NewsService/VtodoManager.NewsService.Web"
RUN dotnet build "VtodoManager.NewsService.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VtodoManager.NewsService.Web.csproj" -c Release -o /app/publish

RUN dotnet tool install --global dotnet-ef --version 8.0.3
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VtodoManager.NewsService.Web.dll"]
