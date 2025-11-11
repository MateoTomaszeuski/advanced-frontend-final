FROM node:20-alpine

WORKDIR /app

COPY app/client/package*.json app/client/pnpm-lock.yaml* ./client/

WORKDIR /app/client

RUN npm install -g pnpm && pnpm install

COPY app/client/src ./src
COPY app/client/public ./public
COPY app/client/*.json ./
COPY app/client/*.ts ./
COPY app/client/*.js ./
COPY app/client/*.html ./

CMD ["pnpm", "test"]
