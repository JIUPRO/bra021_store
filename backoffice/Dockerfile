FROM node:20-alpine

WORKDIR /app

# Instalar Angular CLI globalmente
RUN npm install -g @angular/cli@17

# Copiar package.json e package-lock.json
COPY package*.json ./

# Instalar dependências
RUN npm install --legacy-peer-deps

# Copiar o restante do código
COPY . .

# Expor a porta
EXPOSE 4200

# Comando para iniciar o servidor de desenvolvimento
CMD ["ng", "serve", "--host", "0.0.0.0", "--port", "4200", "--poll", "2000"]
