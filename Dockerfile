# Stage 1: Restore & Build Release artifact
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution & csproj files trước để tận dụng Docker layer cache cho dotnet restore
COPY ["TheEcosystem.sln", "./"]
COPY ["src/Ecosystem.Domain/Ecosystem.Domain.csproj", "src/Ecosystem.Domain/"]
COPY ["src/Ecosystem.Application/Ecosystem.Application.csproj", "src/Ecosystem.Application/"]
COPY ["src/Ecosystem.Infrastructure/Ecosystem.Infrastructure.csproj", "src/Ecosystem.Infrastructure/"]
COPY ["src/Ecosystem.WebAPI/Ecosystem.WebAPI.csproj", "src/Ecosystem.WebAPI/"]

RUN dotnet restore "src/Ecosystem.WebAPI/Ecosystem.WebAPI.csproj"

# Copy toàn bộ mã nguồn tầng src/ và publish bản Release
COPY ["src/", "src/"]
WORKDIR "/src/src/Ecosystem.WebAPI"
RUN dotnet publish "Ecosystem.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime gọn nhẹ & chạy bằng non-root user
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Chuẩn bị thư mục dữ liệu SQLite & thư mục uploads với quyền cho non-root user 'app'
RUN mkdir -p /app/data /app/wwwroot/uploads && chown -R app:app /app

COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Ecosystem.WebAPI.dll"]
