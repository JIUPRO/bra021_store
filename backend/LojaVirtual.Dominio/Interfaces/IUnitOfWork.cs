namespace LojaVirtual.Dominio.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		IProdutoRepository Produtos { get; }
		IProdutoTamanhoRepository ProdutoTamanhos { get; }
		ICategoriaRepository Categorias { get; }
		IClienteRepository Clientes { get; }
		IPedidoRepository Pedidos { get; }
		IMovimentacaoEstoqueRepository MovimentacoesEstoque { get; }
		IEscolaRepository Escolas { get; }
		IParametroSistemaRepository ParametrosSistema { get; }
		IUsuarioRepository Usuarios { get; }

		Task<int> SalvarMudancasAsync();
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
