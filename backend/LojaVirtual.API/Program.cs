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
builder.Services.AddHttpClient<IStorageService, StorageService>();

// Registrar Serviços da Aplicação
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IEscolaService, EscolaService>();
builder.Services.AddScoped<IParametroSistemaService, ParametroSistemaService>();
builder.Services.AddScoped<IFreteService, FreteService>();
builder.Services.AddScoped<ILogisticaService, LogisticaService>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<RelatorioService>();

// Registrar HttpClient Factory
builder.Services.AddHttpClient();

// Registrar Serviço de Pagamento
builder.Services.AddScoped<IServicoPagamento, ServicoPagamento>();

// Configurar Mapster
MapsterConfig.RegisterMappings();

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
			Secao = "Entrega",
			Tipo = "Lista",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "FreteHabilitado",
			Valor = "false",
			Descricao = "Liga ou desliga a rotina de frete configurável no checkout",
			Secao = "Frete",
			Tipo = "Boolean",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "FreteProvider",
			Valor = "Fixo",
			Descricao = "Define se o checkout usará frete fixo ou integração com Melhor Envio",
			Secao = "Frete",
			Tipo = "Lista",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "FreteCepOrigem",
			Valor = "",
			Descricao = "CEP de origem usado para cotação dinâmica de frete",
			Secao = "Frete",
			Tipo = "Cep",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "FretePrazoPreparacaoDias",
			Valor = "0",
			Descricao = "Prazo interno em dias para separação, emissão e postagem antes do prazo da transportadora",
			Secao = "Frete",
			Tipo = "Numero",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioNaoComercial",
			Valor = "true",
			Descricao = "Define se a etiqueta será gerada como envio não comercial para testes/sandbox",
			Secao = "Melhor Envio",
			Tipo = "Boolean",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteNome",
			Valor = "",
			Descricao = "Nome do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteTelefone",
			Valor = "",
			Descricao = "Telefone do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteEmail",
			Valor = "",
			Descricao = "Email do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteDocumento",
			Valor = "",
			Descricao = "CPF ou CNPJ do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteInscricaoEstadual",
			Valor = "ISENTO",
			Descricao = "Inscrição estadual do remetente usada na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteLogradouro",
			Valor = "",
			Descricao = "Logradouro do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteNumero",
			Valor = "",
			Descricao = "Número do endereço do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteComplemento",
			Valor = "",
			Descricao = "Complemento do endereço do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteBairro",
			Valor = "",
			Descricao = "Bairro do remetente usado na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteCidade",
			Valor = "",
			Descricao = "Cidade do remetente usada na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
			DataCriacao = DateTime.UtcNow,
			DataAtualizacao = DateTime.UtcNow,
			Ativo = true
		},
		new LojaVirtual.Dominio.Entidades.ParametroSistema
		{
			Id = Guid.NewGuid(),
			Chave = "MelhorEnvioRemetenteEstado",
			Valor = "",
			Descricao = "UF do remetente usada na geração da etiqueta do Melhor Envio",
			Secao = "Melhor Envio",
			Tipo = "String",
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
			Secao = "Home",
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
			Secao = "Home",
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
			Secao = "Home",
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
			Secao = "Home",
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
