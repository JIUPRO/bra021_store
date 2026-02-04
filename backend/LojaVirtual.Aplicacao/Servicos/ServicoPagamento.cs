using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace LojaVirtual.Aplicacao.Servicos
{
	public interface IServicoPagamento
	{
		Task<PagamentoResponseDTO> CriarPagamentoAsync(CriarPagamentoRequestDTO request);
		Task<bool> ProcessarWebhookAsync(string pagamentoId);
		Task<bool> ProcessarWebhookSimuladoAsync(string pagamentoId, string statusSimulado);
		Task<PagamentoResponseDTO> RetentarPagamentoAsync(Guid pedidoId, CriarPagamentoRequestDTO request);
		Task<PagamentoResponseDTO> CancelarPagamentoAsync(Guid pedidoId);
		decimal CalcularValorParcela(decimal valorTotal, int numeroParcelas);
	}

	public class ServicoPagamento : IServicoPagamento
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;
		private readonly INotificacaoService _notificacaoService;
		private readonly ILogger<ServicoPagamento> _logger;
		private readonly HttpClient _httpClient;

		public ServicoPagamento(
			 IUnitOfWork unitOfWork,
			 IConfiguration configuration,
			 INotificacaoService notificacaoService,
			 ILogger<ServicoPagamento> logger,
			 IHttpClientFactory httpClientFactory)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
			_notificacaoService = notificacaoService;
			_logger = logger;
			_httpClient = httpClientFactory.CreateClient();
			_httpClient.BaseAddress = new Uri("https://api.mercadopago.com/v1/");
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["MercadoPago:AccessToken"]}");
			_httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
		}

		public decimal CalcularValorParcela(decimal valorTotal, int numeroParcelas)
		{
			if (numeroParcelas <= 0)
				numeroParcelas = 1;
			return Math.Round(valorTotal / numeroParcelas, 2);
		}

		private async Task<PagamentoResponseDTO> ValidarRequisicaoPagamentoAsync(CriarPagamentoRequestDTO request)
		{
			// Buscar máximo de parcelas configurado no sistema
			var parametroMaxParcelas = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("MaximoParcelas");
			int maxParcelas = 3; // valor padrão
			if (parametroMaxParcelas != null && int.TryParse(parametroMaxParcelas.Valor, out int maxParcelasConfig))
			{
				maxParcelas = maxParcelasConfig;
			}

			// Validar parcelas para cartão
			if (string.Equals(request.MetodoPagamento, "credit_card", StringComparison.OrdinalIgnoreCase))
			{
				if (request.Parcelas < 1 || request.Parcelas > maxParcelas)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = $"Número de parcelas inválido. Aceita-se apenas 1 a {maxParcelas} parcelas sem juros.",
						CodigoErro = "INVALID_INSTALLMENTS"
					};
				}
			}

			// PIX não usa parcelas
			if (request.MetodoPagamento == "pix" && request.Parcelas > 1)
			{
				return new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = "PIX não suporta parcelamento.",
					CodigoErro = "PIX_NO_INSTALLMENTS"
				};
			}

			return new PagamentoResponseDTO { Sucesso = true };
		}

		public async Task<PagamentoResponseDTO> CriarPagamentoAsync(CriarPagamentoRequestDTO request)
		{
			try
			{
				// Validar request
				var validacao = await ValidarRequisicaoPagamentoAsync(request);
				if (!validacao.Sucesso)
					return validacao;

				// Buscar pedido usando Guid
				var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(request.PedidoId);

				if (pedido == null)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Pedido não encontrado",
						CodigoErro = "PEDIDO_NAO_ENCONTRADO"
					};
				}

				// Calcular valor da parcela
				var valorParcela = CalcularValorParcela(pedido.ValorTotal, request.Parcelas);

				// Registrar método de pagamento no pedido
				pedido.MetodoPagamento = request.MetodoPagamento;

				// Criar payload para Mercado Pago
				var payloadDict = new Dictionary<string, object>
				{
					["transaction_amount"] = pedido.ValorTotal,
					["description"] = $"Pedido #{pedido.NumeroPedido} - Loja Brazil-021",
					["payer"] = new Dictionary<string, string>
					{
						["email"] = pedido.Cliente?.Email ?? "cliente@lojabrazil021.com"
					},
					["external_reference"] = pedido.Id.ToString()
				};

				// Definir método de pagamento apenas quando necessário
				// Para cartão, o Mercado Pago identifica o método pelo token
				if (!string.IsNullOrWhiteSpace(request.MetodoPagamento))
				{
					if (request.MetodoPagamento == "pix")
					{
						payloadDict["payment_method_id"] = "pix";
					}
					else if (!string.Equals(request.MetodoPagamento, "credit_card", StringComparison.OrdinalIgnoreCase))
					{
						payloadDict["payment_method_id"] = request.MetodoPagamento;
					}
				}

				// Para cartão, enviar parcelas
				if (string.Equals(request.MetodoPagamento, "credit_card", StringComparison.OrdinalIgnoreCase))
				{
					payloadDict["installments"] = request.Parcelas;
				}

				// Adicionar token do cartão se fornecido
				if (!string.IsNullOrEmpty(request.DadosCartao?.CardToken))
				{
					payloadDict["token"] = request.DadosCartao.CardToken;
				}

				var jsonPayload = JsonSerializer.Serialize(payloadDict);
				var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
				var idempotencyKey = Guid.NewGuid().ToString();
				var logPayload = string.IsNullOrEmpty(request.DadosCartao?.CardToken)
					? jsonPayload
					: jsonPayload.Replace(request.DadosCartao.CardToken, "***");

				_logger.LogInformation(
					"💳 Mercado Pago request: PedidoId={PedidoId}, Metodo={Metodo}, Parcelas={Parcelas}, IdempotencyKey={IdempotencyKey}, Payload={Payload}",
					request.PedidoId,
					request.MetodoPagamento,
					request.Parcelas,
					idempotencyKey,
					logPayload);

				// Fazer requisição para criar pagamento
				using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "payments")
				{
					Content = content
				};
				httpRequest.Headers.Add("X-Idempotency-Key", idempotencyKey);

				var response = await _httpClient.SendAsync(httpRequest);
				var responseBody = await response.Content.ReadAsStringAsync();

				_logger.LogInformation(
					"📨 Mercado Pago response: Status={StatusCode}, Body={Response}",
					(int)response.StatusCode,
					responseBody);

				if (response.IsSuccessStatusCode)
				{
					var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);
					if (result == null)
					{
						return new PagamentoResponseDTO
						{
							Sucesso = false,
							Mensagem = "Resposta inválida",
							CodigoErro = "RESPOSTA_INVALIDA"
						};
					}

					var status = result["status"].GetString();
					var id = result["id"].GetInt64().ToString();

					// Atualizar status do pedido
					pedido.PagamentoId = id;
					pedido.MetodoPagamento = request.MetodoPagamento;
					pedido.Status = status == "approved" ? StatusPedido.Pago : StatusPedido.AguardandoPagamento;
					await _unitOfWork.Pedidos.AtualizarAsync(pedido);
					await _unitOfWork.SalvarMudancasAsync();

					var pagamentoResponse = new PagamentoResponseDTO
					{
						Sucesso = true,
						PagamentoId = id,
						Status = status,
						Parcelas = request.Parcelas,
						ValorParcela = valorParcela,
						Mensagem = $"Pagamento de {pedido.ValorTotal:C} criado com sucesso."
					};

					// Adicionar informações de parcelamento
					if (request.Parcelas > 1)
					{
						pagamentoResponse.Mensagem += $" {request.Parcelas}x de {valorParcela:C}";
					}

					// Extrair QR Code do PIX se disponível
					if (request.MetodoPagamento == "pix" && result.TryGetValue("point_of_interaction", out var poi))
					{
						try
						{
							var poiStr = poi.ToString();
							if (!string.IsNullOrEmpty(poiStr))
							{
								var poiObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(poiStr);
								if (poiObj != null && poiObj.TryGetValue("transaction_data", out var transData))
								{
									var transStr = transData.ToString();
									if (!string.IsNullOrEmpty(transStr))
									{
										var transObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(transStr);
										if (transObj != null && transObj.TryGetValue("qr_code_base64", out var qr))
										{
											pagamentoResponse.QrCodeBase64 = qr.GetString();
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "Erro ao extrair QR Code PIX");
						}
					}

					return pagamentoResponse;
				}
				else
				{
					// Tentar extrair mensagem de erro do Mercado Pago
					string mensagemErro = "Erro ao processar pagamento";
					string codigoErro = "ERRO_MERCADO_PAGO";

					try
					{
						var errorResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);
						if (errorResult != null)
						{
							if (errorResult.TryGetValue("message", out var msg))
								mensagemErro = msg.GetString() ?? mensagemErro;
							if (errorResult.TryGetValue("error", out var err))
								codigoErro = err.GetString() ?? codigoErro;
						}
					}
					catch { }

					_logger.LogError("❌ Erro na API Mercado Pago: {StatusCode} - {Erro} - {Response}",
						response.StatusCode, codigoErro, responseBody);

					// IMPORTANTE: Manter pedido em "AguardandoPagamento" em caso de erro
					if (pedido.Status == StatusPedido.AguardandoPagamento || pedido.Status == StatusPedido.Pendente)
					{
						// Apenas atualiza se estiver em estado inicial
						await _unitOfWork.Pedidos.AtualizarAsync(pedido);
						await _unitOfWork.SalvarMudancasAsync();
					}

					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = mensagemErro,
						CodigoErro = codigoErro
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Erro ao criar pagamento para pedido {PedidoId}", request.PedidoId);
				return new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = ex.Message,
					CodigoErro = "ERRO_INTERNO"
				};
			}
		}

		public async Task<bool> ProcessarWebhookAsync(string pagamentoId)
		{
			try
			{
				_logger.LogInformation("🔄 ProcessarWebhook iniciado para pagamento {PagamentoId}", pagamentoId);

				// Buscar informações do pagamento no Mercado Pago
				var response = await _httpClient.GetAsync($"payments/{pagamentoId}");

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("Pagamento {PagamentoId} não encontrado no Mercado Pago", pagamentoId);
					return false;
				}

				var responseBody = await response.Content.ReadAsStringAsync();
				var payment = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);

				if (payment == null || !payment.TryGetValue("external_reference", out var externalRef))
				{
					_logger.LogWarning("Pagamento {PagamentoId} sem referência externa", pagamentoId);
					return false;
				}

				var externalReference = externalRef.GetString();
				_logger.LogInformation("📋 External reference encontrado: {ExternalRef}", externalReference);

				if (string.IsNullOrEmpty(externalReference))
				{
					return false;
				}

				// Buscar pedido pela referência externa (Guid)
				var pedidoId = Guid.Parse(externalReference);
				_logger.LogInformation("🔍 Buscando pedido {PedidoId}", pedidoId);

				var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(pedidoId);

				if (pedido == null)
				{
					_logger.LogWarning("Pedido {PedidoId} não encontrado para pagamento {PagamentoId}", pedidoId, pagamentoId);
					return false;
				}

				_logger.LogInformation("📦 Pedido encontrado: {NumeroPedido}, Status atual: {StatusAtual}", pedido.NumeroPedido, pedido.Status);

				// Atualizar status do pedido baseado no status do pagamento
				var statusAnterior = pedido.Status;
				var status = payment["status"].GetString();
				pedido.PagamentoId = pagamentoId;
				_logger.LogInformation("💳 Status do pagamento no MP: {StatusMP}", status);

				var novoStatus = status switch
				{
					"approved" => StatusPedido.Pago,
					"pending" or "in_process" => StatusPedido.AguardandoPagamento,
					"cancelled" or "rejected" => StatusPedido.Cancelado,
					_ => pedido.Status
				};

				if (statusAnterior == StatusPedido.Pago && novoStatus == StatusPedido.AguardandoPagamento)
				{
					_logger.LogWarning("⚠️ Ignorando downgrade de pedido pago para AguardandoPagamento (Pedido {PedidoId})", pedido.Id);
					return true;
				}

				pedido.Status = novoStatus;
				pedido.DataAtualizacao = DateTime.UtcNow;

				_logger.LogInformation("🎯 Novo status do pedido: {NovoStatus} (era {StatusAnterior})", pedido.Status, statusAnterior);

				if (statusAnterior != pedido.Status)
				{
					if (pedido.Status == StatusPedido.Cancelado)
					{
						await ProcessarCancelamentoPedidoAsync(pedido);
					}

					_logger.LogInformation("💾 Atualizando pedido no banco de dados...");
					await _unitOfWork.Pedidos.AtualizarAsync(pedido);
					await _unitOfWork.SalvarMudancasAsync();

					_logger.LogInformation(
						 "✅ Status do pedido {PedidoId} atualizado de {StatusAnterior} para {StatusNovo} (Pagamento: {PagamentoId})",
						 pedido.Id, statusAnterior, pedido.Status, pagamentoId);
				}
				else
				{
					_logger.LogInformation("⚠️ Status não mudou, nenhuma atualização necessária");
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao processar webhook para pagamento {PagamentoId}", pagamentoId);
				return false;
			}
		}

		/// <summary>
		/// Processa webhook simulado (para testes) usando status fornecido sem consultar o Mercado Pago
		/// </summary>
		public async Task<bool> ProcessarWebhookSimuladoAsync(string pagamentoId, string statusSimulado)
		{
			try
			{
				_logger.LogInformation("🔄 ProcessarWebhookSimulado iniciado para pagamento {PagamentoId} com status {Status}", pagamentoId, statusSimulado);

				// Buscar informações do pagamento no Mercado Pago para pegar o external_reference
				var response = await _httpClient.GetAsync($"payments/{pagamentoId}");

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("Pagamento {PagamentoId} não encontrado no Mercado Pago", pagamentoId);
					return false;
				}

				var responseBody = await response.Content.ReadAsStringAsync();
				var payment = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);

				if (payment == null || !payment.TryGetValue("external_reference", out var externalRef))
				{
					_logger.LogWarning("Pagamento {PagamentoId} sem referência externa", pagamentoId);
					return false;
				}

				var externalReference = externalRef.GetString();
				_logger.LogInformation("📋 External reference encontrado: {ExternalRef}", externalReference);

				if (string.IsNullOrEmpty(externalReference))
				{
					return false;
				}

				// Buscar pedido pela referência externa (Guid)
				var pedidoId = Guid.Parse(externalReference);
				_logger.LogInformation("🔍 Buscando pedido {PedidoId}", pedidoId);

				var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(pedidoId);

				if (pedido == null)
				{
					_logger.LogWarning("Pedido {PedidoId} não encontrado para pagamento {PagamentoId}", pedidoId, pagamentoId);
					return false;
				}

				_logger.LogInformation("📦 Pedido encontrado: {NumeroPedido}, Status atual: {StatusAtual}", pedido.NumeroPedido, pedido.Status);

				// Atualizar status do pedido baseado no status SIMULADO (não do MP)
				var statusAnterior = pedido.Status;
				_logger.LogInformation("💳 Status simulado: {StatusSimulado}", statusSimulado);
				pedido.PagamentoId = pagamentoId;

				var novoStatus = statusSimulado switch
				{
					"approved" => StatusPedido.Pago,
					"pending" or "in_process" => StatusPedido.AguardandoPagamento,
					"cancelled" or "rejected" => StatusPedido.Cancelado,
					_ => pedido.Status
				};

				if (statusAnterior == StatusPedido.Pago && novoStatus == StatusPedido.AguardandoPagamento)
				{
					_logger.LogWarning("⚠️ Ignorando downgrade de pedido pago para AguardandoPagamento (Pedido {PedidoId})", pedido.Id);
					return true;
				}

				pedido.Status = novoStatus;
				pedido.DataAtualizacao = DateTime.UtcNow;

				_logger.LogInformation("🎯 Novo status do pedido: {NovoStatus} (era {StatusAnterior})", pedido.Status, statusAnterior);

				if (statusAnterior != pedido.Status)
				{
					if (pedido.Status == StatusPedido.Cancelado)
					{
						await ProcessarCancelamentoPedidoAsync(pedido);
					}

					_logger.LogInformation("💾 Atualizando pedido no banco de dados...");
					await _unitOfWork.Pedidos.AtualizarAsync(pedido);
					await _unitOfWork.SalvarMudancasAsync();

					_logger.LogInformation(
						 "✅ Status do pedido {PedidoId} atualizado de {StatusAnterior} para {StatusNovo} (Pagamento: {PagamentoId})",
						 pedido.Id, statusAnterior, pedido.Status, pagamentoId);
				}
				else
				{
					_logger.LogInformation("⚠️ Status não mudou, nenhuma atualização necessária");
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao processar webhook simulado para pagamento {PagamentoId}", pagamentoId);
				return false;
			}
		}

		/// <summary>
		/// Permite retentar pagamento de um pedido que falhou ou está aguardando pagamento
		/// </summary>
		public async Task<PagamentoResponseDTO> RetentarPagamentoAsync(Guid pedidoId, CriarPagamentoRequestDTO request)
		{
			try
			{
				_logger.LogInformation("🔄 Reentando pagamento para pedido {PedidoId}", pedidoId);

				var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(pedidoId);

				if (pedido == null)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Pedido não encontrado",
						CodigoErro = "PEDIDO_NAO_ENCONTRADO"
					};
				}

				// Verificar se pedido está em estado que permite retry
				if (pedido.Status != StatusPedido.AguardandoPagamento && pedido.Status != StatusPedido.Pendente)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = $"Pedido não pode ser reprocessado. Status atual: {pedido.Status}",
						CodigoErro = "PEDIDO_STATUS_INVALIDO"
					};
				}

				_logger.LogInformation("✅ Pedido {PedidoId} em estado válido para retry. Criando novo pagamento...", pedidoId);

				// Usar CriarPagamentoAsync normalmente
				request.PedidoId = pedidoId;
				return await CriarPagamentoAsync(request);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao retentar pagamento para pedido {PedidoId}", pedidoId);
				return new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = "Erro ao retentar pagamento",
					CodigoErro = "ERRO_INTERNO"
				};
			}
		}

		public async Task<PagamentoResponseDTO> CancelarPagamentoAsync(Guid pedidoId)
		{
			try
			{
				_logger.LogInformation("🔄 Cancelamento/Estorno solicitado para pedido {PedidoId}", pedidoId);

				var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(pedidoId);
				if (pedido == null)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Pedido não encontrado",
						CodigoErro = "PEDIDO_NAO_ENCONTRADO"
					};
				}

				if (pedido.Status == StatusPedido.Cancelado)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = true,
						Mensagem = "Pedido já está cancelado"
					};
				}

				if (string.IsNullOrWhiteSpace(pedido.PagamentoId))
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "PagamentoId não informado no pedido",
						CodigoErro = "PAGAMENTO_ID_NAO_INFORMADO"
					};
				}

				// Consultar status atual do pagamento no Mercado Pago
				var response = await _httpClient.GetAsync($"payments/{pedido.PagamentoId}");
				if (!response.IsSuccessStatusCode)
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Não foi possível consultar o pagamento no Mercado Pago",
						CodigoErro = "MP_PAGAMENTO_NAO_ENCONTRADO"
					};
				}

				var responseBody = await response.Content.ReadAsStringAsync();
				var payment = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);
				if (payment == null || !payment.TryGetValue("status", out var statusElement))
				{
					return new PagamentoResponseDTO
					{
						Sucesso = false,
						Mensagem = "Status do pagamento não encontrado no Mercado Pago",
						CodigoErro = "MP_STATUS_NAO_ENCONTRADO"
					};
				}

				var statusPagamento = statusElement.GetString() ?? string.Empty;
				_logger.LogInformation("💳 Status atual do pagamento {PagamentoId}: {StatusMP}", pedido.PagamentoId, statusPagamento);

				// Se aprovado, solicitar estorno (refund)
				if (statusPagamento == "approved")
				{
					_logger.LogInformation("🔁 Solicitando estorno para pagamento {PagamentoId}", pedido.PagamentoId);
					var refundResponse = await _httpClient.PostAsJsonAsync($"payments/{pedido.PagamentoId}/refunds", new { });
					if (!refundResponse.IsSuccessStatusCode)
					{
						return new PagamentoResponseDTO
						{
							Sucesso = false,
							Mensagem = "Falha ao solicitar estorno no Mercado Pago",
							CodigoErro = "MP_ESTORNO_FALHOU"
						};
					}
				}
				// Se pendente/em processo, cancelar pagamento
				else if (statusPagamento == "pending" || statusPagamento == "in_process")
				{
					_logger.LogInformation("✖ Cancelando pagamento pendente {PagamentoId}", pedido.PagamentoId);
					var cancelResponse = await _httpClient.PutAsJsonAsync($"payments/{pedido.PagamentoId}", new { status = "cancelled" });
					if (!cancelResponse.IsSuccessStatusCode)
					{
						return new PagamentoResponseDTO
						{
							Sucesso = false,
							Mensagem = "Falha ao cancelar pagamento no Mercado Pago",
							CodigoErro = "MP_CANCELAMENTO_FALHOU"
						};
					}
				}
				// Se já cancelado/rejeitado, segue fluxo local

				pedido.Status = StatusPedido.Cancelado;
				pedido.DataAtualizacao = DateTime.UtcNow;
				await ProcessarCancelamentoPedidoAsync(pedido);
				await _unitOfWork.Pedidos.AtualizarAsync(pedido);
				await _unitOfWork.SalvarMudancasAsync();

				return new PagamentoResponseDTO
				{
					Sucesso = true,
					Mensagem = "Cancelamento/estorno solicitado com sucesso"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao cancelar/estornar pagamento do pedido {PedidoId}", pedidoId);
				return new PagamentoResponseDTO
				{
					Sucesso = false,
					Mensagem = "Erro ao cancelar/estornar pagamento",
					CodigoErro = "ERRO_INTERNO"
				};
			}
		}

		private async Task ProcessarCancelamentoPedidoAsync(LojaVirtual.Dominio.Entidades.Pedido pedido)
		{
			// Repor estoque dos itens e registrar movimentação de devolução
			foreach (var item in pedido.Itens)
			{
				if (!item.ProdutoTamanhoId.HasValue)
					continue;

				var tamanho = await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(item.ProdutoTamanhoId.Value);
				if (tamanho == null)
					continue;

				var estoqueAnterior = tamanho.QuantidadeEstoque;
				tamanho.QuantidadeEstoque += item.Quantidade;
				tamanho.DataAtualizacao = DateTime.UtcNow;
				await _unitOfWork.ProdutoTamanhos.AtualizarAsync(tamanho);

				var movimentacao = new LojaVirtual.Dominio.Entidades.MovimentacaoEstoque
				{
					ProdutoTamanhoId = tamanho.Id,
					Quantidade = item.Quantidade,
					Tipo = TipoMovimentacao.Devolucao,
					Motivo = $"Cancelamento - Pedido {pedido.NumeroPedido} - Tamanho {tamanho.Tamanho}",
					DataMovimentacao = DateTime.UtcNow,
					EstoqueAnterior = estoqueAnterior,
					EstoqueAtual = tamanho.QuantidadeEstoque,
					Referencia = pedido.NumeroPedido,
					Ativo = true
				};
				await _unitOfWork.MovimentacoesEstoque.AdicionarAsync(movimentacao);
			}

			// Desativar movimentações de saída ligadas ao pedido
			var movimentacoes = await _unitOfWork.MovimentacoesEstoque.ObterPorReferenciaAsync(pedido.NumeroPedido);
			foreach (var mov in movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida))
			{
				mov.Ativo = false;
				mov.DataAtualizacao = DateTime.UtcNow;
				await _unitOfWork.MovimentacoesEstoque.AtualizarAsync(mov);
			}

			await _notificacaoService.EnviarEmailCancelamentoPedidoAsync(pedido);
		}
	}
}
