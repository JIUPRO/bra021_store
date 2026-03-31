using System.Text;
using LojaVirtual.Aplicacao.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LogisticaController : ControllerBase
	{
		private readonly ILogisticaService _logisticaService;

		public LogisticaController(ILogisticaService logisticaService)
		{
			_logisticaService = logisticaService;
		}

		[Authorize]
		[HttpPost("pedidos/{id}/gerar-etiqueta")]
		public async Task<IActionResult> GerarEtiqueta(Guid id, CancellationToken cancellationToken)
		{
			try
			{
				var resultado = await _logisticaService.GerarEtiquetaAsync(id, cancellationToken);
				return Ok(resultado);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpPost("pedidos/{id}/sincronizar-rastreio")]
		public async Task<IActionResult> SincronizarRastreio(Guid id, CancellationToken cancellationToken)
		{
			try
			{
				var resultado = await _logisticaService.SincronizarRastreioAsync(id, cancellationToken);
				return Ok(resultado);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[AllowAnonymous]
		[HttpPost("melhor-envio/webhook")]
		public async Task<IActionResult> WebhookMelhorEnvio(CancellationToken cancellationToken)
		{
			try
			{
				Request.EnableBuffering();
				using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
				var body = await reader.ReadToEndAsync(cancellationToken);
				Request.Body.Position = 0;

				var signature = Request.Headers["X-ME-Signature"].FirstOrDefault();
				await _logisticaService.ProcessarWebhookAsync(body, signature, cancellationToken);
				return Ok(new { sucesso = true });
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}
	}
}
