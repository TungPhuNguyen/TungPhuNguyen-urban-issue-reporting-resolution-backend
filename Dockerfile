FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["UrbanIssue.API/UrbanIssue.API.csproj", "UrbanIssue.API/"]
COPY ["UrbanIssue.Application/UrbanIssue.Application.csproj", "UrbanIssue.Application/"]
COPY ["UrbanIssue.Domain/UrbanIssue.Domain.csproj", "UrbanIssue.Domain/"]
COPY ["UrbanIssue.Infrastructure.Sqlserver/UrbanIssue.Infrastructure.Sqlserver.csproj", "UrbanIssue.Infrastructure.Sqlserver/"]

RUN dotnet restore "UrbanIssue.API/UrbanIssue.API.csproj"

COPY . .

WORKDIR "/src/UrbanIssue.API"

RUN dotnet publish "UrbanIssue.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UrbanIssue.API.dll"]
