#!/bin/bash
# =============================================================================
# QM-AI 开发构建脚本 — 只构建前端 + 后端，跳过基础设施
# 用法:
#   ./build-dev.sh            # 构建前端 + 后端
#   ./build-dev.sh --no-cache # 强制重新构建（不使用缓存）
# =============================================================================

set -e

NO_CACHE=""
if [[ "$1" == "--no-cache" ]]; then
  NO_CACHE="--no-cache"
  echo "🔨 强制重新构建（不使用缓存）..."
else
  echo "🔨 构建前端 + 后端容器..."
fi

# 并行构建 frontend 和 backend
docker compose build $NO_CACHE --parallel frontend backend

echo ""
echo "🔄 重新启动前端 + 后端容器..."
docker compose up -d --no-deps --force-recreate frontend backend

echo ""
echo "✅ 完成！前端 + 后端已重新部署"
echo "查看日志:  docker compose logs -f frontend backend"
