using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Infraestrutura.Repositories;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class EstoqueController : ControllerBase
	{
		private readonly IEstoqueService _estoqueService;
		private readonly ProdutoTamanhoRepository _repositorioProdutoTamanho;
		private readonly MovimentacaoEstoqueRepository _repositorioMovimentacaoEstoque;
		private readonly ProdutoRepository _repositorioProduto;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<EstoqueController> _logger;

		public EstoqueController(
			IEstoqueService servicoEstoque,
			ProdutoTamanhoRepository repositorioProdutoTamanho,
			MovimentacaoEstoqueRepository repositorioMovimentacaoEstoque,
			ProdutoRepository repositorioProduto,
			IUnitOfWork unitOfWork,
			ILogger<EstoqueController> logger)
		{
			_estoqueService = servicoEstoque;
			_repositorioProdutoTamanho = repositorioProdutoTamanho;
			_repositorioMovimentacaoEstoque = repositorioMovimentacaoEstoque;
			_repositorioProduto = repositorioProduto;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		/// <summary>
		/// Lista todas as variações de um produto com saldo atual
		/// </summary>
		[HttpGet("tamanhos/{produtoId}")]
		public async Task<IActionResult> ListarTamanhosPorProduto(Guid produtoId)
		{
			try
			{
				var produto = await _repositorioProduto.ObterPorIdAsync(produtoId);
				if (produto == null)
					return NotFound(new { mensagem = "Produto não encontrado" });

				var tamanhos = await _repositorioProdutoTamanho.ObterTodosPorProduto(produtoId);

				var resultado = new List<object>();
				foreach (var tamanho in tamanhos)
				{
					var saldoAtual = await _repositorioProdutoTamanho.CalcularSaldoAtualAsync(tamanho.Id);
					resultado.Add(new
					{
						tamanho.Id,
						tamanho.Tamanho,
						SaldoAtual = saldoAtual,
						tamanho.Ativo
					});
				}

				return Ok(resultado);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao listar tamanhos");
				return BadRequest(new { mensagem = "Erro ao listar tamanhos", erro = ex.Message });
			}
		}

		/// <summary>
		/// Adiciona entrada de estoque (compra, recebimento, etc)
		/// </summary>
		[HttpPost("adicionar-entrada")]
		public async Task<IActionResult> AdicionarEntrada([FromBody] AdicionarEntradaEstoqueDTO dto)
		{
			try
			{
				var tamanho = await _repositorioProdutoTamanho.ObterPorIdAsync(dto.ProdutoTamanhoId);
				if (tamanho == null)
					return NotFound(new { mensagem = "Tamanho não encontrado" });

				var estoqueAnterior = await _repositorioProdutoTamanho.CalcularSaldoAtualAsync(tamanho.Id);
				var estoqueAtual = estoqueAnterior + dto.Quantidade;

				// Registrar movimentação
				var movimentacao = new MovimentacaoEstoque
				{
					ProdutoTamanhoId = tamanho.Id,
					Quantidade = dto.Quantidade,
					Tipo = TipoMovimentacao.Entrada,
					Motivo = dto.Motivo ?? "Entrada de estoque",
					DataMovimentacao = DateTime.UtcNow,
					EstoqueAnterior = estoqueAnterior,
					EstoqueAtual = estoqueAtual,
					Referencia = dto.Referencia,
					Ativo = true,
					DataCriacao = DateTime.UtcNow
				};

				await _repositorioMovimentacaoEstoque.AdicionarAsync(movimentacao);

				// Atualizar saldo do tamanho
				tamanho.QuantidadeEstoque = estoqueAtual;
				tamanho.DataAtualizacao = DateTime.UtcNow;
				await _repositorioProdutoTamanho.AtualizarAsync(tamanho);

				await _unitOfWork.SalvarMudancasAsync();

				return Ok(new
				{
					mensagem = "Entrada registrada com sucesso",
					estoqueAnterior,
					quantidade = dto.Quantidade,
					estoqueAtual,
					tamanho = tamanho.Tamanho
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao adicionar entrada");
				return BadRequest(new { mensagem = "Erro ao adicionar entrada", erro = ex.Message });
			}
		}

		/// <summary>
		/// Lista movimentações de estoque de um tamanho
		/// </summary>
		[HttpGet("movimentações/{produtoTamanhoId}")]
		public async Task<IActionResult> ListarMovimentacoes(Guid produtoTamanhoId, [FromQuery] int? dias = null)
		{
			try
			{
				var tamanho = await _repositorioProdutoTamanho.ObterPorIdAsync(produtoTamanhoId);
				if (tamanho == null)
					return NotFound(new { mensagem = "Tamanho não encontrado" });

				var todasMovimentacoes = await _repositorioMovimentacaoEstoque.ObterPorPeriodoAsync(
					dias.HasValue ? DateTime.UtcNow.AddDays(-dias.Value) : DateTime.MinValue,
					DateTime.UtcNow);

				var movimentacoes = todasMovimentacoes
					.Where(m => m.ProdutoTamanhoId == produtoTamanhoId)
					.OrderByDescending(m => m.DataMovimentacao)
					.Select(m => new
					{
						m.Id,
						m.DataMovimentacao,
						Tipo = m.Tipo.ToString(),
						m.Quantidade,
						m.EstoqueAnterior,
						m.EstoqueAtual,
						m.Motivo,
						m.Referencia
					})
					.ToList();

				return Ok(movimentacoes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao listar movimentações");
				return BadRequest(new { mensagem = "Erro ao listar movimentações", erro = ex.Message });
			}
		}

		/// <summary>
		/// Ajusta estoque (perdas, devoluções, etc)
		/// </summary>
		[HttpPost("ajustar-estoque")]
		public async Task<IActionResult> AjustarEstoque([FromBody] AjustarEstoqueTamanhoDTO dto)
		{
			try
			{
				var tamanho = await _repositorioProdutoTamanho.ObterPorIdAsync(dto.ProdutoTamanhoId);
				if (tamanho == null)
					return NotFound(new { mensagem = "Tamanho não encontrado" });

				var estoqueAnterior = await _repositorioProdutoTamanho.CalcularSaldoAtualAsync(tamanho.Id);
				var novoSaldo = dto.NovaQuantidade;

				// Registrar movimentação de ajuste
				var movimentacao = new MovimentacaoEstoque
				{
					ProdutoTamanhoId = tamanho.Id,
					Quantidade = Math.Abs(novoSaldo - estoqueAnterior),
					Tipo = TipoMovimentacao.Ajuste,
					Motivo = dto.Motivo ?? "Ajuste de estoque",
					DataMovimentacao = DateTime.UtcNow,
					EstoqueAnterior = estoqueAnterior,
					EstoqueAtual = novoSaldo,
					Ativo = true,
					DataCriacao = DateTime.UtcNow
				};

				await _repositorioMovimentacaoEstoque.AdicionarAsync(movimentacao);

				// Atualizar saldo do tamanho
				tamanho.QuantidadeEstoque = novoSaldo;
				tamanho.DataAtualizacao = DateTime.UtcNow;
				await _repositorioProdutoTamanho.AtualizarAsync(tamanho);

				await _unitOfWork.SalvarMudancasAsync();

				return Ok(new
				{
					mensagem = "Ajuste registrado com sucesso",
					estoqueAnterior,
					novoSaldo,
					diferencaAjustada = novoSaldo - estoqueAnterior,
					tamanho = tamanho.Tamanho
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao ajustar estoque");
				return BadRequest(new { mensagem = "Erro ao ajustar estoque", erro = ex.Message });
			}
		}

		/// <summary>
		/// Recalcula todos os saldos de um produto baseado nas movimentações
		/// </summary>
		[HttpPost("recalcular-saldos/{produtoId}")]
		public async Task<IActionResult> RecalcularSaldos(Guid produtoId)
		{
			try
			{
				var produto = await _repositorioProduto.ObterPorIdAsync(produtoId);
				if (produto == null)
					return NotFound(new { mensagem = "Produto não encontrado" });

				await _repositorioProdutoTamanho.AtualizarSaldosTamanhosAsync(produtoId);
				await _unitOfWork.SalvarMudancasAsync();

				var tamanhos = await _repositorioProdutoTamanho.ObterTodosPorProduto(produtoId);
				var resultado = tamanhos.Select(t => new
				{
					t.Tamanho,
					t.QuantidadeEstoque
				});

				return Ok(new
				{
					mensagem = "Saldos recalculados com sucesso",
					tamanhos = resultado
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao recalcular saldos");
				return BadRequest(new { mensagem = "Erro ao recalcular saldos", erro = ex.Message });
			}
		}

		// ========== Métodos legados (manter compatibilidade) ==========

		[HttpGet("movimentacoes")]
		public async Task<ActionResult<IEnumerable<MovimentacaoEstoqueDTO>>> ObterTodasMovimentacoes()
		{
			var movimentacoes = await _estoqueService.ObterTodasMovimentacoesAsync();
			return Ok(movimentacoes);
		}

		[HttpPost("movimentacoes")]
		public async Task<ActionResult<MovimentacaoEstoqueDTO>> CriarMovimentacao([FromBody] CriarMovimentacaoEstoqueDTO dto)
		{
			try
			{
				var movimentacao = await _estoqueService.RegistrarMovimentacaoAsync(dto);
				return Ok(movimentacao);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao criar movimentação");
				return BadRequest(new { mensagem = "Erro ao criar movimentação", erro = ex.Message });
			}
		}

		[HttpGet("movimentacoes/produto/{produtoId}")]
		public async Task<ActionResult<IEnumerable<MovimentacaoEstoqueDTO>>> ObterMovimentacoesPorProduto(Guid produtoId)
		{
			var movimentacoes = await _estoqueService.ObterMovimentacoesPorProdutoAsync(produtoId);
			return Ok(movimentacoes);
		}

		[HttpGet("movimentacoes/periodo")]
		public async Task<ActionResult<IEnumerable<MovimentacaoEstoqueDTO>>> ObterMovimentacoesPorPeriodo(
			[FromQuery] DateTime dataInicio,
			[FromQuery] DateTime dataFim)
		{
			var movimentacoes = await _estoqueService.ObterMovimentacoesPorPeriodoAsync(dataInicio, dataFim);
			return Ok(movimentacoes);
		}

		[HttpGet("alertas")]
		public async Task<ActionResult<IEnumerable<AlertaEstoqueDTO>>> ObterAlertas()
		{
			var alertas = await _estoqueService.ObterAlertasEstoqueBaixoAsync();
			return Ok(alertas);
		}
	}
}
