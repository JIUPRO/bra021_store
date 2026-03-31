using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Mapster;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IEstoqueService
	{
		Task<IEnumerable<MovimentacaoEstoqueDTO>> ObterTodasMovimentacoesAsync();
		Task<IEnumerable<MovimentacaoEstoqueDTO>> ObterMovimentacoesPorProdutoAsync(Guid produtoId);
		Task<IEnumerable<MovimentacaoEstoqueDTO>> ObterMovimentacoesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
		Task<ResumoMovimentacaoEstoqueDTO> ConsultarMovimentacoesAsync(ConsultaMovimentacaoEstoqueDTO filtro);
		Task<MovimentacaoEstoqueDTO> RegistrarMovimentacaoAsync(CriarMovimentacaoEstoqueDTO dto);
		Task<IEnumerable<AlertaEstoqueDTO>> ObterAlertasEstoqueBaixoAsync();
		Task<ProdutoDTO?> AjustarEstoqueAsync(AjusteEstoqueDTO dto);
	}

	public class EstoqueService : IEstoqueService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly INotificacaoService _NotificacaoService;

		public EstoqueService(IUnitOfWork unitOfWork, INotificacaoService notificacaoService)
		{
			_unitOfWork = unitOfWork;
			_NotificacaoService = notificacaoService;
		}

		public async Task<IEnumerable<MovimentacaoEstoqueDTO>> ObterTodasMovimentacoesAsync()
		{
			var movimentacoes = await _unitOfWork.MovimentacoesEstoque.ObterTodosAsync();
			return movimentacoes.Adapt<IEnumerable<MovimentacaoEstoqueDTO>>();
		}

		public async Task<IEnumerable<MovimentacaoEstoqueDTO>> ObterMovimentacoesPorProdutoAsync(Guid produtoId)
		{
			var movimentacoes = await _unitOfWork.MovimentacoesEstoque.ObterPorProdutoAsync(produtoId);
			return movimentacoes.Adapt<IEnumerable<MovimentacaoEstoqueDTO>>();
		}

		public async Task<IEnumerable<MovimentacaoEstoqueDTO>> ObterMovimentacoesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
		{
			var movimentacoes = await _unitOfWork.MovimentacoesEstoque.ObterPorPeriodoAsync(dataInicio, dataFim);
			return movimentacoes.Adapt<IEnumerable<MovimentacaoEstoqueDTO>>();
		}

		public async Task<ResumoMovimentacaoEstoqueDTO> ConsultarMovimentacoesAsync(ConsultaMovimentacaoEstoqueDTO filtro)
		{
			var todasMovimentacoes = (await _unitOfWork.MovimentacoesEstoque.ObterTodosAsync()).ToList();

			var movimentacoesBase = todasMovimentacoes.AsEnumerable();

			if (filtro.ProdutoTamanhoId.HasValue)
			{
				movimentacoesBase = movimentacoesBase.Where(m => m.ProdutoTamanhoId == filtro.ProdutoTamanhoId.Value);
			}
			else if (filtro.ProdutoId.HasValue)
			{
				movimentacoesBase = movimentacoesBase.Where(m => m.ProdutoTamanho.ProdutoId == filtro.ProdutoId.Value);
			}

			var movimentacoesBaseList = movimentacoesBase
				.OrderBy(m => m.DataMovimentacao)
				.ToList();

			var dataInicio = filtro.DataInicio;
			var dataFim = filtro.DataFim;

			if (dataFim.HasValue)
			{
				dataFim = dataFim.Value.Date.AddDays(1).AddTicks(-1);
			}

			var movimentacoesPeriodo = movimentacoesBaseList.AsEnumerable();

			if (dataInicio.HasValue)
			{
				movimentacoesPeriodo = movimentacoesPeriodo.Where(m => m.DataMovimentacao >= dataInicio.Value);
			}

			if (dataFim.HasValue)
			{
				movimentacoesPeriodo = movimentacoesPeriodo.Where(m => m.DataMovimentacao <= dataFim.Value);
			}

			var movimentacoesPeriodoList = movimentacoesPeriodo
				.OrderByDescending(m => m.DataMovimentacao)
				.ToList();

			var saldoAtual = movimentacoesBaseList
				.GroupBy(m => m.ProdutoTamanhoId)
				.Sum(g => g.OrderByDescending(m => m.DataMovimentacao).First().EstoqueAtual);

			var saldoInicialPeriodo = 0;

			if (dataInicio.HasValue)
			{
				saldoInicialPeriodo = movimentacoesBaseList
					.GroupBy(m => m.ProdutoTamanhoId)
					.Sum(g =>
					{
						var ordenadas = g.OrderBy(m => m.DataMovimentacao).ToList();
						var ultimaAntesDoInicio = ordenadas.LastOrDefault(m => m.DataMovimentacao < dataInicio.Value);
						if (ultimaAntesDoInicio != null)
						{
							return ultimaAntesDoInicio.EstoqueAtual;
						}

						var primeiraDepoisDoInicio = ordenadas.FirstOrDefault(m => m.DataMovimentacao >= dataInicio.Value);
						return primeiraDepoisDoInicio?.EstoqueAnterior ?? 0;
					});
			}

			return new ResumoMovimentacaoEstoqueDTO
			{
				ProdutoId = filtro.ProdutoId,
				ProdutoTamanhoId = filtro.ProdutoTamanhoId,
				DataInicio = filtro.DataInicio,
				DataFim = filtro.DataFim,
				SaldoInicialPeriodo = saldoInicialPeriodo,
				SaldoAtual = saldoAtual,
				TotalMovimentacoes = movimentacoesPeriodoList.Count,
				Movimentacoes = movimentacoesPeriodoList.Adapt<List<MovimentacaoEstoqueDTO>>()
			};
		}

		public async Task<MovimentacaoEstoqueDTO> RegistrarMovimentacaoAsync(CriarMovimentacaoEstoqueDTO dto)
		{
			var produtoTamanho = await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(dto.ProdutoTamanhoId);
			if (produtoTamanho == null)
			{
				throw new ArgumentException("Produto/Tamanho não encontrado.");
			}

			if (!produtoTamanho.Ativo)
			{
				throw new InvalidOperationException("Produto/Tamanho inativo.");
			}

			var estoqueAnterior = produtoTamanho.QuantidadeEstoque;
			var estoqueAtual = estoqueAnterior;

			switch (dto.Tipo)
			{
				case TipoMovimentacao.Entrada:
				case TipoMovimentacao.Devolucao:
					estoqueAtual += dto.Quantidade;
					break;
				case TipoMovimentacao.Saida:
					if (estoqueAnterior < dto.Quantidade)
					{
						throw new InvalidOperationException("Estoque insuficiente para saída.");
					}
					estoqueAtual -= dto.Quantidade;
					break;
				case TipoMovimentacao.Ajuste:
					estoqueAtual = dto.Quantidade;
					break;
			}

			var movimentacao = new MovimentacaoEstoque
			{
				ProdutoTamanhoId = dto.ProdutoTamanhoId,
				Quantidade = dto.Quantidade,
				Tipo = dto.Tipo,
				Motivo = dto.Motivo,
				Referencia = dto.Referencia,
				DataMovimentacao = DateTime.UtcNow,
				EstoqueAnterior = estoqueAnterior,
				EstoqueAtual = estoqueAtual
			};

			produtoTamanho.QuantidadeEstoque = estoqueAtual;

			await _unitOfWork.MovimentacoesEstoque.AdicionarAsync(movimentacao);
			await _unitOfWork.SalvarMudancasAsync();

			var movimentacaoComRelacionamentos = await _unitOfWork.MovimentacoesEstoque.ObterPorIdAsync(movimentacao.Id);
			return (movimentacaoComRelacionamentos ?? movimentacao).Adapt<MovimentacaoEstoqueDTO>();
		}

		public Task<IEnumerable<AlertaEstoqueDTO>> ObterAlertasEstoqueBaixoAsync()
		{
			return Task.FromResult<IEnumerable<AlertaEstoqueDTO>>(new List<AlertaEstoqueDTO>());
		}

		public Task<ProdutoDTO?> AjustarEstoqueAsync(AjusteEstoqueDTO dto)
		{
			throw new NotImplementedException("Use o endpoint específico de estoque");
		}
	}
}
