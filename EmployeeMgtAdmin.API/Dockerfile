FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EmployeeMgtAdmin.API/EmployeeMgtAdmin.API.csproj", "EmployeeMgtAdmin.API/"]
COPY ["EmployeeMgtAdmin.Application/EmployeeMgtAdmin.Application.csproj", "EmployeeMgtAdmin.Application/"]
COPY ["EmployeeMgtAdmin.Domain/EmployeeMgtAdmin.Domain.csproj", "EmployeeMgtAdmin.Domain/"]
COPY ["EmployeeMgtAdmin.Infrastructure/EmployeeMgtAdmin.Infrastructure.csproj", "EmployeeMgtAdmin.Infrastructure/"]
RUN dotnet restore "EmployeeMgtAdmin.API/EmployeeMgtAdmin.API.csproj"
COPY . .
WORKDIR "/src/EmployeeMgtAdmin.API"
RUN dotnet build "EmployeeMgtAdmin.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EmployeeMgtAdmin.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeMgtAdmin.API.dll"]
