# syntax=docker/dockerfile:1.4
# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 先复制 .csproj，利用 layer 缓存（不改依赖时跳过 restore）
COPY QM-AI.API.csproj .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore

# 再复制源码并发布
COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S qmai && adduser -S -G qmai qmai

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5611 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5611

USER qmai

ENTRYPOINT ["dotnet", "QM-AI.API.dll"]
