# 🛒 Loja Brazil 021 - E-commerce de Jiu-Jitsu

Sistema completo de e-commerce especializado em produtos de Jiu-Jitsu, com integração ao MercadoPago e sistema de gestão administrativa.

## 🏗️ Arquitetura

```
loja-brazil-021/
├── backend/              # API .NET 8
├── frontend/             # Loja Virtual (Angular 17)
└── backoffice/          # Painel Administrativo (Angular 17)
```

## 🚀 Stack Tecnológica

### Backend
- **.NET 8** - API RESTful
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **MercadoPago SDK** - Pagamentos

### Frontend
- **Angular 17** - Framework SPA
- **Bootstrap 5** - UI/UX
- **Standalone Components**

## 📋 Pré-requisitos

- **Docker** e **Docker Compose**
- **Node.js 20+** (desenvolvimento local)
- **.NET 8 SDK** (desenvolvimento local)

## 🛠️ Instalação e Execução

### Desenvolvimento Local

1. **Clone o repositório**
```bash
git clone https://github.com/JIUPRO/bra021_store.git
cd bra021_store
```

2. **Configure as variáveis de ambiente**

Copie o arquivo de exemplo e preencha com suas credenciais:
```bash
cp .env.example .env
```

Edite o `.env` com suas configurações de banco, SMTP, etc.

3. **Inicie os containers**
```bash
docker-compose up -d
```

4. **Acesse as aplicações**
- **Loja Virtual**: http://localhost:4200
- **Backoffice**: http://localhost:4201
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

## 🔧 Comandos Úteis

```bash
# Parar containers
docker-compose down

# Ver logs
docker-compose logs -f

# Rebuild
docker-compose up -d --build
```

## 📱 Funcionalidades

### Loja Virtual
- ✅ Catálogo de produtos por categoria
- ✅ Carrinho de compras
- ✅ Integração MercadoPago
- ✅ Área do cliente
- ✅ Histórico de pedidos

### Backoffice
- ✅ Dashboard com estatísticas
- ✅ Gestão de produtos e categorias
- ✅ Gestão de pedidos e clientes
- ✅ Controle de estoque
- ✅ Relatórios de vendas

### API
- ✅ RESTful API
- ✅ Autenticação JWT
- ✅ Integração MercadoPago
- ✅ Notificações por email
- ✅ Controle transacional

## 🔒 Segurança

- ⚠️ **Nunca commite o arquivo `.env`**
- ⚠️ **Use variáveis de ambiente em produção**
- ✅ Senhas hasheadas
- ✅ CORS configurado
- ✅ Validação de dados

## 🚀 Deploy em Produção

### Docker Swarm + Portainer

As imagens são automaticamente geradas pelo GitHub Actions e publicadas no GHCR:

```yaml
# Exemplo de Stack no Portainer
services:
  api:
    image: ghcr.io/jiupro/bra021_store-backend:latest
    environment:
      ConnectionStrings__DefaultConnection: ${DB_CONNECTION_STRING}
      EmailSettings__SmtpServer: ${SMTP_SERVER}
      # ... outras variáveis
```

**Configure as variáveis de ambiente no Portainer** antes de fazer o deploy.

## 📚 Documentação da API

Acesse o Swagger em: `/swagger`

### Endpoints Principais
- `GET /api/produtos` - Listar produtos
- `POST /api/pedidos` - Criar pedido
- `POST /api/clientes/login` - Autenticar

## 👨‍💻 Desenvolvimento

### Backend
```bash
cd backend/LojaVirtual.API
dotnet run
```

### Frontend
```bash
cd frontend
npm install
ng serve
```

### Backoffice
```bash
cd backoffice
npm install
ng serve --port 4201
```

## 📝 Licença

Projeto privado - Todos os direitos reservados.

---

**Desenvolvido para Brazil 021 Store**
