# syntax=docker/dockerfile:1.4
# ---- Build stage ----
FROM node:22-alpine AS build
WORKDIR /app

# 缓存 npm 包（BuildKit）
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm \
    npm set cache /root/.npm && \
    npm ci --prefer-offline

# 复制源码并构建
COPY . .
RUN --mount=type=cache,target=/root/.npm \
    npx vite build

# ---- Runtime stage (Nginx) ----
FROM nginx:stable-alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 5610

CMD ["nginx", "-g", "daemon off;"]
