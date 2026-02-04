using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class ProdutoTamanhoRepository : BaseRepository<ProdutoTamanho>, IProdutoTamanhoRepository
	{
		private readonly LojaDbContext _dbContexto;

		public ProdutoTamanhoRepository(LojaDbContext contexto) : base(contexto)
		{
			_dbContexto = contexto;
		}

		public async Task<List<ProdutoTamanho>> ObterTodosPorProduto(Guid produtoId)
		{
			return await _dbSet
				 .Where(t => t.ProdutoId == produtoId)
				 .OrderBy(t => t.Tamanho)
				 .ToListAsync();
		}

		/// <summary>
		/// Calcula o saldo de estoque atual baseado nas movimentações
		/// Saldo = Entradas + Devoluções + Ajustes - Saídas
		/// </summary>
		public async Task<int> CalcularSaldoAtualAsync(Guid produtoTamanhoId)
		{
			var movimentacoes = await _dbContexto.MovimentacoesEstoque
				.Where(m => m.ProdutoTamanhoId == produtoTamanhoId)
				.ToListAsync();

			if (!movimentacoes.Any())
				return 0;

			var saldo = 0;
			foreach (var mov in movimentacoes.OrderBy(m => m.DataMovimentacao))
			{
				switch (mov.Tipo)
				{
					case TipoMovimentacao.Entrada:
					case TipoMovimentacao.Devolucao:
						saldo += mov.Quantidade;
						break;
					case TipoMovimentacao.Saida:
						saldo -= mov.Quantidade;
						break;
					case TipoMovimentacao.Ajuste:
						saldo = mov.Quantidade; // Ajuste define o saldo
						break;
				}
			}

			return Math.Max(0, saldo); // Nunca negativo
		}

		/// <summary>
		/// Atualiza o saldo de todos os tamanhos de um produto baseado nas movimentações
		/// </summary>
		public async Task AtualizarSaldosTamanhosAsync(Guid produtoId)
		{
			var tamanhos = await ObterTodosPorProduto(produtoId);

			foreach (var tamanho in tamanhos)
			{
				var novoSaldo = await CalcularSaldoAtualAsync(tamanho.Id);
				tamanho.QuantidadeEstoque = novoSaldo;
				tamanho.DataAtualizacao = DateTime.UtcNow;
				await AtualizarAsync(tamanho);
			}
		}
	}
}
