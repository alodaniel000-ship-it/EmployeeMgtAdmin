FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EmployeeMgtAdmin/EmployeeMgtAdmin.csproj", "EmployeeMgtAdmin/"]
RUN dotnet restore "EmployeeMgtAdmin/EmployeeMgtAdmin.csproj"
COPY . .
WORKDIR "/src/EmployeeMgtAdmin"
RUN dotnet build "./EmployeeMgtAdmin.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EmployeeMgtAdmin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeMgtAdmin.dll"]
