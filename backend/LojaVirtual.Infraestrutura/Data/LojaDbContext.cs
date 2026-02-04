using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Infraestrutura.Data
{
	public class LojaDbContext : DbContext
	{
		public LojaDbContext(DbContextOptions<LojaDbContext> options) : base(options)
		{
		}

		public DbSet<Categoria> Categorias { get; set; }
		public DbSet<Produto> Produtos { get; set; }
		public DbSet<ProdutoTamanho> ProdutoTamanhos { get; set; }
		public DbSet<Cliente> Clientes { get; set; }
		public DbSet<Pedido> Pedidos { get; set; }
		public DbSet<ItemPedido> ItensPedido { get; set; }
		public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
		public DbSet<Escola> Escolas { get; set; }
		public DbSet<ParametroSistema> ParametrosSistema { get; set; }
		public DbSet<Usuario> Usuarios { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configurações de Categoria
			modelBuilder.Entity<Categoria>(entity =>
			{
				entity.ToTable("Categorias");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
				entity.Property(e => e.Descricao).HasMaxLength(500);
				entity.Property(e => e.ImagemUrl).HasMaxLength(500);
				entity.HasIndex(e => e.OrdemExibicao);
			});

			// Configurações de Produto
			modelBuilder.Entity<Produto>(entity =>
			{
				entity.ToTable("Produtos");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
				entity.Property(e => e.Descricao).HasMaxLength(4000);
				entity.Property(e => e.DescricaoCurta).HasMaxLength(500);
				entity.Property(e => e.Preco).HasPrecision(18, 2);
				entity.Property(e => e.PrecoPromocional).HasPrecision(18, 2);
				entity.Property(e => e.ValorFrete).HasPrecision(18, 2);
				entity.Property(e => e.PrazoEntregaDias);
				entity.Property(e => e.ImagemUrl).HasMaxLength(500);

				entity.HasIndex(e => e.Nome);
				entity.HasIndex(e => e.Destaque);
				entity.HasIndex(e => e.Ativo);

				entity.HasOne(e => e.Categoria)
							.WithMany(c => c.Produtos)
							.HasForeignKey(e => e.CategoriaId)
							.OnDelete(DeleteBehavior.Restrict);
			});

			// Configurações de ProdutoTamanho
			modelBuilder.Entity<ProdutoTamanho>(entity =>
			{
				entity.ToTable("ProdutoTamanhos");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Tamanho).IsRequired().HasMaxLength(50);
				entity.HasIndex(e => e.ProdutoId);
				entity.HasIndex(e => new { e.ProdutoId, e.Tamanho }).IsUnique();

				entity.HasOne(e => e.Produto)
							.WithMany(p => p.Tamanhos)
							.HasForeignKey(e => e.ProdutoId)
							.OnDelete(DeleteBehavior.Cascade);
			});

			// Configurações de Cliente
			modelBuilder.Entity<Cliente>(entity =>
			{
				entity.ToTable("Clientes");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
				entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
				entity.Property(e => e.Telefone).HasMaxLength(20);
				entity.Property(e => e.Cpf).HasMaxLength(14);
				entity.Property(e => e.SenhaHash).HasMaxLength(500);
				entity.Property(e => e.TokenConfirmacaoEmail).HasMaxLength(500);
				entity.HasIndex(e => e.Email).IsUnique();
				entity.HasIndex(e => e.Cpf).IsUnique();

				// Endereço
				entity.Property(e => e.Cep).HasMaxLength(9);
				entity.Property(e => e.Logradouro).HasMaxLength(200);
				entity.Property(e => e.Numero).HasMaxLength(20);
				entity.Property(e => e.Complemento).HasMaxLength(100);
				entity.Property(e => e.Bairro).HasMaxLength(100);
				entity.Property(e => e.Cidade).HasMaxLength(100);
				entity.Property(e => e.Estado).HasMaxLength(2);
			});

			// Configurações de Pedido
			modelBuilder.Entity<Pedido>(entity =>
			{
				entity.ToTable("Pedidos");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.NumeroPedido).IsRequired().HasMaxLength(20);
				entity.Property(e => e.ValorSubtotal).HasPrecision(18, 2);
				entity.Property(e => e.ValorFrete).HasPrecision(18, 2);
				entity.Property(e => e.ValorDesconto).HasPrecision(18, 2);
				entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
				entity.Property(e => e.PrazoEntregaDias);
				entity.Property(e => e.Observacoes).HasMaxLength(1000);
				entity.Property(e => e.MetodoPagamento).HasMaxLength(20);
				entity.Property(e => e.NotaFiscalUrl).HasMaxLength(500);
				entity.HasIndex(e => e.NumeroPedido).IsUnique();
				entity.HasIndex(e => e.Status);
				entity.HasIndex(e => e.DataPedido);

				// Endereço de entrega
				entity.Property(e => e.NomeEntrega).IsRequired().HasMaxLength(200);
				entity.Property(e => e.TelefoneEntrega).IsRequired().HasMaxLength(20);
				entity.Property(e => e.CepEntrega).IsRequired().HasMaxLength(9);
				entity.Property(e => e.LogradouroEntrega).IsRequired().HasMaxLength(200);
				entity.Property(e => e.NumeroEntrega).IsRequired().HasMaxLength(20);
				entity.Property(e => e.ComplementoEntrega).HasMaxLength(100);
				entity.Property(e => e.BairroEntrega).IsRequired().HasMaxLength(100);
				entity.Property(e => e.CidadeEntrega).IsRequired().HasMaxLength(100);
				entity.Property(e => e.EstadoEntrega).IsRequired().HasMaxLength(2);

				entity.HasOne(e => e.Cliente)
							.WithMany(c => c.Pedidos)
							.HasForeignKey(e => e.ClienteId)
							.OnDelete(DeleteBehavior.Restrict);
			});

			// Configurações de ItemPedido
			modelBuilder.Entity<ItemPedido>(entity =>
			{
				entity.ToTable("ItensPedido");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.PrecoUnitario).HasPrecision(18, 2);
				entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
				entity.Property(e => e.Observacoes).HasMaxLength(500);

				entity.HasOne(e => e.Pedido)
							.WithMany(p => p.Itens)
							.HasForeignKey(e => e.PedidoId)
							.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.Produto)
							.WithMany(p => p.ItensPedido)
							.HasForeignKey(e => e.ProdutoId)
							.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(e => e.ProdutoTamanho)
						.WithMany(v => v.ItensPedido)
						.HasForeignKey(e => e.ProdutoTamanhoId)
							.OnDelete(DeleteBehavior.Restrict)
							.IsRequired(false);
			});

			// Configurações de MovimentacaoEstoque
			modelBuilder.Entity<MovimentacaoEstoque>(entity =>
			{
				entity.ToTable("MovimentacoesEstoque");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Motivo).HasMaxLength(500);
				entity.Property(e => e.Referencia).HasMaxLength(100);
				entity.HasIndex(e => e.DataMovimentacao);
				entity.HasIndex(e => e.Tipo);

				entity.HasOne(e => e.ProdutoTamanho)
						.WithMany(p => p.MovimentacoesEstoque)
						.HasForeignKey(e => e.ProdutoTamanhoId)
							.OnDelete(DeleteBehavior.Restrict);
			});

			// Configurações de Escola
			modelBuilder.Entity<Escola>(entity =>
			{
				entity.ToTable("Escolas");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
				entity.Property(e => e.Cep).HasMaxLength(10);
				entity.Property(e => e.Logradouro).HasMaxLength(200);
				entity.Property(e => e.Numero).HasMaxLength(20);
				entity.Property(e => e.Complemento).HasMaxLength(100);
				entity.Property(e => e.Bairro).HasMaxLength(100);
				entity.Property(e => e.Cidade).HasMaxLength(100);
				entity.Property(e => e.Estado).HasMaxLength(2);
				entity.Property(e => e.Contato).HasMaxLength(100);
				entity.Property(e => e.ProfessorResponsavel).HasMaxLength(200);
				entity.Property(e => e.PercentualComissao).HasPrecision(5, 2);
				entity.HasIndex(e => e.Nome);
				entity.HasIndex(e => e.Ativo);
			});

			// Configurações de ParametroSistema
			modelBuilder.Entity<ParametroSistema>(entity =>
			{
				entity.ToTable("ParametrosSistema");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Chave).IsRequired().HasMaxLength(100);
				entity.Property(e => e.Valor).IsRequired().HasMaxLength(500);
				entity.Property(e => e.Descricao).HasMaxLength(500);
				entity.Property(e => e.Tipo).HasMaxLength(50);
				entity.HasIndex(e => e.Chave).IsUnique();
			});

			// Configurações de Usuario
			modelBuilder.Entity<Usuario>(entity =>
			{
				entity.ToTable("Usuarios");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
				entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
				entity.Property(e => e.SenhaHash).IsRequired();
				entity.Property(e => e.Ativo).HasDefaultValue(true);
				entity.Property(e => e.DataCriacao).HasDefaultValueSql("GETUTCDATE()");
				entity.HasIndex(e => e.Email).IsUnique();
			});
		}
	}
}
