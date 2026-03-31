using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using LojaVirtual.Infraestrutura.Services;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PedidosController : ControllerBase
	{
		private readonly IPedidoService _pedidoService;
		private readonly IStorageService _storageService;

		public PedidosController(IPedidoService servicoPedido, IStorageService storageService)
		{
			_pedidoService = servicoPedido;
			_storageService = storageService;
		}

		[Authorize]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterTodos()
		{
			var pedidos = await _pedidoService.ObterTodosAsync();
			return Ok(pedidos);
		}

		[Authorize]
		[HttpGet("{id}")]
		public async Task<ActionResult<PedidoDTO>> ObterPorId(Guid id)
		{
			var pedido = await _pedidoService.ObterPorIdAsync(id);
			if (pedido == null)
				return NotFound(new { mensagem = "Pedido não encontrado" });

			return Ok(pedido);
		}

		[Authorize]
		[HttpGet("cliente/{clienteId}")]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterPorCliente(Guid clienteId)
		{
			var pedidos = await _pedidoService.ObterPorClienteAsync(clienteId);
			return Ok(pedidos);
		}

		[Authorize]
		[HttpGet("status/{status}")]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterPorStatus(StatusPedido status)
		{
			var pedidos = await _pedidoService.ObterPorStatusAsync(status);
			return Ok(pedidos);
		}

		[Authorize]
		[HttpGet("periodo")]
		public async Task<ActionResult<IEnumerable<ResumoPedidoDTO>>> ObterPorPeriodo(
			 [FromQuery] DateTime dataInicio,
			 [FromQuery] DateTime dataFim)
		{
			var pedidos = await _pedidoService.ObterPorPeriodoAsync(dataInicio, dataFim);
			return Ok(pedidos);
		}

		[AllowAnonymous]
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

		[Authorize]
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

		[Authorize]
		[HttpPut("{id}/nota-fiscal")]
		public async Task<ActionResult<PedidoDTO>> AtualizarNotaFiscal(Guid id, [FromBody] AtualizarNotaFiscalDTO dto)
		{
			try
			{
				if (id != dto.Id)
					return BadRequest(new { mensagem = "ID do pedido não corresponde" });

				if (string.IsNullOrWhiteSpace(dto.NotaFiscalUrl))
					return BadRequest(new { mensagem = "URL da nota fiscal é obrigatória" });

				var pedidoAtual = await _pedidoService.ObterPorIdAsync(id);
				if (pedidoAtual == null)
				{
					return NotFound(new { mensagem = "Pedido não encontrado" });
				}

				var pedido = await _pedidoService.AtualizarNotaFiscalAsync(dto);
				if (pedido == null)
					return NotFound(new { mensagem = "Pedido não encontrado" });

				if (string.IsNullOrWhiteSpace(dto.NotaFiscalKey) &&
					!string.IsNullOrWhiteSpace(pedidoAtual.NotaFiscalKey) &&
					!string.Equals(pedidoAtual.NotaFiscalUrl, dto.NotaFiscalUrl, StringComparison.OrdinalIgnoreCase))
				{
					await _storageService.DeleteFileAsync(pedidoAtual.NotaFiscalKey, HttpContext.RequestAborted);
				}

				return Ok(pedido);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("{id}/nota-fiscal/upload")]
		public async Task<ActionResult<PedidoDTO>> UploadNotaFiscal(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest(new { mensagem = "Selecione um PDF para enviar." });
			}

			var allowedTypes = new[] { "application/pdf" };
			if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
			{
				return BadRequest(new { mensagem = "Envie apenas arquivos PDF." });
			}

			var pedidoAtual = await _pedidoService.ObterPorIdAsync(id);
			if (pedidoAtual == null)
			{
				return NotFound(new { mensagem = "Pedido não encontrado" });
			}

			try
			{
				await using var stream = file.OpenReadStream();
				var upload = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, cancellationToken);
				var pedido = await _pedidoService.AtualizarNotaFiscalAsync(new AtualizarNotaFiscalDTO
				{
					Id = id,
					NotaFiscalUrl = upload.Url,
					NotaFiscalKey = upload.Key
				});

				if (pedido == null)
				{
					await _storageService.DeleteFileAsync(upload.Key, cancellationToken);
					return NotFound(new { mensagem = "Pedido não encontrado" });
				}

				if (!string.IsNullOrWhiteSpace(pedidoAtual.NotaFiscalKey) && pedidoAtual.NotaFiscalKey != upload.Key)
				{
					await _storageService.DeleteFileAsync(pedidoAtual.NotaFiscalKey, cancellationToken);
				}

				return Ok(pedido);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpDelete("{id}/nota-fiscal")]
		public async Task<ActionResult<PedidoDTO>> RemoverNotaFiscal(Guid id, CancellationToken cancellationToken)
		{
			var pedidoAtual = await _pedidoService.ObterPorIdAsync(id);
			if (pedidoAtual == null)
			{
				return NotFound(new { mensagem = "Pedido não encontrado" });
			}

			try
			{
				if (!string.IsNullOrWhiteSpace(pedidoAtual.NotaFiscalKey))
				{
					await _storageService.DeleteFileAsync(pedidoAtual.NotaFiscalKey, cancellationToken);
				}

				var pedido = await _pedidoService.AtualizarNotaFiscalAsync(new AtualizarNotaFiscalDTO
				{
					Id = id,
					NotaFiscalUrl = string.Empty,
					NotaFiscalKey = null
				});

				if (pedido == null)
				{
					return NotFound(new { mensagem = "Pedido não encontrado" });
				}

				return Ok(pedido);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}
	}
}
