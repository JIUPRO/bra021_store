using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Aplicacao.Services;
using LojaVirtual.Infraestrutura.Repositories;
using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class RelatoriosController : ControllerBase
	{
		private readonly PedidoRepository _repositorioPedido;
		private readonly ProdutoRepository _repositorioProduto;
		private readonly MovimentacaoEstoqueRepository _repositorioMovimentacaoEstoque;
		private readonly RelatorioService _RelatorioService;
		private readonly ILogger<RelatoriosController> _logger;

		public RelatoriosController(PedidoRepository repositorioPedido, ProdutoRepository repositorioProduto, MovimentacaoEstoqueRepository repositorioMovimentacaoEstoque, RelatorioService RelatorioService, ILogger<RelatoriosController> logger)
		{
			_repositorioPedido = repositorioPedido;
			_repositorioProduto = repositorioProduto;
			_repositorioMovimentacaoEstoque = repositorioMovimentacaoEstoque;
			_RelatorioService = RelatorioService;
			_logger = logger;
		}

		/// <summary>
		/// Gera relatório de pedidos em PDF
		/// </summary>
		/// <param name="dataInicio">Data inicial (formato: yyyy-MM-dd)</param>
		/// <param name="dataFim">Data final (formato: yyyy-MM-dd)</param>
		/// <param name="status">Status do pedido (opcional)</param>
		/// <returns>PDF com relatório de pedidos</returns>
		[HttpGet("pedidos")]
		public async Task<IActionResult> GerarRelatorioPedidos(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim,
			 [FromQuery] int? status = null)
		{
			try
			{
				// Validar e parsear datas
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio))
				{
					return BadRequest(new { mensagem = "Data inicial inválida. Use o formato yyyy-MM-dd" });
				}

				if (!DateTime.TryParse(dataFim, out DateTime dtFim))
				{
					return BadRequest(new { mensagem = "Data final inválida. Use o formato yyyy-MM-dd" });
				}

				// Ajustar data fim para incluir o dia inteiro (até 23:59:59)
				dtFim = dtFim.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

				// Buscar pedidos no período
				var pedidos = (await _repositorioPedido.ObterPorPeriodoAsync(dtInicio, dtFim)).ToList();

				// Filtrar por status se informado
				if (status.HasValue)
				{
					pedidos = pedidos.Where(p => (int)p.Status == status.Value).ToList();
				}

				// Ordenar por data decrescente
				pedidos = pedidos.OrderByDescending(p => p.DataPedido).ToList();

				// Gerar PDF
				var pdf = _RelatorioService.GerarRelatorioPedidos(pedidos, dtInicio, dtFim);

				// Retornar arquivo
				var nomeArquivo = $"relatorio_pedidos_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERRO AO GERAR RELATÓRIO DE PEDIDOS");
				return BadRequest(new { mensagem = "Erro ao gerar relatório", erro = ex.Message, inner = ex.InnerException?.Message, stackTrace = ex.StackTrace });
			}
		}

		/// <summary>
		/// Gera relatório de vendas por período
		/// </summary>
		/// <param name="dataInicio">Data inicial</param>
		/// <param name="dataFim">Data final</param>
		/// <returns>PDF com relatório de vendas</returns>
		[HttpGet("vendas")]
		public async Task<IActionResult> GerarRelatorioVendas(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim)
		{
			try
			{
				// Validar e parsear datas
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio))
				{
					return BadRequest(new { mensagem = "Data inicial inválida. Use o formato yyyy-MM-dd" });
				}

				if (!DateTime.TryParse(dataFim, out DateTime dtFim))
				{
					return BadRequest(new { mensagem = "Data final inválida. Use o formato yyyy-MM-dd" });
				}

				var pedidos = (await _repositorioPedido.ObterPorPeriodoAsync(dtInicio, dtFim)).ToList();

				// Filtrar apenas pedidos entregues ou confirmados para vendas
				pedidos = pedidos.Where(p =>
					 p.Status == StatusPedido.Pago ||
					 p.Status == StatusPedido.Enviado ||
					 p.Status == StatusPedido.Entregue)
					 .ToList();

				var pdf = _RelatorioService.GerarRelatorioPedidos(pedidos, dtInicio, dtFim);
				var nomeArquivo = $"relatorio_vendas_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = "Erro ao gerar relatório de vendas", erro = ex.Message });
			}
		}

		/// <summary>
		/// Gera relatório de comissões por escola
		/// </summary>
		/// <param name="dataInicio">Data inicial (formato: yyyy-MM-dd)</param>
		/// <param name="dataFim">Data final (formato: yyyy-MM-dd)</param>
		/// <returns>PDF com relatório de comissões</returns>
		[HttpGet("comissao")]
		public async Task<IActionResult> GerarRelatorioComissao(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim)
		{
			try
			{
				// Validar e parsear datas
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio))
				{
					return BadRequest(new { mensagem = "Data inicial inválida. Use o formato yyyy-MM-dd" });
				}

				if (!DateTime.TryParse(dataFim, out DateTime dtFim))
				{
					return BadRequest(new { mensagem = "Data final inválida. Use o formato yyyy-MM-dd" });
				}

				// Ajustar data fim para incluir o dia inteiro (até 23:59:59)
				dtFim = dtFim.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

				// Buscar pedidos no período - incluir todos os status exceto cancelados
				var pedidos = (await _repositorioPedido.ObterPorPeriodoAsync(dtInicio, dtFim))
					.Where(p => p.Status != StatusPedido.Cancelado)
					.ToList();

				// Gerar PDF de comissões
				var pdf = _RelatorioService.GerarRelatorioComissao(pedidos, dtInicio, dtFim);

				// Retornar arquivo
				var nomeArquivo = $"relatorio_comissao_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERRO AO GERAR RELATÓRIO DE COMISSÃO");
				return BadRequest(new { mensagem = "Erro ao gerar relatório de comissão", erro = ex.Message });
			}
		}

		/// <summary>
		/// Gera relatório de produtos mais vendidos
		/// </summary>
		/// <param name="dataInicio">Data inicial (formato: yyyy-MM-dd)</param>
		/// <param name="dataFim">Data final (formato: yyyy-MM-dd)</param>
		/// <returns>PDF com relatório de produtos mais vendidos</returns>
		[HttpGet("produtos-vendidos")]
		public async Task<IActionResult> GerarRelatorioProdutosMaisVendidos(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim)
		{
			try
			{
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio) || !DateTime.TryParse(dataFim, out DateTime dtFim))
					return BadRequest(new { mensagem = "Datas inválidas. Use o formato yyyy-MM-dd" });

				dtFim = dtFim.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

				// Incluir todos os pedidos exceto cancelados
				var pedidos = (await _repositorioPedido.ObterPorPeriodoAsync(dtInicio, dtFim))
					.Where(p => p.Status != StatusPedido.Cancelado)
					.ToList();

				var pdf = _RelatorioService.GerarRelatorioProdutosMaisVendidos(pedidos, dtInicio, dtFim);
				var nomeArquivo = $"relatorio_produtos_vendidos_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERRO AO GERAR RELATÓRIO DE PRODUTOS MAIS VENDIDOS");
				return BadRequest(new { mensagem = "Erro ao gerar relatório", erro = ex.Message });
			}
		}

		/// <summary>
		/// Gera relatório de clientes
		/// </summary>
		/// <param name="dataInicio">Data inicial (formato: yyyy-MM-dd)</param>
		/// <param name="dataFim">Data final (formato: yyyy-MM-dd)</param>
		/// <returns>PDF com relatório de clientes</returns>
		[HttpGet("clientes")]
		public async Task<IActionResult> GerarRelatorioClientes(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim)
		{
			try
			{
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio) || !DateTime.TryParse(dataFim, out DateTime dtFim))
					return BadRequest(new { mensagem = "Datas inválidas. Use o formato yyyy-MM-dd" });

				dtFim = dtFim.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

				var pedidos = (await _repositorioPedido.ObterPorPeriodoAsync(dtInicio, dtFim)).ToList();

				var pdf = _RelatorioService.GerarRelatorioClientes(pedidos, dtInicio, dtFim);
				var nomeArquivo = $"relatorio_clientes_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERRO AO GERAR RELATÓRIO DE CLIENTES");
				return BadRequest(new { mensagem = "Erro ao gerar relatório", erro = ex.Message });
			}
		}

		/// <summary>
		/// Gera relatório de movimentação de estoque
		/// </summary>
		/// <param name="dataInicio">Data inicial (formato: yyyy-MM-dd)</param>
		/// <param name="dataFim">Data final (formato: yyyy-MM-dd)</param>
		/// <returns>PDF com relatório de estoque</returns>
		[HttpGet("estoque")]
		public async Task<IActionResult> GerarRelatorioEstoque(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim)
		{
			try
			{
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio) || !DateTime.TryParse(dataFim, out DateTime dtFim))
					return BadRequest(new { mensagem = "Datas inválidas. Use o formato yyyy-MM-dd" });

				dtFim = dtFim.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

				// Buscar todas as movimentações de estoque no período
				var todasAsMovimentacoes = (await _repositorioMovimentacaoEstoque.ObterTodosAsync())
					.Where(m => m.DataMovimentacao >= dtInicio && m.DataMovimentacao <= dtFim)
					.OrderBy(m => m.ProdutoTamanho?.Produto?.Nome)
					.ThenBy(m => m.DataMovimentacao)
					.ToList();

				// Buscar todos os produtos para referência
				var produtos = (await _repositorioProduto.ObterTodosAsync()).ToList();

				var pdf = _RelatorioService.GerarRelatorioEstoqueExtrato(todasAsMovimentacoes, produtos, dtInicio, dtFim);
				var nomeArquivo = $"relatorio_estoque_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERRO AO GERAR RELATÓRIO DE ESTOQUE");
				return BadRequest(new { mensagem = "Erro ao gerar relatório", erro = ex.Message });
			}
		}

		/// <summary>
		/// Gera relatório de produtos sem saída
		/// </summary>
		/// <param name="dataInicio">Data inicial (formato: yyyy-MM-dd)</param>
		/// <param name="dataFim">Data final (formato: yyyy-MM-dd)</param>
		/// <returns>PDF com relatório de produtos sem saída</returns>
		[HttpGet("produtos-sem-saida")]
		public async Task<IActionResult> GerarRelatorioProdutosSemSaida(
			 [FromQuery] string dataInicio,
			 [FromQuery] string dataFim)
		{
			try
			{
				if (!DateTime.TryParse(dataInicio, out DateTime dtInicio) || !DateTime.TryParse(dataFim, out DateTime dtFim))
					return BadRequest(new { mensagem = "Datas inválidas. Use o formato yyyy-MM-dd" });

				dtFim = dtFim.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

				var todosProdutos = (await _repositorioProduto.ObterTodosAsync()).ToList();
				var pedidos = (await _repositorioPedido.ObterPorPeriodoAsync(dtInicio, dtFim)).ToList();

				var pdf = _RelatorioService.GerarRelatorioProdutosSemSaida(todosProdutos, pedidos, dtInicio, dtFim);
				var nomeArquivo = $"relatorio_produtos_sem_saida_{dtInicio:yyyyMMdd}_{dtFim:yyyyMMdd}.pdf";
				return File(pdf, "application/pdf", nomeArquivo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERRO AO GERAR RELATÓRIO DE PRODUTOS SEM SAÍDA");
				return BadRequest(new { mensagem = "Erro ao gerar relatório", erro = ex.Message });
			}
		}
	}
}
