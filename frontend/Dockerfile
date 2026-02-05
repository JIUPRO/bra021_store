FROM node:20-alpine AS build

WORKDIR /app

# Copiar package.json e package-lock.json
COPY package*.json ./

# Instalar dependências
RUN npm install --legacy-peer-deps

# Copiar o restante do código
COPY . .

# Build de produção
RUN npm run build -- --configuration production

FROM nginx:alpine

# Copiar build para o Nginx
COPY --from=build /app/dist/loja-virtual-frontend /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
