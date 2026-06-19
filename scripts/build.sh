#!/bin/bash
# =============================================================================
# QM-AI 一键构建脚本
# =============================================================================
# 用法:
#   ./scripts/build.sh              # 构建所有服务
#   ./scripts/build.sh backend      # 仅构建后端
#   ./scripts/build.sh frontend     # 仅构建前端
#   ./scripts/build.sh ai-service   # 仅构建 AI 服务
#   ./scripts/build.sh infra        # 仅拉取基础设施镜像
#   ./scripts/build.sh up           # 构建 + 启动
# =============================================================================

set -e

# 启用 BuildKit（关键加速）
export DOCKER_BUILDKIT=1
export COMPOSE_DOCKER_CLI_BUILD=1
export BUILDKIT_PROGRESS=plain

SERVICE=${1:-all}

echo "🔧 QM-AI Build Tool"
echo "   DOCKER_BUILDKIT=1        ✅ 已启用"
echo "   COMPOSE_DOCKER_CLI_BUILD=1 ✅ 已启用"
echo ""

case "$SERVICE" in
  all)
    echo "📦 并行构建所有服务..."
    docker compose build --parallel
    ;;
  backend|frontend|ai-service)
    echo "📦 构建 $SERVICE ..."
    docker compose build "$SERVICE"
    ;;
  infra)
    echo "📦 拉取基础设施镜像..."
    docker compose pull mysql redis minio rabbitmq
    ;;
  up)
    echo "📦 构建 + 启动..."
    docker compose build --parallel
    docker compose up -d
    echo "✅ 启动完成"
    echo "   前端: http://localhost:5611"
    echo "   后端: http://localhost:5610/swagger"
    echo "   AI:   http://localhost:8000/health"
    ;;
  *)
    echo "❌ 未知参数: $SERVICE"
    echo "用法: $0 [all|backend|frontend|ai-service|infra|up]"
    exit 1
    ;;
esac

echo "✅ 完成"
