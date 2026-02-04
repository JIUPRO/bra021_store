using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
	public interface IProdutoTamanhoRepository : IBaseRepository<ProdutoTamanho>
	{
		Task<List<ProdutoTamanho>> ObterTodosPorProduto(Guid produtoId);
		Task<int> CalcularSaldoAtualAsync(Guid produtoTamanhoId);
		Task AtualizarSaldosTamanhosAsync(Guid produtoId);
	}
}
