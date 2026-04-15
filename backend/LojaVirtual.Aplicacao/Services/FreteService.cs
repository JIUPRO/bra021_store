using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Interfaces;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IFreteService
	{
		Task<CotacaoFreteResponseDTO> CotarAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken = default);
		OpcaoFreteDTO SelecionarOpcao(CotacaoFreteResponseDTO cotacao, string? codigoServicoPreferido);
	}

	public class FreteService : IFreteService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IFreteProvider _providerFixo;
		private readonly IReadOnlyDictionary<string, IFreteProvider> _providers;

		public FreteService(IUnitOfWork unitOfWork, IEnumerable<IFreteProvider> providers)
		{
			_unitOfWork = unitOfWork;
			_providers = providers.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
			_providerFixo = _providers[FreteProviderNames.Fixo];
		}

		public async Task<CotacaoFreteResponseDTO> CotarAsync(CotacaoFreteRequestDTO dto, CancellationToken cancellationToken = default)
		{
			var configuracao = await ObterConfiguracaoAsync();
			var context = new FreteProviderContext
			{
				FreteHabilitado = configuracao.FreteHabilitado,
				ProviderConfigurado = configuracao.Provider,
				CepOrigem = configuracao.CepOrigem,
				PrazoPreparacaoDias = configuracao.PrazoPreparacaoDias
			};

			if (!configuracao.FreteHabilitado || string.Equals(configuracao.Provider, FreteProviderNames.Fixo, StringComparison.OrdinalIgnoreCase))
			{
				var resposta = await _providerFixo.CotarAsync(dto, context, cancellationToken);
				resposta.Mensagem = !configuracao.FreteHabilitado
					? "Frete dinâmico desabilitado. Usando valor fixo do produto."
					: "Frete fixo configurado. Usando valor fixo do produto.";
				return resposta;
			}

			if (string.IsNullOrWhiteSpace(configuracao.CepOrigem) || LimparCep(dto.CepDestino).Length != 8)
			{
				var resposta = await _providerFixo.CotarAsync(dto, context, cancellationToken);
				resposta.Mensagem = "CEP de origem ou destino inválido para cotação dinâmica. Usando valor fixo do produto.";
				return resposta;
			}

			if (!_providers.TryGetValue(configuracao.Provider, out var providerDinamico))
			{
				var resposta = await _providerFixo.CotarAsync(dto, context, cancellationToken);
				resposta.Mensagem = $"Provider de frete '{configuracao.Provider}' não suportado. Usando valor fixo do produto.";
				return resposta;
			}

			try
			{
				return await providerDinamico.CotarAsync(dto, context, cancellationToken);
			}
			catch (Exception ex) when (!string.Equals(configuracao.Provider, FreteProviderNames.Fixo, StringComparison.OrdinalIgnoreCase))
			{
				var resposta = await _providerFixo.CotarAsync(dto, context, cancellationToken);
				resposta.Mensagem = $"{ex.Message} Usando valor fixo do produto.";
				return resposta;
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
					Provider = FreteProviderNames.Fixo,
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
			var provider = string.IsNullOrWhiteSpace(freteProvider?.Valor) ? FreteProviderNames.Fixo : freteProvider.Valor.Trim();
			var cepOrigem = LimparCep(freteCepOrigem?.Valor);
			var prazoPreparacaoDias = int.TryParse(fretePrazoPreparacao?.Valor, out var prazoPreparacao) && prazoPreparacao > 0
				? prazoPreparacao
				: 0;

			return (habilitado, provider, cepOrigem, prazoPreparacaoDias);
		}

		private static string LimparCep(string? cep)
		{
			return new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
		}
	}
}
