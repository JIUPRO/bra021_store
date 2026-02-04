using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using LojaVirtual.Infraestrutura.Data;
using LojaVirtual.Infraestrutura.Repositories;
using LojaVirtual.Infraestrutura.Services;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Aplicacao.Mapeamentos;
using LojaVirtual.Aplicacao.Services;
using LojaVirtual.Aplicacao.Servicos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar autenticação JWT
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey não configurada");
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LojaVirtual",
		ValidateAudience = true,
		ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LojaVirtualClient",
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

// Configurar CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("PermitirTudo", policy =>
	{
		policy.AllowAnyOrigin()
				 .AllowAnyMethod()
				 .AllowAnyHeader();
	});
});

// Configurar DbContext
builder.Services.AddDbContext<LojaDbContext>(options =>
	 options.UseSqlServer(
		  builder.Configuration.GetConnectionString("DefaultConnection"),
		  b => b.MigrationsAssembly("LojaVirtual.Infraestrutura")));

// Registrar Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrar Repositórios
builder.Services.AddScoped<PedidoRepository>();
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<ProdutoTamanhoRepository>();
builder.Services.AddScoped<MovimentacaoEstoqueRepository>();

// Registrar Serviços de Notificação
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

// Registrar Serviços da Aplicação
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IEscolaService, EscolaService>();
builder.Services.AddScoped<IParametroSistemaService, ParametroSistemaService>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<RelatorioService>();

// Registrar HttpClient Factory
builder.Services.AddHttpClient();

// Registrar Serviço de Pagamento
builder.Services.AddScoped<IServicoPagamento, ServicoPagamento>();

// Configurar AutoMapper
builder.Services.AddAutoMapper(typeof(MapeamentoPerfil));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirTudo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Aplicar migrações automaticamente e inserir dados padrão
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<LojaDbContext>();
	dbContext.Database.Migrate();

	// Inserir usuário administrativo padrão se não existir
	if (!dbContext.Usuarios.Any())
	{
		using (var sha256 = System.Security.Cryptography.SHA256.Create())
		{
			var senhaHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes("Admin@123")));
			dbContext.Usuarios.Add(new LojaVirtual.Dominio.Entidades.Usuario
			{
				Id = Guid.NewGuid(),
				Email = "adminbra021@rlm.dev.br",
				Nome = "Administrador",
				SenhaHash = senhaHash,
				Ativo = true,
				DataCriacao = DateTime.UtcNow
			});
			dbContext.SaveChanges();
		}
	}

	// Inserir parâmetros padrão se não existirem
	var parametrosPadrao = new List<LojaVirtual.Dominio.Entidades.ParametroSistema>
	{
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "TipoEnderecoEntrega",
			Valor = "Escola",
			Descricao = "Define como o endereço de entrega é tratado: Cliente, Escola ou Ambos",
			Tipo = "Lista",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "CarrosselImagem1",
			Valor = "",
			Descricao = "URL da primeira imagem do carrossel da home",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "CarrosselImagem2",
			Valor = "",
			Descricao = "URL da segunda imagem do carrossel da home",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "CarrosselImagem3",
			Valor = "",
			Descricao = "URL da terceira imagem do carrossel da home",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "CarrosselImagem4",
			Valor = "",
			Descricao = "URL da quarta imagem do carrossel da home",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		}
	};

	var chavesExistentes = dbContext.ParametrosSistema
		.Select(p => p.Chave)
		.ToHashSet();

	var parametrosParaInserir = parametrosPadrao
		.Where(p => !chavesExistentes.Contains(p.Chave))
		.ToList();

	if (parametrosParaInserir.Count > 0)
	{
		dbContext.ParametrosSistema.AddRange(parametrosParaInserir);
		dbContext.SaveChanges();
	}
}

app.Run();
