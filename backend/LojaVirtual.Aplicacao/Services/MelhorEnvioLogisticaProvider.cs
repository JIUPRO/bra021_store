using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LojaVirtual.Aplicacao.Services
{
	public class MelhorEnvioLogisticaProvider : IMelhorEnvioLogisticaProvider
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly INotificacaoService _notificacaoService;
		private readonly ILogger<MelhorEnvioLogisticaProvider> _logger;

		public MelhorEnvioLogisticaProvider(
			IUnitOfWork unitOfWork,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			INotificacaoService notificacaoService,
			ILogger<MelhorEnvioLogisticaProvider> logger)
		{
			_unitOfWork = unitOfWork;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_notificacaoService = notificacaoService;
			_logger = logger;
			QuestPDF.Settings.License = LicenseType.Community;
		}

		public string ProviderName => FreteProviderNames.MelhorEnvio;

		public async Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			ValidarPedidoParaEtiqueta(pedido);
			pedido.ProviderLogisticaUtilizado ??= FreteProviderNames.MelhorEnvio;
			pedido.ProviderFreteUtilizado ??= FreteProviderNames.MelhorEnvio;

			var serviceId = ObterServiceId(pedido.CodigoServicoFrete);
			var remetente = await ObterRemetenteAsync();
			var naoComercial = await ObterNaoComercialAsync();
			var client = CriarClient();
			var baseUrl = ObterBaseUrl();

			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				var cartPayload = MontarPayloadCarrinho(pedido, remetente, serviceId, naoComercial);
				var cartResponse = await EnviarJsonAsync(client, HttpMethod.Post, $"{baseUrl}/api/v2/me/cart", cartPayload, cancellationToken);
				pedido.IntegracaoFretePedidoId = ExtrairString(cartResponse, "id", "order_id", "order");
				pedido.IntegracaoFreteProtocolo = ExtrairString(cartResponse, "protocol", "protocolo", "order");
			}

			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				throw new InvalidOperationException("O Melhor Envio não retornou o identificador da etiqueta no carrinho.");
			}

			await EnviarJsonAsync(
				client,
				HttpMethod.Post,
				$"{baseUrl}/api/v2/me/shipment/checkout",
				new Dictionary<string, object?> { ["orders"] = new[] { pedido.IntegracaoFretePedidoId } },
				cancellationToken);

			await EnviarJsonAsync(
				client,
				HttpMethod.Post,
				$"{baseUrl}/api/v2/me/shipment/generate",
				new Dictionary<string, object?> { ["orders"] = new[] { pedido.IntegracaoFretePedidoId } },
				cancellationToken);

			pedido.DataEtiquetaGerada ??= DateTime.UtcNow;
			await AtualizarDadosPedidoAsync(client, baseUrl, pedido, cancellationToken);
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
				Mensagem = "Etiqueta do Melhor Envio gerada. Use o botão de impressão do sistema para baixar o PDF."
			};
		}

		public async Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			pedido.ProviderLogisticaUtilizado ??= FreteProviderNames.MelhorEnvio;
			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				throw new InvalidOperationException("O pedido ainda não possui envio gerado no Melhor Envio.");
			}

			var client = CriarClient();
			var baseUrl = ObterBaseUrl();
			await AtualizarDadosPedidoAsync(client, baseUrl, pedido, cancellationToken);
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
				Mensagem = MontarMensagemSincronizacao(pedido)
			};
		}

		public async Task<ArquivoEtiquetaDTO> ObterArquivoEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var pedido = await ObterPedidoLogisticoAsync(pedidoId);
			if (string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				throw new InvalidOperationException("O pedido ainda não possui etiqueta gerada no Melhor Envio.");
			}

			var client = CriarClient();
			var baseUrl = ObterBaseUrl();
			var orderId = Uri.EscapeDataString(pedido.IntegracaoFretePedidoId);
			var envio = await EnviarJsonAsync(
				client,
				HttpMethod.Get,
				$"{baseUrl}/api/v2/me/orders/{orderId}",
				null,
				cancellationToken);

			var fullPdfUrl = ExtrairUrlArquivoCompleto(envio);
			if (!string.IsNullOrWhiteSpace(fullPdfUrl))
			{
				_logger.LogInformation("Tentando baixar arquivo completo de etiqueta do Melhor Envio: {Url}", fullPdfUrl);
				using var pdfResponse = await _httpClientFactory.CreateClient().GetAsync(fullPdfUrl, cancellationToken);
				var pdfContent = await pdfResponse.Content.ReadAsByteArrayAsync(cancellationToken);
				if (pdfResponse.IsSuccessStatusCode)
				{
					return new ArquivoEtiquetaDTO
					{
						Conteudo = pdfContent,
						ContentType = pdfResponse.Content.Headers.ContentType?.MediaType ?? "application/pdf",
						NomeArquivo = $"etiqueta-{pedido.NumeroPedido}.pdf"
					};
				}

				_logger.LogWarning(
					"Não foi possível baixar o arquivo completo de etiqueta do Melhor Envio pela URL {Url}. Status {StatusCode}.",
					fullPdfUrl,
					(int)pdfResponse.StatusCode);
			}

			var etiquetaJpegUrl = ExtrairUrlArquivo(envio, "1", "jpeg");
			var daceJpegUrl = ExtrairUrlArquivo(envio, "dace", "jpeg");
			if (!string.IsNullOrWhiteSpace(etiquetaJpegUrl) || !string.IsNullOrWhiteSpace(daceJpegUrl))
			{
				var etiquetaJpeg = await TentarBaixarArquivoAsync(etiquetaJpegUrl, cancellationToken);
				var daceJpeg = await TentarBaixarArquivoAsync(daceJpegUrl, cancellationToken);

				if (etiquetaJpeg != null || daceJpeg != null)
				{
					var pdfCombinado = GerarPdfCombinadoEtiqueta(etiquetaJpeg, daceJpeg);
					return new ArquivoEtiquetaDTO
					{
						Conteudo = pdfCombinado,
						ContentType = "application/pdf",
						NomeArquivo = $"etiqueta-{pedido.NumeroPedido}.pdf"
					};
				}
			}

			var pdfUrls = ExtrairUrlsArquivoEtiqueta(envio);
			foreach (var pdfUrl in pdfUrls)
			{
				_logger.LogInformation("Tentando baixar arquivo de etiqueta do Melhor Envio: {Url}", pdfUrl);
				using var pdfResponse = await _httpClientFactory.CreateClient().GetAsync(pdfUrl, cancellationToken);
				var pdfContent = await pdfResponse.Content.ReadAsByteArrayAsync(cancellationToken);
				if (pdfResponse.IsSuccessStatusCode)
				{
					return new ArquivoEtiquetaDTO
					{
						Conteudo = pdfContent,
						ContentType = pdfResponse.Content.Headers.ContentType?.MediaType ?? "application/pdf",
						NomeArquivo = $"etiqueta-{pedido.NumeroPedido}.pdf"
					};
				}

				_logger.LogWarning(
					"Não foi possível baixar o arquivo de etiqueta do Melhor Envio pela URL {Url}. Status {StatusCode}.",
					pdfUrl,
					(int)pdfResponse.StatusCode);
			}

			var response = await client.GetAsync($"{baseUrl}/api/v2/me/imprimir/pdf/{orderId}", cancellationToken);
			var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var erro = Encoding.UTF8.GetString(content);
				throw new InvalidOperationException($"Melhor Envio retornou {(int)response.StatusCode} ao imprimir etiqueta: {erro}");
			}

			var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
			if (string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase) ||
				PareceJson(content))
			{
				var pdfFallbackUrl = ExtrairUrlPdfMelhorEnvio(content);
				if (string.IsNullOrWhiteSpace(pdfFallbackUrl))
				{
					throw new InvalidOperationException("O Melhor Envio não retornou a URL do PDF da etiqueta.");
				}

				using var pdfResponse = await _httpClientFactory.CreateClient().GetAsync(pdfFallbackUrl, cancellationToken);
				var pdfContent = await pdfResponse.Content.ReadAsByteArrayAsync(cancellationToken);
				if (!pdfResponse.IsSuccessStatusCode)
				{
					var erroPdf = Encoding.UTF8.GetString(pdfContent);
					throw new InvalidOperationException($"Não foi possível baixar o PDF da etiqueta do Melhor Envio: {erroPdf}");
				}

				content = pdfContent;
				mediaType = pdfResponse.Content.Headers.ContentType?.MediaType ?? "application/pdf";
			}

			return new ArquivoEtiquetaDTO
			{
				Conteudo = content,
				ContentType = mediaType,
				NomeArquivo = $"etiqueta-{pedido.NumeroPedido}.pdf"
			};
		}

		public async Task ProcessarWebhookAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default)
		{
			_ = cancellationToken;
			ValidarWebhook(headers);

			var payload = JsonSerializer.Deserialize<MelhorEnvioWebhookDTO>(rawBody, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? throw new InvalidOperationException("Payload de webhook do Melhor Envio inválido.");

			var pedido = await ObterPedidoPorIntegracaoAsync(payload.OrderId, payload.Protocol, payload.Id);
			if (pedido == null)
			{
				_logger.LogWarning("Webhook do Melhor Envio ignorado. Pedido não encontrado para OrderId={OrderId}, Protocol={Protocol}, Id={Id}.", payload.OrderId, payload.Protocol, payload.Id);
				return;
			}

			pedido.ProviderLogisticaUtilizado ??= FreteProviderNames.MelhorEnvio;
			pedido.IntegracaoFretePedidoId ??= payload.OrderId ?? payload.Id;
			pedido.IntegracaoFreteProtocolo ??= payload.Protocol;
			pedido.CodigoRastreio = payload.Tracking ?? pedido.CodigoRastreio;
			pedido.UrlRastreio = payload.TrackingUrl ?? pedido.UrlRastreio;
			pedido.StatusLogistico = payload.Status ?? pedido.StatusLogistico;
			pedido.DataAtualizacao = DateTime.UtcNow;

			await SalvarPedidoLogisticoAsync(pedido);
		}

		private HttpClient CriarClient()
		{
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterAccessToken());
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", ObterUserAgent());
			return client;
		}

		private string ObterBaseUrl()
		{
			return (_configuration["MelhorEnvio:BaseUrl"] ?? "https://sandbox.melhorenvio.com.br").TrimEnd('/');
		}

		private string ObterAccessToken()
		{
			return _configuration["MelhorEnvio:AccessToken"]
				?? throw new InvalidOperationException("MelhorEnvio:AccessToken não configurado.");
		}

		private string ObterUserAgent()
		{
			return _configuration["MelhorEnvio:UserAgent"]
				?? throw new InvalidOperationException("MelhorEnvio:UserAgent não configurado.");
		}

		private async Task<bool> ObterNaoComercialAsync()
		{
			var parametro = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("LogisticaNaoComercial");
			return bool.TryParse(parametro?.Valor, out var valor) && valor;
		}

		private void ValidarWebhook(IReadOnlyDictionary<string, string?> headers)
		{
			var tokenName = _configuration["MelhorEnvio:WebhookTokenName"];
			var tokenValue = _configuration["MelhorEnvio:WebhookTokenValue"];
			if (string.IsNullOrWhiteSpace(tokenName) || string.IsNullOrWhiteSpace(tokenValue))
			{
				return;
			}

			headers.TryGetValue(tokenName, out var recebido);
			if (!string.Equals(recebido, tokenValue, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("Token do webhook do Melhor Envio inválido.");
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

		private async Task<Pedido?> ObterPedidoPorIntegracaoAsync(params string?[] chaves)
		{
			var valores = chaves
				.Where(v => !string.IsNullOrWhiteSpace(v))
				.Select(v => v!.Trim())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			if (valores.Count == 0)
			{
				return null;
			}

			var pedidos = await _unitOfWork.Pedidos.ObterPorFiltroAsync(p =>
				(p.IntegracaoFretePedidoId != null && valores.Contains(p.IntegracaoFretePedidoId)) ||
				(p.IntegracaoFreteProtocolo != null && valores.Contains(p.IntegracaoFreteProtocolo)) ||
				valores.Contains(p.NumeroPedido));

			return pedidos.FirstOrDefault();
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

		private static int ObterServiceId(string? codigoServico)
		{
			if (!int.TryParse(codigoServico, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serviceId))
			{
				throw new InvalidOperationException("O serviço selecionado do Melhor Envio não possui um ID válido.");
			}

			return serviceId;
		}

		private async Task<RemetenteMelhorEnvioDTO> ObterRemetenteAsync()
		{
			var remetente = new RemetenteMelhorEnvioDTO
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
				throw new InvalidOperationException("Os dados do remetente do Melhor Envio estão incompletos nos parâmetros do sistema.");
			}

			return remetente;
		}

		private Dictionary<string, object?> MontarPayloadCarrinho(Pedido pedido, RemetenteMelhorEnvioDTO remetente, int serviceId, bool naoComercial)
		{
			var documentoCliente = LimparNumero(pedido.Cliente?.Cpf);
			var destinatarioNome = string.Equals(pedido.TipoEntrega, "Escola", StringComparison.OrdinalIgnoreCase)
				? pedido.Escola?.Nome ?? pedido.NomeEntrega
				: pedido.NomeEntrega;

			return new Dictionary<string, object?>
			{
				["from"] = MontarPessoa(remetente.Nome!, remetente.Telefone!, remetente.Email!, remetente.Documento!, remetente.InscricaoEstadual, remetente.Logradouro!, remetente.Complemento, remetente.Numero!, remetente.Bairro!, remetente.Cidade!, remetente.Cep!, remetente.Estado!),
				["to"] = MontarPessoa(destinatarioNome ?? string.Empty, LimparNumero(pedido.TelefoneEntrega), pedido.Cliente?.Email ?? string.Empty, documentoCliente, null, pedido.LogradouroEntrega ?? string.Empty, pedido.ComplementoEntrega, pedido.NumeroEntrega ?? string.Empty, pedido.BairroEntrega ?? string.Empty, pedido.CidadeEntrega ?? string.Empty, LimparNumero(pedido.CepEntrega), pedido.EstadoEntrega ?? string.Empty),
				["products"] = pedido.Itens.Select(item => new Dictionary<string, object?>
				{
					["id"] = item.ProdutoId.ToString(),
					["name"] = item.Produto?.Nome ?? "Produto",
					["quantity"] = item.Quantidade.ToString(CultureInfo.InvariantCulture),
					["unitary_value"] = decimal.Round(item.PrecoUnitario, 2).ToString("0.00", CultureInfo.InvariantCulture)
				}).ToList(),
				["volumes"] = new List<Dictionary<string, object?>>
				{
					new()
					{
						["height"] = pedido.Itens.Max(item => NormalizarDimensao(item.ProdutoTamanho?.Altura)),
						["width"] = pedido.Itens.Max(item => NormalizarDimensao(item.ProdutoTamanho?.Largura)),
						["length"] = Math.Max(1, pedido.Itens.Sum(item => NormalizarDimensao(item.ProdutoTamanho?.Profundidade))),
						["weight"] = decimal.Round(pedido.Itens.Sum(item =>
						{
							var pesoUnitario = item.ProdutoTamanho != null && item.ProdutoTamanho.Peso > 0
								? item.ProdutoTamanho.Peso
								: 0.1d;
							return (decimal)(pesoUnitario * item.Quantidade);
						}), 3)
					}
				},
				["options"] = new Dictionary<string, object?>
				{
					["receipt"] = false,
					["own_hand"] = false,
					["reverse"] = false,
					["non_commercial"] = naoComercial,
					["insurance_value"] = decimal.Round(pedido.ValorSubtotal, 2)
				},
				["service"] = serviceId
			};
		}

		private static Dictionary<string, object?> MontarPessoa(
			string nome,
			string telefone,
			string email,
			string documento,
			string? inscricaoEstadual,
			string logradouro,
			string? complemento,
			string numero,
			string bairro,
			string cidade,
			string cep,
			string estado)
		{
			var pessoa = new Dictionary<string, object?>
			{
				["name"] = nome,
				["phone"] = telefone,
				["email"] = email,
				["state_register"] = inscricaoEstadual,
				["address"] = logradouro,
				["complement"] = complemento,
				["number"] = numero,
				["district"] = bairro,
				["city"] = cidade,
				["postal_code"] = cep,
				["state_abbr"] = estado
			};

			if (documento.Length == 14)
			{
				pessoa["company_document"] = documento;
			}
			else if (!string.IsNullOrWhiteSpace(documento))
			{
				pessoa["document"] = documento;
			}

			return pessoa;
		}

		private async Task AtualizarDadosPedidoAsync(HttpClient client, string baseUrl, Pedido pedido, CancellationToken cancellationToken)
		{
			var envio = await EnviarJsonAsync(
				client,
				HttpMethod.Get,
				$"{baseUrl}/api/v2/me/orders/{Uri.EscapeDataString(pedido.IntegracaoFretePedidoId!)}",
				null,
				cancellationToken);

			pedido.IntegracaoFretePedidoId = ExtrairString(envio, "id", "order_id", "order") ?? pedido.IntegracaoFretePedidoId;
			pedido.IntegracaoFreteProtocolo = ExtrairString(envio, "protocol", "protocolo") ?? pedido.IntegracaoFreteProtocolo;
			pedido.CodigoRastreio = ExtrairString(envio, "tracking", "tracking_code") ?? pedido.CodigoRastreio;
			pedido.UrlRastreio = MontarUrlRastreio(envio, pedido.CodigoRastreio) ?? pedido.UrlRastreio;
			pedido.StatusLogistico = ExtrairString(envio, "status", "description") ?? pedido.StatusLogistico;
			pedido.DataPostagem ??= ExtrairDateTime(envio, "posted_at", "postedAt");
			pedido.DataEntrega ??= ExtrairDateTime(envio, "delivered_at", "deliveredAt");
			pedido.DataEtiquetaGerada ??= ExtrairDateTime(envio, "generated_at", "generatedAt");

			if (pedido.DataEntrega.HasValue && pedido.Status < StatusPedido.Entregue)
			{
				pedido.Status = StatusPedido.Entregue;
			}
			else if (pedido.DataPostagem.HasValue && pedido.Status < StatusPedido.Enviado)
			{
				pedido.Status = StatusPedido.Enviado;
			}
		}

		private async Task<JsonElement> EnviarJsonAsync(HttpClient client, HttpMethod method, string url, object? body, CancellationToken cancellationToken)
		{
			var request = new HttpRequestMessage(method, url);
			if (body != null)
			{
				var json = JsonSerializer.Serialize(body);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
				_logger.LogInformation("Melhor Envio request {Method} {Url}: {Body}", method, url, json);
			}
			else
			{
				_logger.LogInformation("Melhor Envio request {Method} {Url}", method, url);
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

		private static string? ExtrairString(JsonElement element, params string[] propertyNames)
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
						var nested = ExtrairString(property.Value, propertyNames);
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
					var nested = ExtrairString(item, propertyNames);
					if (!string.IsNullOrWhiteSpace(nested))
					{
						return nested;
					}
				}
			}

			return null;
		}

		private static DateTime? ExtrairDateTime(JsonElement element, params string[] propertyNames)
		{
			var valor = ExtrairString(element, propertyNames);
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

		private static string LimparNumero(string? valor)
		{
			return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
		}

		private static int NormalizarDimensao(double? valor)
		{
			if (!valor.HasValue || valor.Value <= 0)
			{
				return 1;
			}

			return Math.Max(1, (int)Math.Ceiling(valor.Value));
		}

		private static string MontarMensagemSincronizacao(Pedido pedido)
		{
			if (!string.IsNullOrWhiteSpace(pedido.CodigoRastreio) || !string.IsNullOrWhiteSpace(pedido.UrlRastreio))
			{
				return "Rastreio sincronizado com sucesso.";
			}

			if (!string.IsNullOrWhiteSpace(pedido.IntegracaoFretePedidoId))
			{
				return $"Envio {pedido.IntegracaoFretePedidoId} localizado no Melhor Envio, mas o rastreio ainda não foi disponibilizado pela API.";
			}

			return "Rastreio sincronizado com sucesso.";
		}

		private static string? MontarUrlRastreio(JsonElement envio, string? codigoRastreio)
		{
			var urlDireta = ExtrairString(envio, "tracking_url", "trackingUrl");
			if (!string.IsNullOrWhiteSpace(urlDireta))
			{
				return urlDireta;
			}

			if (string.IsNullOrWhiteSpace(codigoRastreio))
			{
				return null;
			}

			var trackingLinkBase = ExtrairString(envio, "tracking_link");
			if (string.IsNullOrWhiteSpace(trackingLinkBase))
			{
				return null;
			}

			return $"{trackingLinkBase.TrimEnd('/')}/{Uri.EscapeDataString(codigoRastreio)}";
		}

		private static List<string> ExtrairUrlsArquivoEtiqueta(JsonElement envio)
		{
			var urls = new List<string>();

			if (envio.ValueKind != JsonValueKind.Object ||
				!envio.TryGetProperty("files", out var files) ||
				files.ValueKind != JsonValueKind.Object)
			{
				return urls;
			}

			if (files.TryGetProperty("dace", out var dace) && dace.ValueKind == JsonValueKind.Object)
			{
				AdicionarUrlSeValida(urls, ExtrairPropriedade(dace, "fullPdf"));
				AdicionarUrlSeValida(urls, ExtrairPropriedade(dace, "pdf"));
			}

			foreach (var file in files.EnumerateObject())
			{
				if (file.Value.ValueKind != JsonValueKind.Object)
				{
					continue;
				}

				AdicionarUrlSeValida(urls, ExtrairPropriedade(file.Value, "pdf"));
			}

			return urls;
		}

		private static string? ExtrairUrlArquivoCompleto(JsonElement envio)
		{
			return ExtrairUrlArquivo(envio, "dace", "fullPdf");
		}

		private static string? ExtrairUrlArquivo(JsonElement envio, string grupo, string formato)
		{
			if (envio.ValueKind != JsonValueKind.Object ||
				!envio.TryGetProperty("files", out var files) ||
				files.ValueKind != JsonValueKind.Object ||
				!files.TryGetProperty(grupo, out var fileGroup) ||
				fileGroup.ValueKind != JsonValueKind.Object)
			{
				return null;
			}

			return ExtrairPropriedade(fileGroup, formato);
		}

		private static void AdicionarUrlSeValida(List<string> urls, string? url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return;
			}

			if (!urls.Contains(url, StringComparer.OrdinalIgnoreCase))
			{
				urls.Add(url);
			}
		}

		private async Task<byte[]?> TentarBaixarArquivoAsync(string? url, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return null;
			}

			_logger.LogInformation("Tentando baixar arquivo auxiliar do Melhor Envio: {Url}", url);
			using var response = await _httpClientFactory.CreateClient().GetAsync(url, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning(
					"Não foi possível baixar o arquivo auxiliar do Melhor Envio pela URL {Url}. Status {StatusCode}.",
					url,
					(int)response.StatusCode);
				return null;
			}

			return await response.Content.ReadAsByteArrayAsync(cancellationToken);
		}

		private static byte[] GerarPdfCombinadoEtiqueta(byte[]? etiquetaJpeg, byte[]? daceJpeg)
		{
			return Document.Create(container =>
			{
				if (etiquetaJpeg != null)
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(20);
						page.Content().AlignCenter().AlignMiddle().Width(220).Image(etiquetaJpeg, ImageScaling.FitArea);
					});
				}

				if (daceJpeg != null)
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(20);
						page.Content().AlignCenter().AlignMiddle().Image(daceJpeg, ImageScaling.FitArea);
					});
				}
			}).GeneratePdf();
		}

		private static string? ExtrairPropriedade(JsonElement element, string propertyName)
		{
			if (element.ValueKind == JsonValueKind.Object &&
				element.TryGetProperty(propertyName, out var property) &&
				property.ValueKind != JsonValueKind.Null)
			{
				var value = property.GetString();
				return string.IsNullOrWhiteSpace(value) ? null : value;
			}

			return null;
		}

		private static bool PareceJson(byte[] content)
		{
			if (content.Length == 0)
			{
				return false;
			}

			var texto = Encoding.UTF8.GetString(content).TrimStart();
			return texto.StartsWith("{", StringComparison.Ordinal) || texto.StartsWith("[", StringComparison.Ordinal);
		}

		private static string? ExtrairUrlPdfMelhorEnvio(byte[] content)
		{
			using var document = JsonDocument.Parse(content);
			var root = document.RootElement;

			if (root.ValueKind == JsonValueKind.Array)
			{
				foreach (var item in root.EnumerateArray())
				{
					if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
					{
						return item.GetString();
					}

					var nested = ExtrairString(item, "url", "link", "pdf", "pdf_url");
					if (!string.IsNullOrWhiteSpace(nested))
					{
						return nested;
					}
				}
			}

			return ExtrairString(root, "url", "link", "pdf", "pdf_url");
		}

		private sealed class RemetenteMelhorEnvioDTO
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
	}
}
