using Microsoft.AspNetCore.Mvc;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PedidosController : ControllerBase
	{
		private readonly IPedidoService _pedidoService;

		public PedidosController(IPedidoService servicoPedido)
		{
			_pedidoService = servicoPedido;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterTodos()
		{
			var pedidos = await _pedidoService.ObterTodosAsync();
			return Ok(pedidos);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<PedidoDTO>> ObterPorId(Guid id)
		{
			var pedido = await _pedidoService.ObterPorIdAsync(id);
			if (pedido == null)
				return NotFound(new { mensagem = "Pedido não encontrado" });

			return Ok(pedido);
		}

		[HttpGet("cliente/{clienteId}")]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterPorCliente(Guid clienteId)
		{
			var pedidos = await _pedidoService.ObterPorClienteAsync(clienteId);
			return Ok(pedidos);
		}

		[HttpGet("status/{status}")]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterPorStatus(StatusPedido status)
		{
			var pedidos = await _pedidoService.ObterPorStatusAsync(status);
			return Ok(pedidos);
		}

		[HttpGet("periodo")]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterPorPeriodo(
			 [FromQuery] DateTime dataInicio,
			 [FromQuery] DateTime dataFim)
		{
			var pedidos = await _pedidoService.ObterPorPeriodoAsync(dataInicio, dataFim);
			return Ok(pedidos);
		}

		[HttpPost]
		public async Task<ActionResult<PedidoDTO>> Criar([FromBody] CriarPedidoDTO dto)
		{
			try
			{
				var pedido = await _pedidoService.CriarAsync(dto);
				return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpPut("{id}/status")]
		public async Task<ActionResult<PedidoDTO>> AtualizarStatus(Guid id, [FromBody] StatusPedido status)
		{
			try
			{
				var dto = new AtualizarStatusPedidoDTO { Id = id, Status = status };
				var pedido = await _pedidoService.AtualizarStatusAsync(dto);
				if (pedido == null)
					return NotFound(new { mensagem = "Pedido não encontrado" });

				return Ok(pedido);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[HttpPut("{id}/nota-fiscal")]
		public async Task<ActionResult<PedidoDTO>> AtualizarNotaFiscal(Guid id, [FromBody] AtualizarNotaFiscalDTO dto)
		{
			try
			{
				if (id != dto.Id)
					return BadRequest(new { mensagem = "ID do pedido não corresponde" });

				if (string.IsNullOrWhiteSpace(dto.NotaFiscalUrl))
					return BadRequest(new { mensagem = "URL da nota fiscal é obrigatória" });

				var pedido = await _pedidoService.AtualizarNotaFiscalAsync(dto);
				if (pedido == null)
					return NotFound(new { mensagem = "Pedido não encontrado" });

				return Ok(pedido);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}
	}
}
