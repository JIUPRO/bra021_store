using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
	public interface IMovimentacaoEstoqueRepository : IBaseRepository<MovimentacaoEstoque>
	{
		Task<IEnumerable<MovimentacaoEstoque>> ObterPorProdutoAsync(Guid produtoId);
		Task<IEnumerable<MovimentacaoEstoque>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
		Task<IEnumerable<MovimentacaoEstoque>> ObterPorReferenciaAsync(string referencia);
	}
}
