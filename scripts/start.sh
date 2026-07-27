#!/bin/bash
# =============================================================================
# QM-AI 本地启动脚本（非 Docker）
# =============================================================================
# 前置条件：
#   1. MySQL 已在本地运行（端口 3306）
#   2. Redis 已在本地运行（端口 6379）
#   3. Node.js >= 18
#   4. .NET 9 SDK
#   5. Python 3.10+
#
# 用法：
#   ./scripts/start.sh              # 启动前后端
#   ./scripts/start.sh --ai         # 启动前后端 + AI 服务
#   ./scripts/start.sh --check      # 仅检查环境依赖
#   ./scripts/start.sh --stop       # 停止所有本地进程
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# 颜色
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

info()  { echo -e "${BLUE}[INFO]${NC} $*"; }
ok()    { echo -e "${GREEN}[OK]${NC} $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $*"; }
error() { echo -e "${RED}[ERROR]${NC} $*"; }

# ─── 端口定义 ────────────────────────────────────────────────────────────────
BACKEND_PORT=5611
FRONTEND_PORT=5610
AI_PORT=8000

# PID 文件（用于 --stop）
BACKEND_PID_FILE="$SCRIPT_DIR/.backend.pid"
FRONTEND_PID_FILE="$SCRIPT_DIR/.frontend.pid"
AI_PID_FILE="$SCRIPT_DIR/.ai.pid"

# ─── 环境检查 ────────────────────────────────────────────────────────────────
check_command() {
    if command -v "$1" &>/dev/null; then
        local version
        version=$($1 --version 2>&1 | head -1)
        ok "$1 ($version)"
        return 0
    fi
    error "$1 未安装或未加入 PATH"
    return 1
}

check_port() {
    local port=$1
    local service=$2
    if lsof -i :"$port" &>/dev/null; then
        local pids
        pids=$(lsof -t -i :"$port" 2>/dev/null | tr -d '[:space:]')
        if [ -n "$pids" ]; then
            warn "$service 端口 $port 已被占用 (PIDs: $pids)，自动 kill..."
            kill -9 $pids 2>/dev/null || true
            sleep 1
            ok "$service 端口 $port 已释放"
        fi
    fi
    return 0
}

check_env() {
    echo "============================================"
    echo "  环境检查"
    echo "============================================"

    local fail=0

    echo ""
    echo "── 运行时依赖 ──"
    check_command node || fail=1
    check_command dotnet  || fail=1
    check_command python3 || fail=1

    echo ""
    echo "── 端口检查 ──"
    check_port $BACKEND_PORT "后端"
    check_port $FRONTEND_PORT "前端"

    echo ""
    echo "── 外部服务 ──"
    if mysqladmin ping -h localhost -u root -p"${MYSQL_ROOT_PASSWORD:-qmai_root_2024}" &>/dev/null 2>&1; then
        ok "MySQL localhost:3306 可连接"
    else
        warn "MySQL localhost:3306 不可连接（请确认 MySQL 已启动）"
    fi

    if redis-cli -h localhost -p 6379 ping &>/dev/null 2>&1; then
        ok "Redis localhost:6379 可连接"
    else
        warn "Redis localhost:6379 不可连接（请确认 Redis 已启动）"
    fi

    echo ""
    if [ $fail -eq 0 ]; then
        ok "环境检查通过"
    else
        error "环境检查未通过，请修复上述问题"
    fi
    return $fail
}

# ─── 后端 ────────────────────────────────────────────────────────────────────
start_backend() {
    echo ""
    echo "============================================"
    echo "  启动后端 (Backend) → http://localhost:$BACKEND_PORT"
    echo "============================================"

    local backend_dir="$PROJECT_ROOT/backend"

    # 恢复 NuGet 包
    if [ ! -d "$backend_dir/obj" ] || [ ! -f "$backend_dir/bin/Debug/net9.0/QM-AI.API.dll" ]; then
        info "正在恢复 .NET 包..."
        dotnet restore "$backend_dir/QM-AI.API.csproj"
    fi

    # 停止旧进程并清理端口
    kill_port $BACKEND_PORT "backend"

    cd "$backend_dir"
    dotnet run --project QM-AI.API.csproj &
    local pid=$!
    echo $pid > "$BACKEND_PID_FILE"
    ok "后端已启动 (PID: $pid)"
    cd - > /dev/null
}

# ─── 前端 ────────────────────────────────────────────────────────────────────
start_frontend() {
    echo ""
    echo "============================================"
    echo "  启动前端 (Frontend) → http://localhost:$FRONTEND_PORT"
    echo "============================================"

    local frontend_dir="$PROJECT_ROOT/frontend"

    # 安装依赖
    if [ ! -d "$frontend_dir/node_modules" ]; then
        info "正在安装前端依赖..."
        cd "$frontend_dir"
        npm install
        cd - > /dev/null
    fi

    # 停止旧进程并清理端口
    kill_port $FRONTEND_PORT "frontend"

    cd "$frontend_dir"
    npm run dev &
    local pid=$!
    echo $pid > "$FRONTEND_PID_FILE"
    ok "前端已启动 (PID: $pid)"
    cd - > /dev/null
}

# ─── AI 服务 ─────────────────────────────────────────────────────────────────
start_ai() {
    echo ""
    echo "============================================"
    echo "  启动 AI 服务 (AI Service) → http://localhost:$AI_PORT"
    echo "============================================"

    local ai_dir="$PROJECT_ROOT/ai-service"

    # 创建虚拟环境
    if [ ! -d "$ai_dir/venv" ]; then
        info "正在创建 Python 虚拟环境..."
        python3 -m venv "$ai_dir/venv"
    fi
    source "$ai_dir/venv/bin/activate"

    # 安装依赖
    info "正在安装 Python 依赖..."
    pip install -r "$ai_dir/requirements.txt" -q

    # 停止旧进程并清理端口
    kill_port $AI_PORT "ai"

    cd "$ai_dir"
    python -m uvicorn app.main:app --host 0.0.0.0 --port $AI_PORT &
    local pid=$!
    echo $pid > "$AI_PID_FILE"
    ok "AI 服务已启动 (PID: $pid)"
    cd - > /dev/null

    deactivate
}

# ─── 清理端口 ────────────────────────────────────────────────────────────────
kill_port() {
    local port=$1
    local service=$2

    # 通过 PID 文件先尝试优雅停止
    local pid_file=""
    case "$service" in
        backend) pid_file="$BACKEND_PID_FILE" ;;
        frontend) pid_file="$FRONTEND_PID_FILE" ;;
        ai) pid_file="$AI_PID_FILE" ;;
    esac

    if [ -n "$pid_file" ] && [ -f "$pid_file" ]; then
        local old_pid
        old_pid=$(cat "$pid_file")
        if kill -0 "$old_pid" 2>/dev/null; then
            info "正在停止 $service (PID: $old_pid)..."
            kill "$old_pid" 2>/dev/null || true
            sleep 2
            kill -0 "$old_pid" 2>/dev/null && kill -9 "$old_pid" 2>/dev/null || true
            ok "$service 已停止"
        fi
        rm -f "$pid_file"
    fi

    # 检查端口是否仍被占用
    if lsof -i :"$port" &>/dev/null; then
        local pids
        pids=$(lsof -t -i :"$port" 2>/dev/null | tr -d '[:space:]')
        if [ -n "$pids" ]; then
            warn "端口 $port 仍被占用 (PIDs: $pids)，强制 kill..."
            kill -9 $pids 2>/dev/null || true
            sleep 1
            ok "端口 $port 已释放"
        fi
    fi
}

stop_all() {
    echo "============================================"
    echo "  停止所有本地进程"
    echo "============================================"
    kill_port $BACKEND_PORT "backend"
    kill_port $FRONTEND_PORT "frontend"
    kill_port $AI_PORT "ai"
    ok "全部已停止"
}

# ─── 主入口 ──────────────────────────────────────────────────────────────────
usage() {
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  (无)          启动前后端（自动停止旧进程、释放端口）"
    echo "  --ai          启动前后端 + AI 服务"
    echo "  --check       仅检查环境依赖"
    echo "  --stop        停止所有本地进程并释放端口"
    echo "  --help        显示帮助"
}

main() {
    case "${1:-}" in
        --check)
            check_env
            ;;
        --stop)
            stop_all
            ;;
        --ai)
            check_env || exit 1
            start_backend
            start_frontend
            start_ai
            ;;
        --help)
            usage
            ;;
        "")
            check_env || exit 1
            start_backend
            start_frontend
            ;;
        *)
            error "未知参数: $1"
            usage
            exit 1
            ;;
    esac

    echo ""
    echo "============================================"
    echo "  QM-AI 已启动"
    echo "============================================"
    echo "  前端:  http://localhost:$FRONTEND_PORT"
    echo "  后端:  http://localhost:$BACKEND_PORT/swagger"
    if [ "${1:-}" = "--ai" ]; then
        echo "  AI:    http://localhost:$AI_PORT/docs"
    fi
    echo "  按 Ctrl+C 停止服务"
    echo "============================================"
}

main "$@"
