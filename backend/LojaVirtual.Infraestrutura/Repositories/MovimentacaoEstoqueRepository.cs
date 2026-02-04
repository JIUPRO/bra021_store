using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class MovimentacaoEstoqueRepository : BaseRepository<MovimentacaoEstoque>, IMovimentacaoEstoqueRepository
	{
		public MovimentacaoEstoqueRepository(LojaDbContext contexto) : base(contexto)
		{
		}

		public override async Task<IEnumerable<MovimentacaoEstoque>> ObterTodosAsync()
		{
			return await _dbSet
				 .Include(m => m.ProdutoTamanho)
					  .ThenInclude(pt => pt.Produto)
				 .OrderByDescending(m => m.DataMovimentacao)
				 .ToListAsync();
		}

		public async Task<IEnumerable<MovimentacaoEstoque>> ObterPorProdutoAsync(Guid produtoId)
		{
			return await _dbSet
				 .Include(m => m.ProdutoTamanho)
					  .ThenInclude(pt => pt.Produto)
				 .Where(m => m.ProdutoTamanho.ProdutoId == produtoId)
				 .OrderByDescending(m => m.DataMovimentacao)
				 .ToListAsync();
		}

		public async Task<IEnumerable<MovimentacaoEstoque>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
		{
			return await _dbSet
				 .Include(m => m.ProdutoTamanho)
					  .ThenInclude(pt => pt.Produto)
				 .Where(m => m.DataMovimentacao >= dataInicio && m.DataMovimentacao <= dataFim)
				 .OrderByDescending(m => m.DataMovimentacao)
				 .ToListAsync();
		}

		public async Task<IEnumerable<MovimentacaoEstoque>> ObterPorReferenciaAsync(string referencia)
		{
			return await _dbSet
				 .Include(m => m.ProdutoTamanho)
					  .ThenInclude(pt => pt.Produto)
				 .Where(m => m.Referencia == referencia)
				 .OrderByDescending(m => m.DataMovimentacao)
				 .ToListAsync();
		}
	}
}
