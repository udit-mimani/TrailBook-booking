FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GoTyolo.Api/GoTyolo.Api.csproj", "GoTyolo.Api/"]
COPY ["GoTyolo.Domain/GoTyolo.Domain.csproj", "GoTyolo.Domain/"]
COPY ["GoTyolo.Infrastructure/GoTyolo.Infrastructure.csproj", "GoTyolo.Infrastructure/"]
RUN dotnet restore "GoTyolo.Api/GoTyolo.Api.csproj"
COPY . .
WORKDIR "/src/GoTyolo.Api"
RUN dotnet build "GoTyolo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GoTyolo.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoTyolo.Api.dll"]