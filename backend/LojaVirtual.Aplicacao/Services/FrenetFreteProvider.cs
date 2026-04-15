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
	public class FrenetFreteProvider : IFreteProvider
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<FrenetFreteProvider> _logger;

		public FrenetFreteProvider(
			IUnitOfWork unitOfWork,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<FrenetFreteProvider> logger)
		{
			_unitOfWork = unitOfWork;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
		}

		public string ProviderName => FreteProviderNames.Frenet;

		public async Task<CotacaoFreteResponseDTO> CotarAsync(
			CotacaoFreteRequestDTO dto,
			FreteProviderContext context,
			CancellationToken cancellationToken = default)
		{
			var token = _configuration["Frenet:QuoteToken"];
			if (string.IsNullOrWhiteSpace(token))
			{
				throw new InvalidOperationException("QuoteToken da Frenet não configurado.");
			}

			var (itens, valorDeclarado) = await MontarItensFrenetAsync(dto, cancellationToken);
			if (itens.Count == 0)
			{
				throw new InvalidOperationException("Carrinho sem produtos válidos para cotação.");
			}

			var payload = new Dictionary<string, object?>
			{
				["SellerCEP"] = context.CepOrigem,
				["RecipientCEP"] = LimparCep(dto.CepDestino),
				["ShipmentInvoiceValue"] = decimal.Round(valorDeclarado, 2),
				["ShippingItemArray"] = itens
			};

			if (!string.IsNullOrWhiteSpace(dto.CodigoServico))
			{
				payload["ShippingServiceCode"] = dto.CodigoServico;
			}

			var client = _httpClientFactory.CreateClient();
			var baseUrl = (_configuration["Frenet:BaseUrl"] ?? "https://api.frenet.com.br").TrimEnd('/');
			var endpoint = $"{baseUrl}/shipping/quote";
			var payloadJson = JsonSerializer.Serialize(payload);
			_logger.LogInformation("Cotando frete na Frenet. Endpoint: {Endpoint}. Payload: {Payload}", endpoint, payloadJson);

			var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.TryAddWithoutValidation("token", token);
			request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

			var response = await client.SendAsync(request, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				var respostaErro = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogWarning("Falha ao cotar frete na Frenet. Status: {StatusCode}. Resposta: {Resposta}", response.StatusCode, respostaErro);
				throw new InvalidOperationException("Frenet indisponível no momento.");
			}

			var respostaSucesso = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogInformation("Resposta da Frenet recebida: {Resposta}", respostaSucesso);
			using var document = JsonDocument.Parse(respostaSucesso);
			var opcoes = ParsearOpcoesFrenet(document.RootElement);
			if (opcoes.Count == 0)
			{
				var detalhesErro = ExtrairDetalhesErroFrenet(document.RootElement);
				if (!string.IsNullOrWhiteSpace(detalhesErro))
				{
					_logger.LogWarning("Frenet retornou sem opções válidas. Detalhes: {Detalhes}", detalhesErro);
					throw new InvalidOperationException($"Frenet sem opções válidas: {detalhesErro}");
				}

				throw new InvalidOperationException("Nenhuma opção de frete disponível na Frenet.");
			}

			AplicarPrazoPreparacao(opcoes, context.PrazoPreparacaoDias);
			return new CotacaoFreteResponseDTO
			{
				FreteHabilitado = true,
				ProviderConfigurado = context.ProviderConfigurado,
				ProviderUtilizado = FreteProviderNames.Frenet,
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

		private async Task<(List<Dictionary<string, object?>> Itens, decimal ValorDeclarado)> MontarItensFrenetAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken)
		{
			_ = cancellationToken;
			var itens = new List<Dictionary<string, object?>>();
			decimal valorDeclarado = 0;

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
				valorDeclarado += (produto.PrecoPromocional ?? produto.Preco) * item.Quantidade;

				itens.Add(new Dictionary<string, object?>
				{
					["SKU"] = produto.Id.ToString(),
					["Category"] = produto.CategoriaId.ToString(),
					["Weight"] = tamanho != null && tamanho.Peso > 0 ? tamanho.Peso : 0.1,
					["Length"] = NormalizarDimensao(tamanho?.Profundidade),
					["Height"] = NormalizarDimensao(tamanho?.Altura),
					["Width"] = NormalizarDimensao(tamanho?.Largura),
					["Quantity"] = item.Quantidade
				});
			}

			return (itens, valorDeclarado);
		}

		private static void AplicarPrazoPreparacao(List<OpcaoFreteDTO> opcoes, int prazoPreparacaoDias)
		{
			foreach (var opcao in opcoes)
			{
				opcao.PrazoPreparacaoDias = prazoPreparacaoDias;
				opcao.PrazoEntregaDias = prazoPreparacaoDias + opcao.PrazoEnvioDias;
			}
		}

		private static List<OpcaoFreteDTO> ParsearOpcoesFrenet(JsonElement root)
		{
			var opcoes = new List<OpcaoFreteDTO>();
			var servicos = ExtrairArray(root, "ShippingSevicesArray", "ShippingServicesArray", "Services", "ShippingServices");
			if (servicos == null)
			{
				return opcoes;
			}

			foreach (var element in servicos.Value.EnumerateArray())
			{
				if (TemErroFrenet(element))
				{
					continue;
				}

				var valor = ObterDecimalFlexivel(element, "ShippingPrice", "Price", "ServiceValue");
				if (!valor.HasValue)
				{
					continue;
				}

				var prazoEnvio = ObterIntFlexivel(element, "DeliveryTime", "DeliveryDays", "ShippingDeliveryTime") ?? 0;

				opcoes.Add(new OpcaoFreteDTO
				{
					Provider = FreteProviderNames.Frenet,
					CodigoServico = ObterStringFlexivel(element, "ServiceCode", "ShippingServiceCode"),
					NomeServico = ObterStringFlexivel(element, "ServiceDescription", "ServiceName", "Description") ?? "Frete Frenet",
					NomeTransportadora = ObterStringFlexivel(element, "Carrier", "CarrierName"),
					Valor = valor.Value,
					PrazoEnvioDias = prazoEnvio
				});
			}

			return opcoes;
		}

		private static string? ExtrairDetalhesErroFrenet(JsonElement root)
		{
			var erros = new List<string>();
			var servicos = ExtrairArray(root, "ShippingSevicesArray", "ShippingServicesArray", "Services", "ShippingServices");

			if (servicos.HasValue)
			{
				foreach (var element in servicos.Value.EnumerateArray())
				{
					if (!TemErroFrenet(element))
					{
						continue;
					}

					var erro = ObterStringFlexivel(element, "Msg", "ErrorMessage", "Message", "Error");
					if (string.IsNullOrWhiteSpace(erro))
					{
						erro = "Serviço indisponível";
					}

					var nomeServico = ObterStringFlexivel(element, "ServiceDescription", "ServiceName", "Description") ?? "Serviço";
					var transportadora = ObterStringFlexivel(element, "Carrier", "CarrierName");
					var prefixo = string.IsNullOrWhiteSpace(transportadora) ? nomeServico : $"{transportadora} / {nomeServico}";
					erros.Add($"{prefixo}: {erro}");
				}
			}

			if (erros.Count == 0)
			{
				var mensagem = ObterStringFlexivel(root, "Message", "Error", "ErrorMessage");
				if (!string.IsNullOrWhiteSpace(mensagem))
				{
					erros.Add(mensagem);
				}
			}

			return erros.Count == 0 ? null : string.Join(" | ", erros);
		}

		private static bool TemErroFrenet(JsonElement element)
		{
			if (!element.TryGetProperty("Error", out var errorElement))
			{
				return false;
			}

			return errorElement.ValueKind switch
			{
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				JsonValueKind.String when bool.TryParse(errorElement.GetString(), out var boolValue) => boolValue,
				JsonValueKind.Number when errorElement.TryGetInt32(out var numberValue) => numberValue != 0,
				_ => !string.IsNullOrWhiteSpace(errorElement.ToString()) &&
				     !string.Equals(errorElement.ToString(), "false", StringComparison.OrdinalIgnoreCase)
			};
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

		private static string? ObterString(JsonElement element, string propertyName)
		{
			if (!element.TryGetProperty(propertyName, out var property))
			{
				return null;
			}

			return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
		}

		private static JsonElement? ExtrairArray(JsonElement element, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
				{
					return property;
				}
			}

			return null;
		}

		private static decimal? ObterDecimalFlexivel(JsonElement element, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				var valor = ObterDecimal(element, propertyName);
				if (valor.HasValue)
				{
					return valor;
				}
			}

			return null;
		}

		private static int? ObterIntFlexivel(JsonElement element, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				var valor = ObterInt(element, propertyName);
				if (valor.HasValue)
				{
					return valor;
				}
			}

			return null;
		}

		private static string? ObterStringFlexivel(JsonElement element, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				var valor = ObterString(element, propertyName);
				if (!string.IsNullOrWhiteSpace(valor))
				{
					return valor;
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
