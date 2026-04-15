using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LojaVirtual.Aplicacao.Services
{
	public class FrenetLogisticaProvider : IFrenetLogisticaProvider
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly INotificacaoService _notificacaoService;
		private readonly ILogger<FrenetLogisticaProvider> _logger;

		public FrenetLogisticaProvider(
			IUnitOfWork unitOfWork,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			INotificacaoService notificacaoService,
			ILogger<FrenetLogisticaProvider> logger)
		{
			_unitOfWork = unitOfWork;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_notificacaoService = notificacaoService;
			_logger = logger;
		}

		public string ProviderName => FreteProviderNames.Frenet;

		public async Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			ValidarPedidoParaEtiqueta(pedido);
			pedido.ProviderLogisticaUtilizado ??= FreteProviderNames.Frenet;
			pedido.ProviderFreteUtilizado ??= FreteProviderNames.Frenet;
			var remetente = await ObterRemetenteFrenetAsync();

			var token = ObterTokenFrenet();
			var partnerToken = ObterPartnerTokenFrenet();
			var baseUrl = ObterBaseUrlFrenetWhitelabel();
			var client = CriarClientFrenet(token, partnerToken);

			var payload = MontarPayloadFrenetOneclick(pedido, remetente);
			var resposta = await EnviarFrenetAsync(client, HttpMethod.Post, $"{baseUrl}/v1/shipments/oneclick", payload, cancellationToken);
			await ProcessarRespostaOneclickFrenetAsync(client, baseUrl, pedido, resposta, cancellationToken);

			pedido.IntegracaoFreteProtocolo ??= pedido.NumeroPedido;
			pedido.IntegracaoFretePedidoId = ExtrairStringFrenet(resposta, "ShipmentId", "shipmentId", "Id", "id")
				?? pedido.IntegracaoFretePedidoId;
			pedido.UrlEtiqueta = ExtrairStringFrenet(resposta, "LabelUrl", "labelUrl", "Url", "url", "PdfUrl", "pdfUrl")
				?? pedido.UrlEtiqueta;
			pedido.UrlRastreio = ExtrairStringFrenet(resposta, "TrackingUrl", "trackingUrl")
				?? pedido.UrlRastreio;
			pedido.CodigoRastreio = ExtrairStringFrenet(resposta, "TrackingNumber", "trackingNumber")
				?? pedido.CodigoRastreio;
			pedido.StatusLogistico = ExtrairStringFrenet(resposta, "ShipmentStatusDescription", "StatusDescription", "Status", "status")
				?? "generated";
			pedido.DataEtiquetaGerada ??= DateTime.UtcNow;

			if (!string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				await TentarAtualizarEtiquetaFrenetAsync(client, baseUrl, pedido, cancellationToken);
				await AtualizarDadosPesquisaFrenetAsync(client, baseUrl, pedido, cancellationToken);
			}

			await SalvarPedidoLogisticoAsync(pedido);

			return new GerarEtiquetaResponseDTO
			{
				PedidoId = pedido.Id.ToString(),
				IntegracaoFretePedidoId = pedido.IntegracaoFretePedidoId,
				IntegracaoFreteProtocolo = pedido.IntegracaoFreteProtocolo,
				CodigoRastreio = pedido.CodigoRastreio,
				UrlRastreio = pedido.UrlRastreio,
				UrlEtiqueta = pedido.UrlEtiqueta,
				StatusLogistico = pedido.StatusLogistico,
				DataEtiquetaGerada = pedido.DataEtiquetaGerada,
				Mensagem = MontarMensagemGeracaoEtiqueta(pedido)
			};
		}

		public async Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			pedido.ProviderLogisticaUtilizado ??= FreteProviderNames.Frenet;
			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				throw new InvalidOperationException("O pedido ainda não possui envio gerado na Frenet.");
			}

			var token = ObterTokenFrenet();
			var partnerToken = ObterPartnerTokenFrenet();
			var baseUrl = ObterBaseUrlFrenetWhitelabel();
			var client = CriarClientFrenet(token, partnerToken);

			await AtualizarDadosPesquisaFrenetAsync(client, baseUrl, pedido, cancellationToken);
			await TentarAtualizarEtiquetaFrenetAsync(client, baseUrl, pedido, cancellationToken);
			await SalvarPedidoLogisticoAsync(pedido);

			return new SincronizarRastreioResponseDTO
			{
				PedidoId = pedido.Id.ToString(),
				IntegracaoFretePedidoId = pedido.IntegracaoFretePedidoId,
				CodigoRastreio = pedido.CodigoRastreio,
				UrlRastreio = pedido.UrlRastreio,
				StatusLogistico = pedido.StatusLogistico,
				DataPostagem = pedido.DataPostagem,
				DataEntrega = pedido.DataEntrega,
				Mensagem = MontarMensagemSincronizacaoRastreio(pedido)
			};
		}

		public Task<ArquivoEtiquetaDTO> ObterArquivoEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			_ = pedidoId;
			_ = cancellationToken;
			throw new InvalidOperationException("A etiqueta da Frenet deve ser acessada pela URL pública já sincronizada no pedido.");
		}

		public async Task ProcessarWebhookAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default)
		{
			_ = cancellationToken;
			ValidarTokenWebhookFrenet(headers);

			var payload = JsonSerializer.Deserialize<FrenetTrackingWebhookDTO>(rawBody, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? throw new InvalidOperationException("Payload de webhook Frenet inválido.");

			if (string.IsNullOrWhiteSpace(payload.OrderId))
			{
				throw new InvalidOperationException("Webhook Frenet sem OrderId.");
			}

			var pedido = await ObterPedidoPorOrderIdFrenetAsync(payload.OrderId);
			if (pedido == null)
			{
				_logger.LogWarning("Webhook Frenet ignorado. Pedido não encontrado para OrderId {OrderId}.", payload.OrderId);
				return;
			}

			if (payload.ShipmentId.HasValue)
			{
				pedido.IntegracaoFretePedidoId = payload.ShipmentId.Value.ToString(CultureInfo.InvariantCulture);
			}

			pedido.UrlRastreio = payload.TrackingUrl ?? pedido.UrlRastreio;
			pedido.CodigoRastreio = payload.TrackingNumber ?? pedido.CodigoRastreio;
			pedido.ServicoFrete ??= payload.ServiceDescrition;

			var ultimoEvento = payload.TrackingEvents
				.OrderByDescending(e => ParseDataEventoFrenet(e.EventDateTime))
				.FirstOrDefault();

			if (ultimoEvento != null)
			{
				var dataEvento = ParseDataEventoFrenet(ultimoEvento.EventDateTime);
				pedido.StatusLogistico = ultimoEvento.EventDescription ?? pedido.StatusLogistico;

				switch (ultimoEvento.EventType)
				{
					case "0":
					case "18":
						pedido.DataPostagem ??= dataEvento;
						if (pedido.Status < StatusPedido.Enviado)
						{
							pedido.Status = StatusPedido.Enviado;
						}
						break;
					case "9":
						pedido.DataEntrega ??= dataEvento;
						pedido.Status = StatusPedido.Entregue;
						break;
				}
			}

			pedido.DataAtualizacao = DateTime.UtcNow;
			await SalvarPedidoLogisticoAsync(pedido);
		}

		private async Task ProcessarRespostaOneclickFrenetAsync(
			HttpClient client,
			string baseUrl,
			Pedido pedido,
			JsonElement resposta,
			CancellationToken cancellationToken)
		{
			var shipmentId = ExtrairStringFrenet(resposta, "ShipmentId", "shipmentId", "Id", "id");
			if (!string.IsNullOrWhiteSpace(shipmentId))
			{
				pedido.IntegracaoFretePedidoId = shipmentId;
				return;
			}

			if (!resposta.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
			{
				return;
			}

			foreach (var item in items.EnumerateArray())
			{
				var itemShipmentId = ExtrairStringFrenet(item, "ShipmentId", "shipmentId", "Id", "id");
				if (!string.IsNullOrWhiteSpace(itemShipmentId))
				{
					pedido.IntegracaoFretePedidoId = itemShipmentId;
					return;
				}

				var errorMessage = ExtrairMensagemErroItemFrenet(item);
				if (string.IsNullOrWhiteSpace(errorMessage))
				{
					continue;
				}

				var shipmentExistente = ExtrairShipmentExistenteFrenet(errorMessage);
				if (string.IsNullOrWhiteSpace(shipmentExistente))
				{
					continue;
				}

				_logger.LogWarning(
					"Frenet informou pedido já integrado no envio {ShipmentId}. Tentando sincronizar dados do envio existente.",
					shipmentExistente);

				pedido.IntegracaoFretePedidoId = shipmentExistente;
				await TentarAtualizarEtiquetaFrenetAsync(client, baseUrl, pedido, cancellationToken);
				await AtualizarDadosPesquisaFrenetAsync(client, baseUrl, pedido, cancellationToken);
				return;
			}
		}

		private async Task<Pedido> ObterPedidoLogisticoAsync(Guid pedidoId)
		{
			var pedido = await _unitOfWork.Pedidos.ObterComItensAsync(pedidoId);
			if (pedido == null)
			{
				throw new InvalidOperationException("Pedido não encontrado.");
			}

			pedido.Cliente = await _unitOfWork.Clientes.ObterPorIdAsync(pedido.ClienteId) ?? pedido.Cliente;
			if (pedido.EscolaId.HasValue)
			{
				pedido.Escola = await _unitOfWork.Escolas.ObterPorIdAsync(pedido.EscolaId.Value);
			}

			return pedido;
		}

		private async Task<Pedido?> ObterPedidoPorOrderIdFrenetAsync(string orderId)
		{
			var normalizado = orderId.Trim();
			var pedidos = await _unitOfWork.Pedidos.ObterPorFiltroAsync(p =>
				p.NumeroPedido == normalizado ||
				p.Id.ToString() == normalizado);

			return pedidos.FirstOrDefault();
		}

		private async Task<RemetenteFrenetDTO?> ObterRemetenteFrenetAsync()
		{
			var remetente = new RemetenteFrenetDTO
			{
				Nome = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteNome"))?.Valor?.Trim(),
				Telefone = LimparNumero((await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteTelefone"))?.Valor),
				Email = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteEmail"))?.Valor?.Trim(),
				Documento = LimparNumero((await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteDocumento"))?.Valor),
				InscricaoEstadual = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteInscricaoEstadual"))?.Valor?.Trim(),
				Logradouro = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteLogradouro"))?.Valor?.Trim(),
				Numero = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteNumero"))?.Valor?.Trim(),
				Complemento = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteComplemento"))?.Valor?.Trim(),
				Bairro = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteBairro"))?.Valor?.Trim(),
				Cidade = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteCidade"))?.Valor?.Trim(),
				Estado = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaRemetenteEstado"))?.Valor?.Trim(),
				Cep = LimparNumero((await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FreteCepOrigem"))?.Valor)
			};

			if (!remetente.EstaCompleto)
			{
				_logger.LogWarning("Remetente Frenet incompleto. Será usado UseFrenetRegistration=true.");
				return null;
			}

			return remetente;
		}

		private void ValidarPedidoParaEtiqueta(Pedido pedido)
		{
			if (string.IsNullOrWhiteSpace(pedido.CodigoServicoFrete))
			{
				throw new InvalidOperationException("O pedido não possui um serviço de frete integrado vinculado.");
			}

			if (string.IsNullOrWhiteSpace(pedido.CepEntrega) ||
				string.IsNullOrWhiteSpace(pedido.LogradouroEntrega) ||
				string.IsNullOrWhiteSpace(pedido.NumeroEntrega) ||
				string.IsNullOrWhiteSpace(pedido.BairroEntrega) ||
				string.IsNullOrWhiteSpace(pedido.CidadeEntrega) ||
				string.IsNullOrWhiteSpace(pedido.EstadoEntrega))
			{
				throw new InvalidOperationException("O pedido não possui endereço de entrega completo para gerar a etiqueta.");
			}

			if (!pedido.Itens.Any())
			{
				throw new InvalidOperationException("O pedido não possui itens.");
			}
		}

		private string ObterTokenFrenet()
		{
			return _configuration["Frenet:HomologacaoToken"]
				?? throw new InvalidOperationException("Frenet:HomologacaoToken não configurado.");
		}

		private string ObterPartnerTokenFrenet()
		{
			return _configuration["Frenet:HomologacaoPartnerToken"]
				?? throw new InvalidOperationException("Frenet:HomologacaoPartnerToken não configurado.");
		}

		private string ObterBaseUrlFrenetWhitelabel()
		{
			var valor = _configuration["Frenet:WhitelabelBaseUrl"];
			if (!string.IsNullOrWhiteSpace(valor))
			{
				return valor.TrimEnd('/');
			}

			return "https://whitelabel-hml.frenet.dev";
		}

		private HttpClient CriarClientFrenet(string token, string partnerToken)
		{
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
			client.DefaultRequestHeaders.TryAddWithoutValidation("token", token);
			client.DefaultRequestHeaders.TryAddWithoutValidation("x-partner-token", partnerToken);
			client.DefaultRequestHeaders.TryAddWithoutValidation("x-printing-format", "A4");
			return client;
		}

		private async Task<JsonElement> EnviarFrenetAsync(HttpClient client, HttpMethod method, string url, object? body, CancellationToken cancellationToken)
		{
			var request = new HttpRequestMessage(method, url);
			if (body != null)
			{
				var json = JsonSerializer.Serialize(body);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
				_logger.LogInformation("Frenet request {Method} {Url}: {Body}", method, url, json);
			}

			_logger.LogInformation(
				"Frenet auth headers {Method} {Url}: token={TokenMascara}, x-partner-token={PartnerTokenMascara}, x-printing-format={PrintingFormat}",
				method,
				url,
				MascararToken(client.DefaultRequestHeaders.TryGetValues("token", out var tokenValues)
					? tokenValues.FirstOrDefault()
					: null),
				MascararToken(client.DefaultRequestHeaders.TryGetValues("x-partner-token", out var partnerValues)
					? partnerValues.FirstOrDefault()
					: null),
				client.DefaultRequestHeaders.TryGetValues("x-printing-format", out var printingValues)
					? printingValues.FirstOrDefault()
					: null);

			var response = await client.SendAsync(request, cancellationToken);
			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogInformation("Frenet response {StatusCode} {Url}: {Body}", response.StatusCode, url, content);

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Frenet retornou {(int)response.StatusCode}: {content}");
			}

			using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
			return document.RootElement.Clone();
		}

		private async Task AtualizarDadosPesquisaFrenetAsync(HttpClient client, string baseUrl, Pedido pedido, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				return;
			}

			var envio = await EnviarFrenetAsync(
				client,
				HttpMethod.Get,
				$"{baseUrl}/v1/shipments/{Uri.EscapeDataString(pedido.IntegracaoFretePedidoId)}",
				null,
				cancellationToken);

			pedido.CodigoRastreio = ExtrairStringFrenet(envio, "TrackingNumber", "trackingNumber", "Code", "code")
				?? pedido.CodigoRastreio;
			pedido.UrlRastreio = ExtrairStringFrenet(envio, "TrackingUrl", "trackingUrl", "Url", "url")
				?? pedido.UrlRastreio;
			pedido.StatusLogistico = ExtrairStringFrenet(envio, "ShipmentStatusDescription", "StatusDescription", "ShipmentStatus", "status")
				?? pedido.StatusLogistico;

			var postadoEm = ExtrairDateTimeFrenet(envio, "PostedAt", "postedAt", "PostDate");
			var entregueEm = ExtrairDateTimeFrenet(envio, "DeliveredAt", "deliveredAt", "DeliveryDate");
			pedido.DataPostagem ??= postadoEm;
			pedido.DataEntrega ??= entregueEm;

			if (pedido.DataEntrega.HasValue && pedido.Status < StatusPedido.Entregue)
			{
				pedido.Status = StatusPedido.Entregue;
			}
			else if (pedido.DataPostagem.HasValue && pedido.Status < StatusPedido.Enviado)
			{
				pedido.Status = StatusPedido.Enviado;
			}
		}

		private async Task SalvarPedidoLogisticoAsync(Pedido pedido)
		{
			var statusAnterior = pedido.Status;
			await _unitOfWork.Pedidos.AtualizarAsync(pedido);
			await _unitOfWork.SalvarMudancasAsync();

			if (pedido.Status != statusAnterior)
			{
				_ = _notificacaoService.EnviarEmailAlteracaoStatusAsync(pedido);
			}
		}

		private async Task TentarAtualizarEtiquetaFrenetAsync(
			HttpClient client,
			string baseUrl,
			Pedido pedido,
			CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId) || !string.IsNullOrWhiteSpace(pedido.UrlEtiqueta))
			{
				return;
			}

			try
			{
				var respostaEtiqueta = await TentarObterEtiquetaBatchFrenetAsync(
					client,
					baseUrl,
					pedido.IntegracaoFretePedidoId,
					cancellationToken)
					?? await EnviarFrenetAsync(
						client,
						HttpMethod.Get,
						$"{baseUrl}/v1/shipments/{Uri.EscapeDataString(pedido.IntegracaoFretePedidoId)}/label",
						null,
						cancellationToken);

				pedido.UrlEtiqueta = ExtrairStringFrenet(respostaEtiqueta, "LabelUrl", "labelUrl", "Url", "url", "PdfUrl", "pdfUrl")
					?? pedido.UrlEtiqueta;
				pedido.UrlRastreio = ExtrairStringFrenet(respostaEtiqueta, "TrackingUrl", "trackingUrl")
					?? pedido.UrlRastreio;
				pedido.CodigoRastreio = ExtrairStringFrenet(respostaEtiqueta, "TrackingNumber", "trackingNumber")
					?? pedido.CodigoRastreio;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Não foi possível obter a etiqueta Frenet do pedido {PedidoId}.", pedido.Id);
			}
		}

		private async Task<JsonElement?> TentarObterEtiquetaBatchFrenetAsync(
			HttpClient client,
			string baseUrl,
			string shipmentId,
			CancellationToken cancellationToken)
		{
			if (!long.TryParse(shipmentId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shipmentIdLong))
			{
				_logger.LogWarning(
					"Não foi possível converter o envio Frenet {ShipmentId} para inteiro antes de consultar a etiqueta em lote.",
					shipmentId);
				return null;
			}

			var candidatos = new object?[]
			{
				new long?[] { shipmentIdLong },
				new long[] { shipmentIdLong },
				new[] { new Dictionary<string, object?> { ["ShipmentId"] = shipmentIdLong } },
				new Dictionary<string, object?> { ["ShipmentIds"] = new long[] { shipmentIdLong } }
			};

			foreach (var body in candidatos)
			{
				try
				{
					var resposta = await EnviarFrenetAsync(
						client,
						HttpMethod.Post,
						$"{baseUrl}/v1/shipments/batch/label",
						body,
						cancellationToken);

					return resposta;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(
						ex,
						"Não foi possível obter a etiqueta Frenet em lote para o envio {ShipmentId} com o payload candidato {TipoPayload}.",
						shipmentId,
						body?.GetType().Name ?? "null");
				}
			}

			return null;
		}

		private void ValidarTokenWebhookFrenet(IReadOnlyDictionary<string, string?> headers)
		{
			var tokenName = _configuration["Frenet:WebhookTokenName"];
			var tokenValue = _configuration["Frenet:WebhookTokenValue"];

			if (string.IsNullOrWhiteSpace(tokenName) || string.IsNullOrWhiteSpace(tokenValue))
			{
				return;
			}

			headers.TryGetValue(tokenName, out var valorRecebido);
			if (!string.Equals(valorRecebido, tokenValue, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("Token do webhook Frenet inválido.");
			}
		}

		private static DateTime? ParseDataEventoFrenet(string? valor)
		{
			if (string.IsNullOrWhiteSpace(valor))
			{
				return null;
			}

			if (DateTime.TryParseExact(
				valor,
				"dd/MM/yyyy HH:mm",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeLocal,
				out var data))
			{
				return data.ToUniversalTime();
			}

			return null;
		}

		private List<Dictionary<string, object?>> MontarPayloadFrenetOneclick(Pedido pedido, RemetenteFrenetDTO? remetente)
		{
			var destinatarioNome = string.Equals(pedido.TipoEntrega, "Escola", StringComparison.OrdinalIgnoreCase)
				? pedido.Escola?.Nome ?? pedido.NomeEntrega
				: pedido.NomeEntrega;

			var orderItems = pedido.Itens.Select(item =>
			{
				var pesoUnitario = item.ProdutoTamanho != null && item.ProdutoTamanho.Peso > 0
					? item.ProdutoTamanho.Peso
					: 0.1d;
				var altura = item.ProdutoTamanho?.Altura ?? 1;
				var largura = item.ProdutoTamanho?.Largura ?? 1;
				var comprimento = item.ProdutoTamanho?.Profundidade ?? 1;
				var opcoes = item.ProdutoTamanho?.Tamanho;

				return new Dictionary<string, object?>
				{
					["OrderId"] = pedido.NumeroPedido,
					["ItemId"] = item.Id.ToString(),
					["ProductId"] = item.ProdutoId.ToString(),
					["ProductOptions"] = string.IsNullOrWhiteSpace(opcoes) ? null : opcoes,
					["ProductType"] = "physical",
					["Weight"] = decimal.Round((decimal)pesoUnitario, 3),
					["Length"] = comprimento > 0 ? comprimento : 1,
					["Height"] = altura > 0 ? altura : 1,
					["Width"] = largura > 0 ? largura : 1,
					["Quantity"] = item.Quantidade,
					["Price"] = decimal.Round(item.PrecoUnitario, 2),
					["IsFragile"] = false,
					["ProductName"] = item.Produto?.Nome ?? "Produto",
					["SKU"] = item.ProdutoId.ToString(),
					["Category"] = item.Produto != null ? item.Produto.CategoriaId.ToString() : null
				};
			}).ToList();

			var volume = new Dictionary<string, object?>
			{
				["Height"] = pedido.Itens.Max(item => NormalizarDimensao(item.ProdutoTamanho?.Altura)),
				["Width"] = pedido.Itens.Max(item => NormalizarDimensao(item.ProdutoTamanho?.Largura)),
				["Length"] = Math.Max(1, pedido.Itens.Sum(item => NormalizarDimensao(item.ProdutoTamanho?.Profundidade))),
				["Weight"] = decimal.Round(pedido.Itens.Sum(item =>
				{
					var pesoUnitario = item.ProdutoTamanho != null && item.ProdutoTamanho.Peso > 0
						? item.ProdutoTamanho.Peso
						: 0.1d;
					return (decimal)(pesoUnitario * item.Quantidade);
				}), 3)
			};
			volume["Price"] = decimal.Round(pedido.ValorSubtotal, 2);
			volume["DeclaredValue"] = decimal.Round(pedido.ValorSubtotal, 2);
			volume["OrderItemsId"] = pedido.Itens.Select(item => item.Id.ToString()).ToList();

			var dataCriacaoPedido = pedido.DataPedido
				.ToUniversalTime()
				.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

			var order = new Dictionary<string, object?>
			{
				["Id"] = pedido.NumeroPedido,
				["Value"] = decimal.Round(pedido.ValorTotal, 2),
				["Created"] = dataCriacaoPedido,
				["UseFrenetRegistration"] = remetente == null,
				["Items"] = orderItems,
				["To"] = new Dictionary<string, object?>
				{
					["Email"] = pedido.Cliente?.Email ?? string.Empty,
					["Name"] = destinatarioNome,
					["Phone"] = LimparNumero(pedido.TelefoneEntrega),
					["Cellphone"] = LimparNumero(pedido.TelefoneEntrega),
					["Document"] = LimparNumero(pedido.Cliente?.Cpf),
					["Address"] = new Dictionary<string, object?>
					{
						["ZipCode"] = LimparNumero(pedido.CepEntrega),
						["City"] = pedido.CidadeEntrega,
						["Street"] = pedido.LogradouroEntrega,
						["AddressNumber"] = pedido.NumeroEntrega,
						["AddressComplement"] = pedido.ComplementoEntrega,
						["AddressQuarter"] = pedido.BairroEntrega,
						["AddressState"] = pedido.EstadoEntrega,
						["Country"] = "BR"
					}
				}
			};

			if (remetente != null)
			{
				order["From"] = new Dictionary<string, object?>
				{
					["Email"] = remetente.Email,
					["Name"] = remetente.Nome,
					["Phone"] = remetente.Telefone,
					["Cellphone"] = remetente.Telefone,
					["Document"] = remetente.Documento,
					["IE"] = remetente.InscricaoEstadual,
					["Address"] = new Dictionary<string, object?>
					{
						["ZipCode"] = remetente.Cep,
						["City"] = remetente.Cidade,
						["Street"] = remetente.Logradouro,
						["AddressNumber"] = remetente.Numero,
						["AddressComplement"] = remetente.Complemento,
						["AddressQuarter"] = remetente.Bairro,
						["AddressState"] = remetente.Estado,
						["Country"] = "BR"
					}
				};
			}

			var quotation = new Dictionary<string, object?>
			{
				["ShippingServiceCode"] = pedido.CodigoServicoFrete,
				["ShippingServiceName"] = pedido.ServicoFrete,
				["PlatformShippingPrice"] = decimal.Round(pedido.ValorFrete, 2),
				["DeliveryTime"] = pedido.PrazoEntregaDias,
				["Carrier"] = pedido.TransportadoraFrete,
				["CarrierCode"] = ObterCarrierCodeFrenet(pedido.TransportadoraFrete),
				["ShippingPrice"] = decimal.Round(pedido.ValorFrete, 2),
				["ShippingCompetitorPrice"] = decimal.Round(pedido.ValorFrete, 2),
				["Services"] = new Dictionary<string, object?>
				{
					["DeclaredValue"] = false,
					["ReceiptNotification"] = false,
					["OwnHand"] = false
				}
			};

			return new List<Dictionary<string, object?>>
			{
				new()
				{
					["TrackingNotificationUrl"] = null,
					["StatusNotificationUrl"] = null,
					["Order"] = order,
					["Volumes"] = volume,
					["Quotation"] = quotation
				}
			};
		}

		private static string? ExtrairStringFrenet(JsonElement element, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				if (element.ValueKind == JsonValueKind.Object &&
					element.TryGetProperty(propertyName, out var property) &&
					property.ValueKind != JsonValueKind.Null &&
					!string.IsNullOrWhiteSpace(property.ToString()))
				{
					return property.ToString();
				}
			}

			if (element.ValueKind == JsonValueKind.Object)
			{
				foreach (var property in element.EnumerateObject())
				{
					if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
					{
						var nested = ExtrairStringFrenet(property.Value, propertyNames);
						if (!string.IsNullOrWhiteSpace(nested))
						{
							return nested;
						}
					}
				}
			}
			else if (element.ValueKind == JsonValueKind.Array)
			{
				foreach (var item in element.EnumerateArray())
				{
					var nested = ExtrairStringFrenet(item, propertyNames);
					if (!string.IsNullOrWhiteSpace(nested))
					{
						return nested;
					}
				}
			}

			return null;
		}

		private static string? ExtrairMensagemErroItemFrenet(JsonElement item)
		{
			if (!item.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Array)
			{
				return null;
			}

			foreach (var error in errors.EnumerateArray())
			{
				var message = ExtrairStringFrenet(error, "Message", "message");
				if (!string.IsNullOrWhiteSpace(message))
				{
					return message;
				}
			}

			return null;
		}

		private static string? ExtrairShipmentExistenteFrenet(string mensagem)
		{
			if (string.IsNullOrWhiteSpace(mensagem))
			{
				return null;
			}

			var match = Regex.Match(
				mensagem,
				@"etiqueta de n[uú]mero\s+(?<id>\d+)",
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			return match.Success ? match.Groups["id"].Value : null;
		}

		private static DateTime? ExtrairDateTimeFrenet(JsonElement element, params string[] propertyNames)
		{
			var valor = ExtrairStringFrenet(element, propertyNames);
			if (string.IsNullOrWhiteSpace(valor))
			{
				return null;
			}

			if (DateTimeOffset.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var offset))
			{
				return offset.UtcDateTime;
			}

			if (DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
			{
				return dateTime.ToUniversalTime();
			}

			return null;
		}

		private static int NormalizarDimensao(double? valor)
		{
			if (!valor.HasValue || valor.Value <= 0)
			{
				return 1;
			}

			return Math.Max(1, (int)Math.Ceiling(valor.Value));
		}

		private static string LimparNumero(string? valor)
		{
			return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
		}

		private static string? ObterCarrierCodeFrenet(string? transportadora)
		{
			if (string.IsNullOrWhiteSpace(transportadora))
			{
				return null;
			}

			var normalizado = transportadora.Trim();
			if (normalizado.Contains("Correios", StringComparison.OrdinalIgnoreCase))
			{
				return "COR";
			}

			return normalizado;
		}

		private static string MontarMensagemGeracaoEtiqueta(Pedido pedido)
		{
			if (!string.IsNullOrWhiteSpace(pedido.UrlEtiqueta))
			{
				return "Etiqueta sincronizada com sucesso.";
			}

			if (!string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				return $"Envio registrado na Frenet sob o ID {pedido.IntegracaoFretePedidoId}, mas a API ainda não retornou a URL da etiqueta. Consulte o painel Frenet ou tente atualizar novamente.";
			}

			return "Solicitação de etiqueta enviada para a Frenet.";
		}

		private static string MontarMensagemSincronizacaoRastreio(Pedido pedido)
		{
			if (!string.IsNullOrWhiteSpace(pedido.CodigoRastreio) || !string.IsNullOrWhiteSpace(pedido.UrlRastreio))
			{
				return "Rastreio sincronizado com sucesso.";
			}

			if (!string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				return $"Envio {pedido.IntegracaoFretePedidoId} localizado na Frenet, mas o rastreio ainda não foi disponibilizado pela API.";
			}

			return "Rastreio sincronizado com sucesso.";
		}

		private sealed class RemetenteFrenetDTO
		{
			public string? Nome { get; set; }
			public string? Telefone { get; set; }
			public string? Email { get; set; }
			public string? Documento { get; set; }
			public string? InscricaoEstadual { get; set; }
			public string? Logradouro { get; set; }
			public string? Numero { get; set; }
			public string? Complemento { get; set; }
			public string? Bairro { get; set; }
			public string? Cidade { get; set; }
			public string? Estado { get; set; }
			public string? Cep { get; set; }

			public bool EstaCompleto =>
				!string.IsNullOrWhiteSpace(Nome) &&
				!string.IsNullOrWhiteSpace(Telefone) &&
				!string.IsNullOrWhiteSpace(Email) &&
				!string.IsNullOrWhiteSpace(Documento) &&
				!string.IsNullOrWhiteSpace(Logradouro) &&
				!string.IsNullOrWhiteSpace(Numero) &&
				!string.IsNullOrWhiteSpace(Bairro) &&
				!string.IsNullOrWhiteSpace(Cidade) &&
				!string.IsNullOrWhiteSpace(Estado) &&
				!string.IsNullOrWhiteSpace(Cep);
		}

		private static string MascararToken(string? valor)
		{
			if (string.IsNullOrWhiteSpace(valor))
			{
				return "(vazio)";
			}

			var trimmed = valor.Trim();
			if (trimmed.Length <= 8)
			{
				return trimmed;
			}

			return $"{trimmed[..4]}...{trimmed[^4..]}";
		}
	}
}
