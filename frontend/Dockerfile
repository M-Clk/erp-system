FROM node:22-alpine AS build
WORKDIR /app

# pnpm'i etkinleştir
RUN corepack enable && corepack prepare pnpm@latest --activate

# Önce bağımlılık dosyalarını kopyala (cache katmanı için)
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

# Sonra kaynak kodu kopyala ve build et
COPY . .
RUN pnpm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
