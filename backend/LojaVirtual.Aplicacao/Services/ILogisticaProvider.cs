using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Services
{
	public interface ILogisticaProvider
	{
		string ProviderName { get; }
		Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task<ArquivoEtiquetaDTO> ObterArquivoEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default);
	}

	public interface IFrenetLogisticaProvider : ILogisticaProvider
	{
		Task ProcessarWebhookAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default);
	}

	public interface IMelhorEnvioLogisticaProvider : ILogisticaProvider
	{
		Task ProcessarWebhookAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default);
	}
}
