using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PagamentosController : ControllerBase
	{
		private readonly IServicoPagamento _servicoPagamento;
		private readonly ILogger<PagamentosController> _logger;

		public PagamentosController(
			 IServicoPagamento servicoPagamento,
			 ILogger<PagamentosController> logger)
		{
			_servicoPagamento = servicoPagamento;
			_logger = logger;
		}

		[HttpPost("criar")]
		public async Task<ActionResult<PagamentoResponseDTO>> CriarPagamento([FromBody] CriarPagamentoRequestDTO request)
		{
			try
			{
				if (request == null)
				{
					_logger.LogWarning("Pagamento request nulo recebido");
					return BadRequest(new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Request inválido"
					});
				}

				var cardNumber = request.DadosCartao?.CardNumber ?? string.Empty;
				var last4 = cardNumber.Length >= 4 ? cardNumber[^4..] : string.Empty;
				var hasToken = !string.IsNullOrWhiteSpace(request.DadosCartao?.CardToken);

				_logger.LogInformation(
					"Pagamento request recebido: PedidoId={PedidoId}, Metodo={Metodo}, HasToken={HasToken}, CardLast4={CardLast4}, CardholderName={CardholderName}",
					request.PedidoId,
					request.MetodoPagamento,
					hasToken,
					last4,
					request.DadosCartao?.CardholderName);

				var resultado = await _servicoPagamento.CriarPagamentoAsync(request);

				if (resultado.Sucesso)
				{
					return Ok(resultado);
				}

				return BadRequest(resultado);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao criar pagamento");
				return StatusCode(500, new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = "Erro interno ao processar pagamento"
				});
			}
		}

		[HttpPost("webhook")]
		public async Task<IActionResult> ProcessarWebhook([FromBody] WebhookNotificacaoDTO notificacao)
		{
			try
			{
				_logger.LogInformation("Webhook recebido: Type={Type}, Action={Action}, DataId={DataId}",
					 notificacao.Type, notificacao.Action, notificacao.Data.Id);

				// Mercado Pago envia notification do tipo "payment"
				if (notificacao.Type == "payment")
				{
					var sucesso = await _servicoPagamento.ProcessarWebhookAsync(notificacao.Data.Id);

					if (sucesso)
					{
						return Ok();
					}

					return BadRequest("Falha ao processar webhook");
				}

				// Outros tipos de notificação são ignorados
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao processar webhook");
				// Retornar 200 mesmo com erro para evitar reenvios do Mercado Pago
				return Ok();
			}
		}

		[HttpGet("webhook/test")]
		public IActionResult TestarWebhook()
		{
			return Ok(new { mensagem = "Webhook endpoint está funcionando", timestamp = DateTime.Now });
		}

		/// <summary>
		/// Endpoint para SIMULAR webhook do Mercado Pago em ambiente de desenvolvimento
		/// Uso: POST /api/pagamentos/webhook/simular/{pagamentoId}?status=approved
		/// </summary>
		[HttpPost("webhook/simular/{pagamentoId}")]
		public async Task<IActionResult> SimularWebhook(string pagamentoId, [FromQuery] string status = "approved")
		{
			try
			{
				_logger.LogInformation("🧪 SIMULAÇÃO: Webhook para pagamento {PagamentoId} com status {Status}", pagamentoId, status);

				// Simula o processamento do webhook com status fornecido (sem consultar MP)
				var sucesso = await _servicoPagamento.ProcessarWebhookSimuladoAsync(pagamentoId, status);

				if (sucesso)
				{
					return Ok(new
					{
						mensagem = $"✅ Webhook simulado com sucesso! Pagamento {pagamentoId} processado.",
						pagamentoId,
						statusSimulado = status,
						timestamp = DateTime.Now
					});
				}

				return BadRequest(new
				{
					mensagem = "❌ Falha ao simular webhook. Verifique se o pagamento existe.",
					pagamentoId
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao simular webhook");
				return StatusCode(500, new
				{
					mensagem = "Erro ao simular webhook",
					erro = ex.Message
				});
			}
		}

		/// <summary>
		/// Endpoint para RETENTAR pagamento de um pedido que falhou ou está aguardando
		/// Uso: POST /api/pagamentos/retentar/{pedidoId}
		/// </summary>
		[HttpPost("retentar/{pedidoId:guid}")]
		public async Task<ActionResult<PagamentoResponseDTO>> RetentarPagamento(Guid pedidoId, [FromBody] CriarPagamentoRequestDTO request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Request inválido",
						CodigoErro = "REQUEST_INVALIDO"
					});
				}

				_logger.LogInformation(
					"🔄 Reentando pagamento: PedidoId={PedidoId}, Metodo={Metodo}, Parcelas={Parcelas}",
					pedidoId,
					request.MetodoPagamento,
					request.Parcelas);

				var resultado = await _servicoPagamento.RetentarPagamentoAsync(pedidoId, request);

				if (resultado.Sucesso)
				{
					return Ok(resultado);
				}

				return BadRequest(resultado);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao retentar pagamento");
				return StatusCode(500, new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = "Erro interno ao processar pagamento",
					CodigoErro = "ERRO_INTERNO"
				});
			}
		}

		/// <summary>
		/// Endpoint para CANCELAR/ESTORNAR pagamento de um pedido
		/// Uso: POST /api/pagamentos/cancelar/{pedidoId}
		/// </summary>
		[HttpPost("cancelar/{pedidoId:guid}")]
		public async Task<ActionResult<PagamentoResponseDTO>> CancelarPagamento(Guid pedidoId)
		{
			try
			{
				_logger.LogInformation("🧾 Cancelamento/Estorno solicitado para PedidoId={PedidoId}", pedidoId);

				var resultado = await _servicoPagamento.CancelarPagamentoAsync(pedidoId);

				if (resultado.Sucesso)
				{
					return Ok(resultado);
				}

				return BadRequest(resultado);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao cancelar/estornar pagamento");
				return StatusCode(500, new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = "Erro interno ao cancelar/estornar pagamento",
					CodigoErro = "ERRO_INTERNO"
				});
			}
		}
	}
}
