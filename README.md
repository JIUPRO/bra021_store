# Loja Virtual - Sistema Completo de E-commerce

Sistema completo de e-commerce com Frontend Angular, Backend .NET e Backoffice Administrativo.

## 🏗️ Arquitetura do Sistema

```
loja-virtual/
├── backend/                  # API .NET 8
│   ├── LojaVirtual.Dominio/      # Entidades e Interfaces
│   ├── LojaVirtual.Aplicacao/    # DTOs, Serviços e Mapeamentos
│   ├── LojaVirtual.Infraestrutura/ # EF Core, Repositórios, Notificações
│   └── LojaVirtual.API/          # Controllers e Configuração
├── frontend/                 # Loja Virtual (Angular 17)
└── backoffice/              # Painel Administrativo (Angular 17)
```

## 🚀 Tecnologias Utilizadas

### Backend
- **.NET 8** - Framework principal
- **Entity Framework Core 8** - ORM para acesso a dados
- **SQL Server** - Banco de dados
- **AutoMapper** - Mapeamento de objetos
- **MailKit/MimeKit** - Envio de emails
- **Arquitetura em Camadas** - Domain, Application, Infrastructure, API

### Frontend
- **Angular 17** - Framework SPA
- **Bootstrap 5** - Framework CSS
- **Bootstrap Icons** - Ícones
- **Standalone Components** - Componentes standalone
- **RxJS** - Programação reativa

### Infraestrutura
- **Docker** - Containerização
- **Docker Compose** - Orquestração de containers

## 📋 Pré-requisitos

- Docker e Docker Compose instalados
- VS Code (recomendado)
- Extensão Docker para VS Code (opcional)

## 🛠️ Configuração Inicial

### 1. Clone ou extraia o projeto

```bash
cd loja-virtual
```

### 2. Configure as variáveis de ambiente (opcional)

Edite o arquivo `docker-compose.yml` para configurar:
- Senha do SQL Server
- Configurações de email (SMTP)
- Configurações de WhatsApp (API)

### 3. Inicie os containers

```bash
docker-compose up -d
```

Este comando irá:
- Baixar as imagens necessárias
- Compilar o backend .NET
- Compilar os frontends Angular
- Iniciar o SQL Server
- Executar as migrations
- Iniciar todos os serviços

### 4. Acesse as aplicações

| Aplicação | URL | Descrição |
|-----------|-----|-----------|
| Loja Virtual | http://localhost:4200 | Frontend da loja |
| Backoffice | http://localhost:4201 | Painel administrativo |
| API | http://localhost:5000 | Backend API |
| Swagger | http://localhost:5000/swagger | Documentação da API |

## 📁 Estrutura do Backend

### Entidades (Dominio)
- **Produto** - Produtos da loja
- **Categoria** - Categorias de produtos
- **Cliente** - Clientes cadastrados
- **Pedido** - Pedidos de venda
- **ItemPedido** - Itens de cada pedido
- **MovimentacaoEstoque** - Controle de estoque

### Serviços (Aplicacao)
- **ServicoProduto** - Gestão de produtos
- **ServicoCategoria** - Gestão de categorias
- **ServicoCliente** - Gestão de clientes
- **ServicoPedido** - Gestão de pedidos
- **ServicoEstoque** - Controle de estoque

### Notificações (Infraestrutura)
- **Email** - Notificações por email (SMTP)
- **WhatsApp** - Notificações por WhatsApp (API)

## 🔧 Comandos Úteis

### Docker Compose

```bash
# Iniciar todos os serviços
docker-compose up -d

# Parar todos os serviços
docker-compose down

# Ver logs
docker-compose logs -f

# Rebuildar containers
docker-compose up -d --build

# Executar migrations manualmente
docker-compose exec backend dotnet ef database update
```

### Backend (.NET)

```bash
cd backend/LojaVirtual.API

# Executar localmente (fora do Docker)
dotnet run

# Criar nova migration
dotnet ef migrations add NomeMigration -p ../LojaVirtual.Infraestrutura -s .

# Atualizar banco de dados
dotnet ef database update
```

### Frontend (Angular)

```bash
cd frontend

# Instalar dependências
npm install

# Executar localmente
ng serve

# Build de produção
ng build --configuration production
```

## 📱 Funcionalidades

### Loja Virtual (Frontend)
- ✅ Listagem de produtos
- ✅ Produtos em destaque
- ✅ Filtro por categoria
- ✅ Pesquisa de produtos
- ✅ Carrinho de compras
- ✅ Checkout
- ✅ Cadastro de clientes
- ✅ Login/Autenticação
- ✅ Histórico de pedidos
- ✅ Design responsivo (mobile/desktop)

### Backoffice (Administração)
- ✅ Dashboard com estatísticas
- ✅ Gestão de produtos (CRUD)
- ✅ Gestão de categorias
- ✅ Gestão de pedidos
- ✅ Controle de estoque
- ✅ Gestão de clientes
- ✅ Relatórios

### Backend (API)
- ✅ RESTful API
- ✅ Autenticação
- ✅ CRUD completo
- ✅ Controle de estoque
- ✅ Notificações por email
- ✅ Notificações por WhatsApp
- ✅ Migrations automáticas

## 🔔 Notificações

### Configuração de Email

Edite o `appsettings.json` do backend:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "Username": "seu-email@gmail.com",
  "Password": "sua-senha-app",
  "FromEmail": "seu-email@gmail.com",
  "ToEmail": "admin@loja.com"
}
```

### Configuração de WhatsApp

Edite o `appsettings.json` do backend:

```json
"WhatsAppSettings": {
  "ApiUrl": "https://api.whatsapp.com/v1/messages",
  "ApiKey": "sua-chave-api",
  "PhoneNumber": "5511999999999"
}
```

**Nota:** Para testes sem integração real, o sistema simula o envio no console.

## 🗄️ Banco de Dados

### SQL Server no Docker

- **Servidor**: `localhost,1433`
- **Banco**: `LojaVirtual`
- **Usuário**: `sa`
- **Senha**: `SenhaForte123!` (configurável no docker-compose.yml)

### Conexão String

```
Server=localhost,1433;Database=LojaVirtual;User Id=sa;Password=SenhaForte123!;TrustServerCertificate=True;
```

## 🔒 Segurança

- Senhas armazenadas com hash SHA256
- CORS configurado para permitir requisições do frontend
- Validação de dados nas APIs
- Transações para operações críticas

## 🐛 Troubleshooting

### Problema: SQL Server não inicia

```bash
# Verificar logs
docker-compose logs sqlserver

# Verificar se a porta 1433 está livia
netstat -an | grep 1433
```

### Problema: Migrations não executam

```bash
# Executar manualmente
docker-compose exec backend dotnet ef database update --project /src/LojaVirtual.Infraestrutura --startup-project /src/LojaVirtual.API
```

### Problema: Frontend não carrega

```bash
# Verificar logs
docker-compose logs frontend

# Rebuildar
docker-compose up -d --build frontend
```

## 📚 Documentação da API

Acesse o Swagger UI em: http://localhost:5000/swagger

### Endpoints Principais

| Endpoint | Descrição |
|----------|-----------|
| GET /api/produtos | Listar produtos |
| GET /api/produtos/destaques | Produtos em destaque |
| POST /api/pedidos | Criar pedido |
| GET /api/pedidos | Listar pedidos |
| GET /api/clientes | Listar clientes |
| POST /api/clientes/login | Autenticar cliente |

## 📝 Licença

Este projeto é apenas para fins educacionais e demonstração.

## 👨‍💻 Desenvolvimento

Para desenvolvimento local sem Docker:

1. Inicie o SQL Server
2. Execute o backend: `cd backend/LojaVirtual.API && dotnet run`
3. Execute o frontend: `cd frontend && ng serve`
4. Execute o backoffice: `cd backoffice && ng serve --port 4201`

---

**Desenvolvido com ❤️ usando Angular + .NET**
