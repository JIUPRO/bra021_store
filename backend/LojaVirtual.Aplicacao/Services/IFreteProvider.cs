using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IFreteProvider
	{
		string ProviderName { get; }
		Task<CotacaoFreteResponseDTO> CotarAsync(
			CotacaoFreteRequestDTO dto,
			FreteProviderContext context,
			CancellationToken cancellationToken = default);
	}

	public sealed class FreteProviderContext
	{
		public bool FreteHabilitado { get; init; }
		public string ProviderConfigurado { get; init; } = FreteProviderNames.Fixo;
		public string CepOrigem { get; init; } = string.Empty;
		public int PrazoPreparacaoDias { get; init; }
	}
}
