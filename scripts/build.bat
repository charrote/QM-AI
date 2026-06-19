@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

set "SERVICE=%~1"
if "%SERVICE%"=="" set "SERVICE=all"

set DOCKER_BUILDKIT=1
set COMPOSE_DOCKER_CLI_BUILD=1
set BUILDKIT_PROGRESS=plain

echo [QM-AI Build Tool]
echo   DOCKER_BUILDKIT=1                 OK
echo   COMPOSE_DOCKER_CLI_BUILD=1        OK
echo.

if /i "%SERVICE%"=="all" (
    echo Building all services...
    docker compose build --parallel
    goto :eof
)

if /i "%SERVICE%"=="infra" (
    echo Pulling infrastructure images...
    docker compose pull mysql redis minio rabbitmq
    goto :eof
)

if /i "%SERVICE%"=="up" (
    echo Building + Starting...
    docker compose build --parallel
    if %errorlevel% neq 0 (
        echo Build failed, aborting.
        goto :eof
    )
    docker compose up -d
    echo.
    echo Startup complete
    echo   Frontend: http://localhost:5610
    echo   Backend:  http://localhost:5611/swagger
    echo   AI:       http://localhost:8000/health
    goto :eof
)

if /i "%SERVICE%"=="backend" (
    echo Building backend...
    docker compose build backend
    goto :eof
)

if /i "%SERVICE%"=="frontend" (
    echo Building frontend...
    docker compose build frontend
    goto :eof
)

if /i "%SERVICE%"=="ai-service" (
    echo Building ai-service...
    docker compose build ai-service
    goto :eof
)

echo Unknown parameter: %SERVICE%
echo Usage: %~nx0 [all^|backend^|frontend^|ai-service^|infra^|up]
exit /b 1
