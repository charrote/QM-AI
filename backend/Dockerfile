# syntax=docker/dockerfile:1.4
# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY QM-AI.API.csproj ./
RUN dotnet restore QM-AI.API.csproj

COPY . .
RUN dotnet publish QM-AI.API.csproj -c Release -o /app --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# 安装 curl 用于健康检查
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:5611

EXPOSE 5611

ENTRYPOINT ["dotnet", "QM-AI.API.dll"]
