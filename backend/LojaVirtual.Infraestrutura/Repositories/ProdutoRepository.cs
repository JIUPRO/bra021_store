using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class ProdutoRepository : BaseRepository<Produto>, IProdutoRepository
	{
		public ProdutoRepository(LojaDbContext contexto) : base(contexto)
		{
		}

		public override async Task<Produto?> ObterPorIdAsync(Guid id)
		{
			return await _dbSet
				 .Include(p => p.Categoria)
				 .FirstOrDefaultAsync(p => p.Id == id);
		}

		public override async Task<IEnumerable<Produto>> ObterTodosAsync()
		{
			return await _dbSet
				 .Include(p => p.Categoria)
				 .Where(p => p.Ativo)
				 .OrderBy(p => p.Nome)
				 .ToListAsync();
		}

		public async Task<IEnumerable<Produto>> ObterProdutosEmDestaqueAsync()
		{
			return await _dbSet
				 .Include(p => p.Categoria)
				  .Where(p => p.Ativo && p.Destaque && p.Tamanhos.Sum(t => t.QuantidadeEstoque) > 0)
				 .OrderBy(p => p.Nome)
				 .ToListAsync();
		}

		public async Task<IEnumerable<Produto>> ObterPorCategoriaAsync(Guid categoriaId)
		{
			return await _dbSet
				 .Include(p => p.Categoria)
				  .Where(p => p.Ativo && p.CategoriaId == categoriaId && p.Tamanhos.Sum(t => t.QuantidadeEstoque) > 0)
				 .OrderBy(p => p.Nome)
				 .ToListAsync();
		}

		public async Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync()
		{
			return await _dbSet
				 .Include(p => p.Categoria)
				  .Where(p => p.Ativo && p.Tamanhos.Sum(t => t.QuantidadeEstoque) <= p.QuantidadeMinimaEstoque)
				  .OrderBy(p => p.Tamanhos.Sum(t => t.QuantidadeEstoque))
				 .ToListAsync();
		}

		public async Task<IEnumerable<Produto>> PesquisarAsync(string termo)
		{
			if (string.IsNullOrWhiteSpace(termo))
				return await ObterTodosAsync();

			termo = termo.ToLower();
			return await _dbSet
				 .Include(p => p.Categoria)
				 .Where(p => p.Ativo &&
					  (p.Nome.ToLower().Contains(termo) ||
						(p.Descricao != null && p.Descricao.ToLower().Contains(termo)) ||
					(p.DescricaoCurta != null && p.DescricaoCurta.ToLower().Contains(termo))))
				 .OrderBy(p => p.Nome)
				 .ToListAsync();
		}
	}
}
