# syntax=docker/dockerfile:1.4
# ---- Build stage ----
FROM python:3.12-slim AS builder

WORKDIR /app

# 缓存 pip 包（BuildKit）
COPY requirements.txt .
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install --user -r requirements.txt

# ---- Runtime stage ----
FROM python:3.12-slim AS runtime

WORKDIR /app

# 安装 curl 用于健康检查
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# 复制 pip 安装的包到系统路径，避免权限问题
COPY --from=builder /root/.local /usr/local
COPY app/ app/

ENV PYTHONPATH=/app \
    PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
