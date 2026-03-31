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
	public interface IFreteService
	{
		Task<CotacaoFreteResponseDTO> CotarAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken = default);
		OpcaoFreteDTO SelecionarOpcao(CotacaoFreteResponseDTO cotacao, string? codigoServicoPreferido);
	}

	public class FreteService : IFreteService
	{
		private const string ProviderFixo = "Fixo";
		private const string ProviderMelhorEnvio = "MelhorEnvio";
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<FreteService> _logger;

		public FreteService(
			IUnitOfWork unitOfWork,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<FreteService> logger)
		{
			_unitOfWork = unitOfWork;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<CotacaoFreteResponseDTO> CotarAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken = default)
		{
			var configuracao = await ObterConfiguracaoAsync();
			var cepDestino = LimparCep(dto.CepDestino);

			if (!configuracao.FreteHabilitado || !string.Equals(configuracao.Provider, ProviderMelhorEnvio, StringComparison.OrdinalIgnoreCase))
			{
				return await CriarRespostaFixaAsync(
					dto,
					configuracao,
					!configuracao.FreteHabilitado ? "Frete dinâmico desabilitado. Usando valor fixo do produto." : null,
					cancellationToken);
			}

			if (string.IsNullOrWhiteSpace(configuracao.CepOrigem) || cepDestino.Length != 8)
			{
				return await CriarRespostaFixaAsync(
					dto,
					configuracao,
					"CEP de origem ou destino inválido para cotação dinâmica. Usando valor fixo do produto.",
					cancellationToken);
			}

			var accessToken = _configuration["MelhorEnvio:AccessToken"];
			if (string.IsNullOrWhiteSpace(accessToken))
			{
				return await CriarRespostaFixaAsync(
					dto,
					configuracao,
					"Token do Melhor Envio não configurado. Usando valor fixo do produto.",
					cancellationToken);
			}

			try
			{
				var produtosCotacao = await MontarProdutosMelhorEnvioAsync(dto, cancellationToken);
				if (produtosCotacao.Count == 0)
				{
					return await CriarRespostaFixaAsync(
						dto,
						configuracao,
						"Carrinho sem produtos válidos para cotação. Usando valor fixo do produto.",
						cancellationToken);
				}

				var payload = new Dictionary<string, object?>
				{
					["from"] = new Dictionary<string, object?> { ["postal_code"] = configuracao.CepOrigem },
					["to"] = new Dictionary<string, object?> { ["postal_code"] = cepDestino },
					["products"] = produtosCotacao
				};

				if (!string.IsNullOrWhiteSpace(dto.CodigoServico))
				{
					payload["services"] = dto.CodigoServico;
				}

				var client = _httpClientFactory.CreateClient();
				var baseUrl = (_configuration["MelhorEnvio:BaseUrl"] ?? "https://sandbox.melhorenvio.com.br").TrimEnd('/');
				var endpoint = $"{baseUrl}/api/v2/me/shipment/calculate";
				var payloadJson = JsonSerializer.Serialize(payload);
				_logger.LogInformation("Cotando frete no Melhor Envio. Endpoint: {Endpoint}. Payload: {Payload}", endpoint, payloadJson);
				var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				request.Headers.UserAgent.ParseAdd(_configuration["MelhorEnvio:UserAgent"] ?? "LojaVirtual ([email protected])");
				request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

				var response = await client.SendAsync(request, cancellationToken);
				if (!response.IsSuccessStatusCode)
				{
					var respostaErro = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger.LogWarning("Falha ao cotar frete no Melhor Envio. Status: {StatusCode}. Resposta: {Resposta}", response.StatusCode, respostaErro);
					return await CriarRespostaFixaAsync(
						dto,
						configuracao,
						"Melhor Envio indisponível no momento. Usando valor fixo do produto.",
						cancellationToken);
				}

				var respostaSucesso = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogInformation("Resposta do Melhor Envio recebida: {Resposta}", respostaSucesso);
				using var document = JsonDocument.Parse(respostaSucesso);
				var opcoes = ParsearOpcoesMelhorEnvio(document.RootElement);
				if (opcoes.Count == 0)
				{
					var detalhesErro = ExtrairDetalhesErroMelhorEnvio(document.RootElement);
					if (!string.IsNullOrWhiteSpace(detalhesErro))
					{
						_logger.LogWarning("Melhor Envio retornou sem opções válidas. Detalhes: {Detalhes}", detalhesErro);
					}

					return await CriarRespostaFixaAsync(
						dto,
						configuracao,
						string.IsNullOrWhiteSpace(detalhesErro)
							? "Nenhuma opção de frete disponível no Melhor Envio. Usando valor fixo do produto."
							: $"Melhor Envio sem opções válidas: {detalhesErro}. Usando valor fixo do produto.",
						cancellationToken);
				}

				AplicarPrazoPreparacao(opcoes, configuracao.PrazoPreparacaoDias);

				return new CotacaoFreteResponseDTO
				{
					FreteHabilitado = true,
					ProviderConfigurado = configuracao.Provider,
					ProviderUtilizado = ProviderMelhorEnvio,
					CepOrigem = configuracao.CepOrigem,
					CepDestino = cepDestino,
					PrazoPreparacaoDias = configuracao.PrazoPreparacaoDias,
					UsandoFallbackFixo = false,
					Opcoes = opcoes
						.OrderBy(o => o.Valor)
						.ThenBy(o => o.PrazoEntregaDias)
						.ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Erro ao cotar frete no Melhor Envio. Aplicando fallback fixo.");
				return await CriarRespostaFixaAsync(
					dto,
					configuracao,
					"Erro ao consultar o Melhor Envio. Usando valor fixo do produto.",
					cancellationToken);
			}
		}

		public OpcaoFreteDTO SelecionarOpcao(CotacaoFreteResponseDTO cotacao, string? codigoServicoPreferido)
		{
			if (!string.IsNullOrWhiteSpace(codigoServicoPreferido))
			{
				var selecionada = cotacao.Opcoes.FirstOrDefault(o => string.Equals(o.CodigoServico, codigoServicoPreferido, StringComparison.OrdinalIgnoreCase));
				if (selecionada != null)
				{
					return selecionada;
				}
			}

			return cotacao.Opcoes
				.OrderBy(o => o.Valor)
				.ThenBy(o => o.PrazoEntregaDias)
				.FirstOrDefault() ?? new OpcaoFreteDTO
				{
					Provider = ProviderFixo,
					NomeServico = "Frete fixo",
					Valor = 0,
					PrazoPreparacaoDias = 0,
					PrazoEnvioDias = 0,
					PrazoEntregaDias = 0
				};
		}

		private async Task<(bool FreteHabilitado, string Provider, string CepOrigem, int PrazoPreparacaoDias)> ObterConfiguracaoAsync()
		{
			var freteHabilitado = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FreteHabilitado");
			var freteProvider = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FreteProvider");
			var freteCepOrigem = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FreteCepOrigem");
			var fretePrazoPreparacao = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FretePrazoPreparacaoDias");

			var habilitado = bool.TryParse(freteHabilitado?.Valor, out var ativo) && ativo;
			var provider = string.IsNullOrWhiteSpace(freteProvider?.Valor) ? ProviderFixo : freteProvider.Valor.Trim();
			var cepOrigem = LimparCep(freteCepOrigem?.Valor);
			var prazoPreparacaoDias = int.TryParse(fretePrazoPreparacao?.Valor, out var prazoPreparacao) && prazoPreparacao > 0
				? prazoPreparacao
				: 0;

			return (habilitado, provider, cepOrigem, prazoPreparacaoDias);
		}

		private async Task<CotacaoFreteResponseDTO> CriarRespostaFixaAsync(
			CotacaoFreteRequestDTO dto,
			(bool FreteHabilitado, string Provider, string CepOrigem, int PrazoPreparacaoDias) configuracao,
			string? mensagem,
			CancellationToken cancellationToken)
		{
			var opcaoFixa = await CalcularFreteFixoAsync(dto, cancellationToken);
			return new CotacaoFreteResponseDTO
			{
				FreteHabilitado = configuracao.FreteHabilitado,
				ProviderConfigurado = configuracao.Provider,
				ProviderUtilizado = ProviderFixo,
				CepOrigem = configuracao.CepOrigem,
				CepDestino = LimparCep(dto.CepDestino),
				PrazoPreparacaoDias = configuracao.PrazoPreparacaoDias,
				UsandoFallbackFixo = true,
				Mensagem = mensagem,
				Opcoes = new List<OpcaoFreteDTO> { opcaoFixa }
			};
		}

		private async Task<OpcaoFreteDTO> CalcularFreteFixoAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken)
		{
			_ = cancellationToken;
			decimal maiorFrete = 0;
			int maiorPrazo = 0;

			foreach (var item in dto.Itens)
			{
				var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
				if (produto == null)
				{
					continue;
				}

				if (produto.ValorFrete > maiorFrete)
				{
					maiorFrete = produto.ValorFrete;
				}

				if (produto.PrazoEntregaDias > maiorPrazo)
				{
					maiorPrazo = produto.PrazoEntregaDias;
				}
			}

			return new OpcaoFreteDTO
			{
				Provider = ProviderFixo,
				CodigoServico = "FIXO",
				NomeServico = "Frete fixo",
				NomeTransportadora = "Configuração interna",
				Valor = maiorFrete,
				PrazoPreparacaoDias = 0,
				PrazoEnvioDias = maiorPrazo,
				PrazoEntregaDias = maiorPrazo
			};
		}

		private async Task<List<Dictionary<string, object?>>> MontarProdutosMelhorEnvioAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken)
		{
			_ = cancellationToken;
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

				var valorSeguro = produto.PrecoPromocional ?? produto.Preco;
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

		private static List<OpcaoFreteDTO> ParsearOpcoesMelhorEnvio(JsonElement root)
		{
			var opcoes = new List<OpcaoFreteDTO>();
			if (root.ValueKind != JsonValueKind.Array)
			{
				return opcoes;
			}

			foreach (var element in root.EnumerateArray())
			{
				if (element.TryGetProperty("error", out var errorElement) &&
					errorElement.ValueKind != JsonValueKind.Null &&
					!string.IsNullOrWhiteSpace(errorElement.ToString()))
				{
					continue;
				}

				var valor = ObterDecimal(element, "custom_price") ?? ObterDecimal(element, "price");
				if (!valor.HasValue)
				{
					continue;
				}

				var prazoEnvio = ObterInt(element, "custom_delivery_time") ?? ObterInt(element, "delivery_time") ?? 0;
				var transportadora = element.TryGetProperty("company", out var companyElement)
					? ObterString(companyElement, "name")
					: null;

				opcoes.Add(new OpcaoFreteDTO
				{
					Provider = ProviderMelhorEnvio,
					CodigoServico = ObterString(element, "id") ?? ObterString(element, "service"),
					NomeServico = ObterString(element, "name") ?? "Frete Melhor Envio",
					NomeTransportadora = transportadora,
					Valor = valor.Value,
					PrazoEnvioDias = prazoEnvio
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

		private static string? ExtrairDetalhesErroMelhorEnvio(JsonElement root)
		{
			if (root.ValueKind != JsonValueKind.Array)
			{
				return null;
			}

			var erros = new List<string>();

			foreach (var element in root.EnumerateArray())
			{
				var nomeServico = ObterString(element, "name") ?? "Serviço";
				var transportadora = element.TryGetProperty("company", out var companyElement)
					? ObterString(companyElement, "name")
					: null;
				var erro = ObterErro(element);

				if (string.IsNullOrWhiteSpace(erro))
				{
					continue;
				}

				var prefixo = string.IsNullOrWhiteSpace(transportadora)
					? nomeServico
					: $"{transportadora} / {nomeServico}";

				erros.Add($"{prefixo}: {erro}");
			}

			return erros.Count == 0 ? null : string.Join(" | ", erros);
		}

		private static string? ObterErro(JsonElement element)
		{
			if (!element.TryGetProperty("error", out var errorElement) || errorElement.ValueKind == JsonValueKind.Null)
			{
				return null;
			}

			return errorElement.ValueKind switch
			{
				JsonValueKind.String => errorElement.GetString(),
				JsonValueKind.Object => string.Join("; ", errorElement.EnumerateObject().Select(p => $"{p.Name}: {p.Value}")),
				JsonValueKind.Array => string.Join("; ", errorElement.EnumerateArray().Select(item => item.ToString())),
				_ => errorElement.ToString()
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
