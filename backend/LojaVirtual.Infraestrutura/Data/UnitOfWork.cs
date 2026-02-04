using Microsoft.EntityFrameworkCore.Storage;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Repositories;

namespace LojaVirtual.Infraestrutura.Data
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly LojaDbContext _contexto;
		private IDbContextTransaction? _transacao;

		public IProdutoRepository Produtos { get; }
		public IProdutoTamanhoRepository ProdutoTamanhos { get; }
		public ICategoriaRepository Categorias { get; }
		public IClienteRepository Clientes { get; }
		public IPedidoRepository Pedidos { get; }
		public IMovimentacaoEstoqueRepository MovimentacoesEstoque { get; }
		public IEscolaRepository Escolas { get; }
		public IParametroSistemaRepository ParametrosSistema { get; }
		public IUsuarioRepository Usuarios { get; }

		public UnitOfWork(LojaDbContext contexto)
		{
			_contexto = contexto;
			Produtos = new ProdutoRepository(contexto);
			ProdutoTamanhos = new ProdutoTamanhoRepository(contexto);
			Categorias = new CategoriaRepository(contexto);
			Clientes = new ClienteRepository(contexto);
			Pedidos = new PedidoRepository(contexto);
			MovimentacoesEstoque = new MovimentacaoEstoqueRepository(contexto);
			Escolas = new EscolaRepository(contexto);
			ParametrosSistema = new ParametroSistemaRepository(contexto);
			Usuarios = new UsuarioRepository(contexto);
		}

		public async Task<int> SalvarMudancasAsync()
		{
			return await _contexto.SaveChangesAsync();
		}

		public async Task BeginTransactionAsync()
		{
			_transacao = await _contexto.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			if (_transacao != null)
			{
				await _transacao.CommitAsync();
				await _transacao.DisposeAsync();
				_transacao = null;
			}
		}

		public async Task RollbackTransactionAsync()
		{
			if (_transacao != null)
			{
				await _transacao.RollbackAsync();
				await _transacao.DisposeAsync();
				_transacao = null;
			}
		}

		public void Dispose()
		{
			_contexto.Dispose();
		}
	}
}