using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LojaVirtual.Aplicacao.Services
{
	public class MelhorEnvioFreteProvider : IFreteProvider
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<MelhorEnvioFreteProvider> _logger;

		public MelhorEnvioFreteProvider(
			IUnitOfWork unitOfWork,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<MelhorEnvioFreteProvider> logger)
		{
			_unitOfWork = unitOfWork;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
		}

		public string ProviderName => FreteProviderNames.MelhorEnvio;

		public async Task<CotacaoFreteResponseDTO> CotarAsync(
			CotacaoFreteRequestDTO dto,
			FreteProviderContext context,
			CancellationToken cancellationToken = default)
		{
			var client = CriarClient();
			var baseUrl = (_configuration["MelhorEnvio:BaseUrl"] ?? "https://sandbox.melhorenvio.com.br").TrimEnd('/');
			var payload = new Dictionary<string, object?>
			{
				["from"] = new Dictionary<string, object?>
				{
					["postal_code"] = context.CepOrigem
				},
				["to"] = new Dictionary<string, object?>
				{
					["postal_code"] = LimparCep(dto.CepDestino)
				},
				["products"] = await MontarProdutosAsync(dto)
			};

			var endpoint = $"{baseUrl}/api/v2/me/shipment/calculate";
			var payloadJson = JsonSerializer.Serialize(payload);
			_logger.LogInformation("Cotando frete no Melhor Envio. Endpoint: {Endpoint}. Payload: {Payload}", endpoint, payloadJson);

			var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
			{
				Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request, cancellationToken);
			var respostaTexto = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogInformation("Resposta do Melhor Envio recebida: {Resposta}", respostaTexto);

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Melhor Envio retornou {(int)response.StatusCode}: {respostaTexto}");
			}

			using var document = JsonDocument.Parse(respostaTexto);
			var opcoes = ParsearOpcoes(document.RootElement);
			if (opcoes.Count == 0)
			{
				throw new InvalidOperationException("Nenhuma opção de frete disponível no Melhor Envio.");
			}

			AplicarPrazoPreparacao(opcoes, context.PrazoPreparacaoDias);
			return new CotacaoFreteResponseDTO
			{
				FreteHabilitado = true,
				ProviderConfigurado = context.ProviderConfigurado,
				ProviderUtilizado = FreteProviderNames.MelhorEnvio,
				CepOrigem = context.CepOrigem,
				CepDestino = LimparCep(dto.CepDestino),
				PrazoPreparacaoDias = context.PrazoPreparacaoDias,
				UsandoFallbackFixo = false,
				Opcoes = opcoes
					.OrderBy(o => o.Valor)
					.ThenBy(o => o.PrazoEntregaDias)
					.ToList()
			};
		}

		private HttpClient CriarClient()
		{
			var token = _configuration["MelhorEnvio:AccessToken"]
				?? throw new InvalidOperationException("MelhorEnvio:AccessToken não configurado.");
			var userAgent = _configuration["MelhorEnvio:UserAgent"]
				?? throw new InvalidOperationException("MelhorEnvio:UserAgent não configurado.");

			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
			return client;
		}

		private async Task<List<Dictionary<string, object?>>> MontarProdutosAsync(CotacaoFreteRequestDTO dto)
		{
			var produtos = new List<Dictionary<string, object?>>();

			foreach (var item in dto.Itens)
			{
				var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
				if (produto == null)
				{
					continue;
				}

				var tamanho = item.ProdutoTamanhoId.HasValue
					? await _unitOfWork.ProdutoTamanhos.ObterPorIdAsync(item.ProdutoTamanhoId.Value)
					: null;

				var valorSeguro = (produto.PrecoPromocional ?? produto.Preco) * item.Quantidade;
				produtos.Add(new Dictionary<string, object?>
				{
					["id"] = produto.Id.ToString(),
					["width"] = NormalizarDimensao(tamanho?.Largura),
					["height"] = NormalizarDimensao(tamanho?.Altura),
					["length"] = NormalizarDimensao(tamanho?.Profundidade),
					["weight"] = tamanho != null && tamanho.Peso > 0 ? tamanho.Peso : 0.1,
					["insurance_value"] = decimal.Round(valorSeguro, 2),
					["quantity"] = item.Quantidade
				});
			}

			return produtos;
		}

		private static List<OpcaoFreteDTO> ParsearOpcoes(JsonElement root)
		{
			var opcoes = new List<OpcaoFreteDTO>();
			if (root.ValueKind != JsonValueKind.Array)
			{
				return opcoes;
			}

			foreach (var item in root.EnumerateArray())
			{
				var error = ObterString(item, "error");
				if (!string.IsNullOrWhiteSpace(error))
				{
					continue;
				}

				var price = ObterDecimal(item, "custom_price") ?? ObterDecimal(item, "price");
				var serviceId = ObterString(item, "id");
				if (!price.HasValue || string.IsNullOrWhiteSpace(serviceId))
				{
					continue;
				}

				opcoes.Add(new OpcaoFreteDTO
				{
					Provider = FreteProviderNames.MelhorEnvio,
					CodigoServico = serviceId,
					NomeServico = ObterString(item, "name") ?? "Frete Melhor Envio",
					NomeTransportadora = ObterString(item, "company", "name"),
					Valor = price.Value,
					PrazoEnvioDias = ObterInt(item, "custom_delivery_time") ?? ObterInt(item, "delivery_time") ?? 0
				});
			}

			return opcoes;
		}

		private static void AplicarPrazoPreparacao(List<OpcaoFreteDTO> opcoes, int prazoPreparacaoDias)
		{
			foreach (var opcao in opcoes)
			{
				opcao.PrazoPreparacaoDias = prazoPreparacaoDias;
				opcao.PrazoEntregaDias = prazoPreparacaoDias + opcao.PrazoEnvioDias;
			}
		}

		private static decimal? ObterDecimal(JsonElement element, string propertyName)
		{
			if (!element.TryGetProperty(propertyName, out var property))
			{
				return null;
			}

			return property.ValueKind switch
			{
				JsonValueKind.Number when property.TryGetDecimal(out var numberValue) => numberValue,
				JsonValueKind.String when decimal.TryParse(property.GetString(), CultureInfo.InvariantCulture, out var stringValue) => stringValue,
				_ => null
			};
		}

		private static int? ObterInt(JsonElement element, string propertyName)
		{
			if (!element.TryGetProperty(propertyName, out var property))
			{
				return null;
			}

			return property.ValueKind switch
			{
				JsonValueKind.Number when property.TryGetInt32(out var numberValue) => numberValue,
				JsonValueKind.String when int.TryParse(property.GetString(), CultureInfo.InvariantCulture, out var stringValue) => stringValue,
				_ => null
			};
		}

		private static string? ObterString(JsonElement element, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				if (element.TryGetProperty(propertyName, out var property))
				{
					if (property.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(property.GetString()))
					{
						return property.GetString();
					}

					if (property.ValueKind == JsonValueKind.Number)
					{
						return property.ToString();
					}

					if (property.ValueKind == JsonValueKind.Object)
					{
						var nested = ObterString(property, "name", "label", "description");
						if (!string.IsNullOrWhiteSpace(nested))
						{
							return nested;
						}
					}
				}
			}

			return null;
		}

		private static double NormalizarDimensao(double? valor)
		{
			return valor.HasValue && valor.Value > 0 ? valor.Value : 1;
		}

		private static string LimparCep(string? cep)
		{
			return new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
		}
	}
}
