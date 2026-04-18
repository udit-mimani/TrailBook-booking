FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TrailBook.Api/TrailBook.Api.csproj", "TrailBook.Api/"]
COPY ["TrailBook.Domain/TrailBook.Domain.csproj", "TrailBook.Domain/"]
COPY ["TrailBook.Infrastructure/TrailBook.Infrastructure.csproj", "TrailBook.Infrastructure/"]
RUN dotnet restore "TrailBook.Api/TrailBook.Api.csproj"
COPY . .
WORKDIR "/src/TrailBook.Api"
RUN dotnet build "TrailBook.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrailBook.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrailBook.Api.dll"]