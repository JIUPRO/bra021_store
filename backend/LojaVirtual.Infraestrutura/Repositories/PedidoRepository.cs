using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class PedidoRepository : BaseRepository<Pedido>, IPedidoRepository
	{
		public PedidoRepository(LojaDbContext contexto) : base(contexto)
		{
		}

		public override async Task<IEnumerable<Pedido>> ObterTodosAsync()
		{
			return await _dbSet
				 .Include(p => p.Cliente)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.Produto)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.ProdutoTamanho)
				 .OrderByDescending(p => p.DataPedido)
				 .ToListAsync();
		}

		public async Task<Pedido?> ObterComItensAsync(Guid id)
		{
			return await _dbSet
				 .Include(p => p.Cliente)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.Produto)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.ProdutoTamanho)
				 .FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IEnumerable<Pedido>> ObterPorClienteAsync(Guid clienteId)
		{
			return await _dbSet
				 .Include(p => p.Cliente)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.Produto)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.ProdutoTamanho)
				 .Where(p => p.ClienteId == clienteId)
				 .OrderByDescending(p => p.DataPedido)
				 .ToListAsync();
		}

		public async Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status)
		{
			return await _dbSet
				 .Include(p => p.Cliente)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.Produto)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.ProdutoTamanho)
				 .Where(p => p.Status == status)
				 .OrderByDescending(p => p.DataPedido)
				 .ToListAsync();
		}

		public async Task<IEnumerable<Pedido>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
		{
			return await _dbSet
				 .Include(p => p.Cliente)
				 .Include(p => p.Escola)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.Produto)
				 .Include(p => p.Itens)
					  .ThenInclude(i => i.ProdutoTamanho)
				 .Where(p => p.DataPedido >= dataInicio && p.DataPedido <= dataFim)
				 .OrderByDescending(p => p.DataPedido)
				 .ToListAsync();
		}

		public async Task<string> GerarNumeroPedidoAsync()
		{
			var ano = DateTime.Now.Year;
			var quantidade = await _dbSet.CountAsync(p => p.DataPedido.Year == ano);
			return $"PED-{ano}-{(quantidade + 1).ToString("D6")}";
		}
	}
}
