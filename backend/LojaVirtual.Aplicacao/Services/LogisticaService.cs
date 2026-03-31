using System.Globalization;
using System.Security.Cryptography;
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
		Task ProcessarWebhookAsync(string rawBody, string? signature, CancellationToken cancellationToken = default);
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
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			ValidarPedidoParaEtiqueta(pedido);

			var remetente = await ObterRemetenteAsync();
			var accessToken = ObterAccessToken();
			var baseUrl = ObterBaseUrl();
			var client = CriarClient(accessToken);

			if (string.IsNullOrWhiteSpace(pedido.MelhorEnvioPedidoId))
			{
				var payloadCarrinho = MontarPayloadCarrinho(pedido, remetente);
				var respostaCarrinho = await EnviarAsync(client, HttpMethod.Post, $"{baseUrl}/api/v2/me/cart", payloadCarrinho, cancellationToken);
				pedido.MelhorEnvioPedidoId = ObterString(respostaCarrinho, "id");
				pedido.MelhorEnvioProtocolo = ObterString(respostaCarrinho, "protocol");
			}

			if (string.IsNullOrWhiteSpace(pedido.MelhorEnvioPedidoId))
			{
				throw new InvalidOperationException("O Melhor Envio não retornou o identificador da etiqueta.");
			}

			var payloadPedido = new Dictionary<string, object?> { ["orders"] = new[] { pedido.MelhorEnvioPedidoId } };
			var etiquetaJaGerada = PedidoJaTemEtiquetaGerada(pedido);

			if (!etiquetaJaGerada)
			{
				await EnviarAsync(client, HttpMethod.Post, $"{baseUrl}/api/v2/me/shipment/checkout", payloadPedido, cancellationToken);
				await EnviarAsync(client, HttpMethod.Post, $"{baseUrl}/api/v2/me/shipment/generate", payloadPedido, cancellationToken);

				pedido.DataEtiquetaGerada = DateTime.UtcNow;
				pedido.StatusLogistico = "generated";
			}

			await TentarAtualizarUrlEtiquetaAsync(client, baseUrl, pedido, payloadPedido, cancellationToken);

			await AtualizarDadosDePesquisaAsync(client, baseUrl, pedido, cancellationToken);
			await SalvarPedidoLogisticoAsync(pedido);

			return new GerarEtiquetaResponseDTO
			{
				PedidoId = pedido.Id.ToString(),
				MelhorEnvioPedidoId = pedido.MelhorEnvioPedidoId,
				MelhorEnvioProtocolo = pedido.MelhorEnvioProtocolo,
				CodigoRastreio = pedido.CodigoRastreio,
				UrlRastreio = pedido.UrlRastreio,
				UrlEtiqueta = pedido.UrlEtiqueta,
				StatusLogistico = pedido.StatusLogistico,
				DataEtiquetaGerada = pedido.DataEtiquetaGerada,
				Mensagem = etiquetaJaGerada
					? "Etiqueta já gerada anteriormente."
					: "Etiqueta gerada com sucesso."
			};
		}

		public async Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			if (string.IsNullOrWhiteSpace(pedido.MelhorEnvioPedidoId))
			{
				throw new InvalidOperationException("O pedido ainda não possui etiqueta gerada no Melhor Envio.");
			}

			var accessToken = ObterAccessToken();
			var baseUrl = ObterBaseUrl();
			var client = CriarClient(accessToken);

			await AtualizarDadosDePesquisaAsync(client, baseUrl, pedido, cancellationToken);
			await SalvarPedidoLogisticoAsync(pedido);

			return new SincronizarRastreioResponseDTO
			{
				PedidoId = pedido.Id.ToString(),
				MelhorEnvioPedidoId = pedido.MelhorEnvioPedidoId,
				CodigoRastreio = pedido.CodigoRastreio,
				UrlRastreio = pedido.UrlRastreio,
				StatusLogistico = pedido.StatusLogistico,
				DataPostagem = pedido.DataPostagem,
				DataEntrega = pedido.DataEntrega,
				Mensagem = "Rastreio sincronizado com sucesso."
			};
		}

		public async Task ProcessarWebhookAsync(string rawBody, string? signature, CancellationToken cancellationToken = default)
		{
			_ = cancellationToken;
			ValidarAssinatura(rawBody, signature);

			var payload = JsonSerializer.Deserialize<MelhorEnvioWebhookDTO>(rawBody, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? throw new InvalidOperationException("Payload de webhook inválido.");

			var pedido = await ObterPedidoPorEtiquetaAsync(payload.Data.Id, payload.Data.Protocol);
			if (pedido == null)
			{
				_logger.LogWarning("Webhook Melhor Envio ignorado. Pedido não encontrado para etiqueta {EtiquetaId} e protocolo {Protocolo}.", payload.Data.Id, payload.Data.Protocol);
				return;
			}

			AplicarDadosLogisticosNoPedido(pedido, payload.Data);
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

		private async Task<Pedido?> ObterPedidoPorEtiquetaAsync(string? etiquetaId, string? protocolo)
		{
			if (!string.IsNullOrWhiteSpace(etiquetaId))
			{
				var porEtiqueta = (await _unitOfWork.Pedidos.ObterPorFiltroAsync(p =>
					p.MelhorEnvioPedidoId == etiquetaId)).FirstOrDefault();
				if (porEtiqueta != null)
				{
					return porEtiqueta;
				}
			}

			if (!string.IsNullOrWhiteSpace(protocolo))
			{
				return (await _unitOfWork.Pedidos.ObterPorFiltroAsync(p =>
					p.MelhorEnvioProtocolo == protocolo)).FirstOrDefault();
			}

			return null;
		}

		private void ValidarPedidoParaEtiqueta(Pedido pedido)
		{
			if (!string.Equals(pedido.TransportadoraFrete, "Correios", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(pedido.ServicoFrete, "Frete Melhor Envio", StringComparison.OrdinalIgnoreCase) &&
				string.IsNullOrWhiteSpace(pedido.CodigoServicoFrete))
			{
				throw new InvalidOperationException("O pedido não possui um serviço do Melhor Envio vinculado.");
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

		private async Task<RemetenteMelhorEnvio> ObterRemetenteAsync()
		{
			var parametros = (await _unitOfWork.ParametrosSistema.ObterTodosAsync())
				.ToDictionary(p => p.Chave, p => p.Valor, StringComparer.OrdinalIgnoreCase);

			string Valor(string chave) => parametros.TryGetValue(chave, out var valor) ? valor?.Trim() ?? string.Empty : string.Empty;

			var remetente = new RemetenteMelhorEnvio
			{
				Nome = Valor("MelhorEnvioRemetenteNome"),
				Telefone = LimparNumero(Valor("MelhorEnvioRemetenteTelefone")),
				Email = Valor("MelhorEnvioRemetenteEmail"),
				Documento = LimparNumero(Valor("MelhorEnvioRemetenteDocumento")),
				InscricaoEstadual = Valor("MelhorEnvioRemetenteInscricaoEstadual"),
				Logradouro = Valor("MelhorEnvioRemetenteLogradouro"),
				Numero = Valor("MelhorEnvioRemetenteNumero"),
				Complemento = Valor("MelhorEnvioRemetenteComplemento"),
				Bairro = Valor("MelhorEnvioRemetenteBairro"),
				Cidade = Valor("MelhorEnvioRemetenteCidade"),
				Estado = Valor("MelhorEnvioRemetenteEstado"),
				Cep = LimparNumero(Valor("FreteCepOrigem")),
				NaoComercial = bool.TryParse(Valor("MelhorEnvioNaoComercial"), out var naoComercial) && naoComercial
			};

			if (string.IsNullOrWhiteSpace(remetente.Nome) ||
				string.IsNullOrWhiteSpace(remetente.Telefone) ||
				string.IsNullOrWhiteSpace(remetente.Email) ||
				string.IsNullOrWhiteSpace(remetente.Documento) ||
				string.IsNullOrWhiteSpace(remetente.Logradouro) ||
				string.IsNullOrWhiteSpace(remetente.Numero) ||
				string.IsNullOrWhiteSpace(remetente.Bairro) ||
				string.IsNullOrWhiteSpace(remetente.Cidade) ||
				string.IsNullOrWhiteSpace(remetente.Estado) ||
				string.IsNullOrWhiteSpace(remetente.Cep))
			{
				throw new InvalidOperationException("Parâmetros do remetente do Melhor Envio não estão completos no backoffice.");
			}

			return remetente;
		}

		private string ObterAccessToken()
		{
			return _configuration["MelhorEnvio:AccessToken"]
				?? throw new InvalidOperationException("MelhorEnvio:AccessToken não configurado.");
		}

		private string ObterBaseUrl()
		{
			return (_configuration["MelhorEnvio:BaseUrl"] ?? "https://sandbox.melhorenvio.com.br").TrimEnd('/');
		}

		private HttpClient CriarClient(string accessToken)
		{
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
			client.DefaultRequestHeaders.UserAgent.ParseAdd(_configuration["MelhorEnvio:UserAgent"] ?? "LojaVirtual ([email protected])");
			return client;
		}

		private Dictionary<string, object?> MontarPayloadCarrinho(Pedido pedido, RemetenteMelhorEnvio remetente)
		{
			var destinatarioNome = string.Equals(pedido.TipoEntrega, "Escola", StringComparison.OrdinalIgnoreCase)
				? pedido.Escola?.Nome ?? pedido.NomeEntrega
				: pedido.NomeEntrega;

			var destinatarioTelefone = LimparNumero(pedido.TelefoneEntrega);
			var destinatarioDocumento = LimparNumero(pedido.Cliente?.Cpf);

			if (string.IsNullOrWhiteSpace(destinatarioTelefone) || string.IsNullOrWhiteSpace(destinatarioDocumento))
			{
				throw new InvalidOperationException("Cliente sem telefone ou CPF cadastrado para emissão da etiqueta.");
			}

			var produtos = pedido.Itens.Select(item => new Dictionary<string, object?>
			{
				["name"] = item.Produto?.Nome ?? "Produto",
				["quantity"] = item.Quantidade,
				["unitary_value"] = decimal.Round(item.PrecoUnitario, 2)
			}).ToList();

			var peso = pedido.Itens.Sum(item =>
			{
				var pesoUnitario = item.ProdutoTamanho != null && item.ProdutoTamanho.Peso > 0
					? item.ProdutoTamanho.Peso
					: 0.1d;
				return (decimal)(pesoUnitario * item.Quantidade);
			});
			var altura = pedido.Itens.Max(item => NormalizarDimensao(item.ProdutoTamanho?.Altura));
			var largura = pedido.Itens.Max(item => NormalizarDimensao(item.ProdutoTamanho?.Largura));
			var comprimento = pedido.Itens.Sum(item => NormalizarDimensao(item.ProdutoTamanho?.Profundidade));

			var from = new Dictionary<string, object?>
			{
				["name"] = remetente.Nome,
				["phone"] = remetente.Telefone,
				["email"] = remetente.Email,
				["address"] = remetente.Logradouro,
				["complement"] = remetente.Complemento,
				["number"] = remetente.Numero,
				["district"] = remetente.Bairro,
				["city"] = remetente.Cidade,
				["postal_code"] = remetente.Cep,
				["state_abbr"] = remetente.Estado
			};

			if (remetente.Documento.Length == 14)
			{
				from["company_document"] = remetente.Documento;
				from["state_register"] = string.IsNullOrWhiteSpace(remetente.InscricaoEstadual) ? "ISENTO" : remetente.InscricaoEstadual;
			}
			else
			{
				from["document"] = remetente.Documento;
			}

			return new Dictionary<string, object?>
			{
				["from"] = from,
				["to"] = new Dictionary<string, object?>
				{
					["name"] = destinatarioNome,
					["phone"] = destinatarioTelefone,
					["email"] = pedido.Cliente?.Email ?? string.Empty,
					["document"] = destinatarioDocumento,
					["state_register"] = "ISENTO",
					["address"] = pedido.LogradouroEntrega,
					["complement"] = pedido.ComplementoEntrega,
					["number"] = pedido.NumeroEntrega,
					["district"] = pedido.BairroEntrega,
					["city"] = pedido.CidadeEntrega,
					["postal_code"] = LimparNumero(pedido.CepEntrega),
					["state_abbr"] = pedido.EstadoEntrega
				},
				["products"] = produtos,
				["volumes"] = new[]
				{
					new Dictionary<string, object?>
					{
						["height"] = altura,
						["width"] = largura,
						["length"] = Math.Max(comprimento, 1),
						["weight"] = peso > 0 ? decimal.Round(peso, 2) : 0.1m
					}
				},
				["options"] = new Dictionary<string, object?>
				{
					["receipt"] = false,
					["own_hand"] = false,
					["reverse"] = false,
					["non_commercial"] = remetente.NaoComercial,
					["insurance_value"] = decimal.Round(pedido.ValorSubtotal, 2)
				},
				["service"] = pedido.CodigoServicoFrete
			};
		}

		private async Task<JsonElement> EnviarAsync(HttpClient client, HttpMethod method, string url, object? body, CancellationToken cancellationToken)
		{
			var request = new HttpRequestMessage(method, url);
			if (body != null)
			{
				var json = JsonSerializer.Serialize(body);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
				_logger.LogInformation("Melhor Envio request {Method} {Url}: {Body}", method, url, json);
			}

			var response = await client.SendAsync(request, cancellationToken);
			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogInformation("Melhor Envio response {StatusCode} {Url}: {Body}", response.StatusCode, url, content);

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Melhor Envio retornou {(int)response.StatusCode}: {content}");
			}

			using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
			return document.RootElement.Clone();
		}

		private async Task AtualizarDadosDePesquisaAsync(HttpClient client, string baseUrl, Pedido pedido, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(pedido.MelhorEnvioPedidoId))
			{
				return;
			}

			var url = $"{baseUrl}/api/v2/me/orders/{Uri.EscapeDataString(pedido.MelhorEnvioPedidoId)}";
			var response = await client.GetAsync(url, cancellationToken);
			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogInformation("Melhor Envio pesquisa etiqueta {StatusCode}: {Body}", response.StatusCode, content);

			if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
			{
				return;
			}

			using var document = JsonDocument.Parse(content);
			var data = document.RootElement.ValueKind == JsonValueKind.Object
				? document.RootElement
				: default;

			if (data.ValueKind == JsonValueKind.Undefined || data.ValueKind == JsonValueKind.Null)
			{
				return;
			}

			_logger.LogInformation("Melhor Envio dados normalizados da pesquisa do pedido {PedidoId}: {Body}", pedido.Id, data.GetRawText());

			var tracking = ObterString(data, "tracking")
				?? ObterString(data, "tracking_code")
				?? ObterString(data, "tracking_number")
				?? ObterStringAninhado(data, "tracking", "code")
				?? ObterStringAninhado(data, "tracking", "number");

			AplicarDadosLogisticosNoPedido(pedido, new MelhorEnvioWebhookDataDTO
			{
				Id = ObterString(data, "id"),
				Protocol = ObterString(data, "protocol"),
				Status = ObterString(data, "status"),
				Tracking = tracking,
				Tracking_Url = ObterString(data, "tracking_url")
					?? ObterString(data, "trackingLink")
					?? ObterStringAninhado(data, "tracking", "url")
					?? MontarTrackingUrl(data, tracking),
				Generated_At = ObterDateTimeOffset(data, "generated_at"),
				Posted_At = ObterDateTimeOffset(data, "posted_at"),
				Delivered_At = ObterDateTimeOffset(data, "delivered_at")
			});
		}

		private async Task SalvarPedidoLogisticoAsync(Pedido pedido)
		{
			await _unitOfWork.Pedidos.AtualizarAsync(pedido);
			await _unitOfWork.SalvarMudancasAsync();
		}

		private static bool PedidoJaTemEtiquetaGerada(Pedido pedido)
		{
			return pedido.DataEtiquetaGerada.HasValue ||
				!string.IsNullOrWhiteSpace(pedido.UrlEtiqueta) ||
				string.Equals(pedido.StatusLogistico, "generated", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(pedido.StatusLogistico, "posted", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(pedido.StatusLogistico, "delivered", StringComparison.OrdinalIgnoreCase);
		}

		private async Task TentarAtualizarUrlEtiquetaAsync(
			HttpClient client,
			string baseUrl,
			Pedido pedido,
			Dictionary<string, object?> payloadPedido,
			CancellationToken cancellationToken)
		{
			if (!string.IsNullOrWhiteSpace(pedido.UrlEtiqueta))
			{
				return;
			}

			try
			{
				var respostaImpressao = await EnviarAsync(client, HttpMethod.Post, $"{baseUrl}/api/v2/me/shipment/print", payloadPedido, cancellationToken);
				pedido.UrlEtiqueta = ObterString(respostaImpressao, "url") ?? ObterString(respostaImpressao, "path");
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Não foi possível obter a URL de impressão da etiqueta do pedido {PedidoId}.", pedido.Id);
			}
		}

		private void AplicarDadosLogisticosNoPedido(Pedido pedido, MelhorEnvioWebhookDataDTO data)
		{
			pedido.MelhorEnvioPedidoId = data.Id ?? pedido.MelhorEnvioPedidoId;
			pedido.MelhorEnvioProtocolo = data.Protocol ?? pedido.MelhorEnvioProtocolo;
			pedido.StatusLogistico = data.Status ?? pedido.StatusLogistico;
			pedido.CodigoRastreio = data.Tracking ?? pedido.CodigoRastreio;
			pedido.UrlRastreio = data.Tracking_Url ?? pedido.UrlRastreio;

			if (data.Generated_At.HasValue)
			{
				pedido.DataEtiquetaGerada = data.Generated_At.Value.UtcDateTime;
			}
			if (data.Posted_At.HasValue)
			{
				pedido.DataPostagem = data.Posted_At.Value.UtcDateTime;
			}
			if (data.Delivered_At.HasValue)
			{
				pedido.DataEntrega = data.Delivered_At.Value.UtcDateTime;
			}

			var statusAnterior = pedido.Status;
			if (string.Equals(data.Status, "posted", StringComparison.OrdinalIgnoreCase))
			{
				pedido.Status = StatusPedido.Enviado;
			}
			else if (string.Equals(data.Status, "delivered", StringComparison.OrdinalIgnoreCase))
			{
				pedido.Status = StatusPedido.Entregue;
			}

			if (pedido.Status != statusAnterior)
			{
				pedido.DataAtualizacao = DateTime.UtcNow;
				_ = _notificacaoService.EnviarEmailAlteracaoStatusAsync(pedido);
			}
		}

		private void ValidarAssinatura(string rawBody, string? signature)
		{
			var secret = _configuration["MelhorEnvio:ClientSecret"];
			if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(signature))
			{
				return;
			}

			using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
			var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
			var calculada = Convert.ToBase64String(hash);

			if (!string.Equals(calculada, signature, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("Assinatura do webhook do Melhor Envio inválida.");
			}
		}

		private static string? ObterString(JsonElement element, string propertyName)
		{
			if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
			{
				return null;
			}

			return property.ToString();
		}

		private static DateTimeOffset? ObterDateTimeOffset(JsonElement element, string propertyName)
		{
			if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
			{
				return null;
			}

			if (property.ValueKind == JsonValueKind.String &&
				DateTimeOffset.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value))
			{
				return value;
			}

			return null;
		}

		private static string? ObterStringAninhado(JsonElement element, string parentPropertyName, string childPropertyName)
		{
			if (!element.TryGetProperty(parentPropertyName, out var parent) ||
				parent.ValueKind != JsonValueKind.Object ||
				!parent.TryGetProperty(childPropertyName, out var child) ||
				child.ValueKind == JsonValueKind.Null)
			{
				return null;
			}

			return child.ToString();
		}

		private static string? MontarTrackingUrl(JsonElement element, string? trackingCode)
		{
			if (string.IsNullOrWhiteSpace(trackingCode))
			{
				return null;
			}

			var trackingBaseUrl = ObterStringAninhado(element, "service", "tracking_link")
				?? ObterStringAninhadoComDoisNiveis(element, "service", "company", "tracking_link");

			if (string.IsNullOrWhiteSpace(trackingBaseUrl))
			{
				return null;
			}

			return $"{trackingBaseUrl.TrimEnd('/')}/{Uri.EscapeDataString(trackingCode)}";
		}

		private static string? ObterStringAninhadoComDoisNiveis(JsonElement element, string parentPropertyName, string middlePropertyName, string childPropertyName)
		{
			if (!element.TryGetProperty(parentPropertyName, out var parent) ||
				parent.ValueKind != JsonValueKind.Object ||
				!parent.TryGetProperty(middlePropertyName, out var middle) ||
				middle.ValueKind != JsonValueKind.Object ||
				!middle.TryGetProperty(childPropertyName, out var child) ||
				child.ValueKind == JsonValueKind.Null)
			{
				return null;
			}

			return child.ToString();
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

		private sealed class RemetenteMelhorEnvio
		{
			public string Nome { get; set; } = string.Empty;
			public string Telefone { get; set; } = string.Empty;
			public string Email { get; set; } = string.Empty;
			public string Documento { get; set; } = string.Empty;
			public string InscricaoEstadual { get; set; } = string.Empty;
			public string Logradouro { get; set; } = string.Empty;
			public string Numero { get; set; } = string.Empty;
			public string Complemento { get; set; } = string.Empty;
			public string Bairro { get; set; } = string.Empty;
			public string Cidade { get; set; } = string.Empty;
			public string Estado { get; set; } = string.Empty;
			public string Cep { get; set; } = string.Empty;
			public bool NaoComercial { get; set; }
		}
	}
}
