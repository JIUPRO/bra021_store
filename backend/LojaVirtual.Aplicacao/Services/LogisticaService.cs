using System.Globalization;
using System.Text;
using System.Text.Json;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LojaVirtual.Aplicacao.Services
{
	public interface ILogisticaService
	{
		Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task ProcessarWebhookFrenetAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default);
	}

	public class LogisticaService : ILogisticaService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly INotificacaoService _notificacaoService;
		private readonly ILogger<LogisticaService> _logger;

		public LogisticaService(
			IUnitOfWork unitOfWork,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			INotificacaoService notificacaoService,
			ILogger<LogisticaService> logger)
		{
			_unitOfWork = unitOfWork;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_notificacaoService = notificacaoService;
			_logger = logger;
		}

		public async Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var provider = await ObterProviderLogisticoAtualAsync();
			await ValidarProviderLogisticoAtualAsync(provider);
			return await GerarEtiquetaFrenetAsync(pedidoId, cancellationToken);
		}

		public async Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var provider = await ObterProviderLogisticoAtualAsync();
			await ValidarProviderLogisticoAtualAsync(provider);
			return await SincronizarRastreioFrenetAsync(pedidoId, cancellationToken);
		}

		private async Task<GerarEtiquetaResponseDTO> GerarEtiquetaFrenetAsync(Guid pedidoId, CancellationToken cancellationToken)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			ValidarPedidoParaEtiqueta(pedido);

			var token = ObterTokenFrenet();
			var partnerToken = ObterPartnerTokenFrenet();
			var baseUrl = ObterBaseUrlFrenetWhitelabel();
			var client = CriarClientFrenet(token, partnerToken);

			var payload = MontarPayloadFrenetOneclick(pedido);
			var resposta = await EnviarFrenetAsync(client, HttpMethod.Post, $"{baseUrl}/v1/shipments/oneclick", payload, cancellationToken);

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
				Mensagem = "Solicitação de etiqueta enviada para a Frenet."
			};
		}

		private async Task<SincronizarRastreioResponseDTO> SincronizarRastreioFrenetAsync(Guid pedidoId, CancellationToken cancellationToken)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
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
				Mensagem = "Rastreio sincronizado com sucesso."
			};
		}

		public async Task ProcessarWebhookFrenetAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default)
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

		private async Task<string?> ObterProviderLogisticoAtualAsync()
		{
			return (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FreteProvider"))?.Valor?.Trim();
		}

		private Task ValidarProviderLogisticoAtualAsync(string? provider)
		{
			if (string.Equals(provider, "Frenet", StringComparison.OrdinalIgnoreCase))
			{
				return Task.CompletedTask;
			}

			throw new InvalidOperationException("A logística automática do backoffice está disponível apenas para o provider Frenet.");
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
			client.DefaultRequestHeaders.TryAddWithoutValidation("partner-token", partnerToken);
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
				var respostaEtiqueta = await EnviarFrenetAsync(
					client,
					HttpMethod.Get,
					$"{baseUrl}/v1/shipments/{Uri.EscapeDataString(pedido.IntegracaoFretePedidoId)}/label",
					null,
					cancellationToken);

				pedido.UrlEtiqueta = ExtrairStringFrenet(respostaEtiqueta, "LabelUrl", "labelUrl", "Url", "url", "PdfUrl", "pdfUrl")
					?? pedido.UrlEtiqueta;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Não foi possível obter a etiqueta Frenet do pedido {PedidoId}.", pedido.Id);
			}
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

		private Dictionary<string, object?> MontarPayloadFrenetOneclick(Pedido pedido)
		{
			var destinatarioNome = string.Equals(pedido.TipoEntrega, "Escola", StringComparison.OrdinalIgnoreCase)
				? pedido.Escola?.Nome ?? pedido.NomeEntrega
				: pedido.NomeEntrega;

			var volumes = new[]
			{
				new Dictionary<string, object?>
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
				}
			};

			var products = pedido.Itens.Select(item => new Dictionary<string, object?>
			{
				["Sku"] = item.ProdutoId.ToString(),
				["Name"] = item.Produto?.Nome ?? "Produto",
				["Quantity"] = item.Quantidade,
				["UnitPrice"] = decimal.Round(item.PrecoUnitario, 2)
			}).ToList();

			return new Dictionary<string, object?>
			{
				["OrderId"] = pedido.NumeroPedido,
				["RecipientName"] = destinatarioNome,
				["RecipientEmail"] = pedido.Cliente?.Email ?? string.Empty,
				["RecipientPhone"] = LimparNumero(pedido.TelefoneEntrega),
				["RecipientZipCode"] = LimparNumero(pedido.CepEntrega),
				["RecipientAddress"] = pedido.LogradouroEntrega,
				["RecipientAddressNumber"] = pedido.NumeroEntrega,
				["RecipientAddressComplement"] = pedido.ComplementoEntrega,
				["RecipientNeighborhood"] = pedido.BairroEntrega,
				["RecipientCity"] = pedido.CidadeEntrega,
				["RecipientState"] = pedido.EstadoEntrega,
				["ServiceCode"] = pedido.CodigoServicoFrete,
				["InvoiceValue"] = decimal.Round(pedido.ValorSubtotal, 2),
				["Volumes"] = volumes,
				["Products"] = products
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
	}
}
