using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Interfaces;

namespace LojaVirtual.Aplicacao.Services
{
	public interface ILogisticaService
	{
		Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task<ArquivoEtiquetaDTO> ObterArquivoEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default);
		Task ProcessarWebhookFrenetAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default);
		Task ProcessarWebhookMelhorEnvioAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default);
	}

	public class LogisticaService : ILogisticaService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IReadOnlyDictionary<string, ILogisticaProvider> _providers;

		public LogisticaService(IUnitOfWork unitOfWork, IEnumerable<ILogisticaProvider> providers)
		{
			_unitOfWork = unitOfWork;
			_providers = providers.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
		}

		public async Task<GerarEtiquetaResponseDTO> GerarEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var provider = await ObterProviderAsync(pedidoId);
			return await provider.GerarEtiquetaAsync(pedidoId, cancellationToken);
		}

		public async Task<SincronizarRastreioResponseDTO> SincronizarRastreioAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var provider = await ObterProviderAsync(pedidoId);
			return await provider.SincronizarRastreioAsync(pedidoId, cancellationToken);
		}

		public async Task<ArquivoEtiquetaDTO> ObterArquivoEtiquetaAsync(Guid pedidoId, CancellationToken cancellationToken = default)
		{
			var provider = await ObterProviderAsync(pedidoId);
			return await provider.ObterArquivoEtiquetaAsync(pedidoId, cancellationToken);
		}

		public async Task ProcessarWebhookFrenetAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default)
		{
			var provider = ObterProviderPorNome(FreteProviderNames.Frenet);
			if (provider is not IFrenetLogisticaProvider frenetProvider)
			{
				throw new InvalidOperationException("O provider Frenet não está configurado para receber webhooks.");
			}

			await frenetProvider.ProcessarWebhookAsync(rawBody, headers, cancellationToken);
		}

		public async Task ProcessarWebhookMelhorEnvioAsync(string rawBody, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken = default)
		{
			var provider = ObterProviderPorNome(FreteProviderNames.MelhorEnvio);
			if (provider is not IMelhorEnvioLogisticaProvider melhorEnvioProvider)
			{
				throw new InvalidOperationException("O provider Melhor Envio não está configurado para receber webhooks.");
			}

			await melhorEnvioProvider.ProcessarWebhookAsync(rawBody, headers, cancellationToken);
		}

		private async Task<ILogisticaProvider> ObterProviderAsync(Guid pedidoId)
		{
			var providerName = await ResolverProviderLogisticoAsync(pedidoId);
			return ObterProviderPorNome(providerName);
		}

		private ILogisticaProvider ObterProviderPorNome(string providerName)
		{
			if (string.Equals(providerName, FreteProviderNames.Fixo, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("O pedido não possui um provider logístico dinâmico configurado.");
			}

			if (_providers.TryGetValue(providerName, out var provider))
			{
				return provider;
			}

			throw new InvalidOperationException($"O provider logístico '{providerName}' não é suportado.");
		}

		private async Task<string> ResolverProviderLogisticoAsync(Guid pedidoId)
		{
			var pedido = await _unitOfWork.Pedidos.ObterPorIdAsync(pedidoId)
				?? throw new InvalidOperationException("Pedido não encontrado.");

			if (!string.IsNullOrWhiteSpace(pedido.ProviderLogisticaUtilizado))
			{
				return pedido.ProviderLogisticaUtilizado.Trim();
			}

			if (!string.IsNullOrWhiteSpace(pedido.ProviderFreteUtilizado) &&
				!string.Equals(pedido.ProviderFreteUtilizado, FreteProviderNames.Fixo, StringComparison.OrdinalIgnoreCase))
			{
				return pedido.ProviderFreteUtilizado.Trim();
			}

			var providerAtual = (await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("FreteProvider"))?.Valor?.Trim();
			if (string.IsNullOrWhiteSpace(providerAtual))
			{
				return FreteProviderNames.Fixo;
			}

			return providerAtual;
		}
	}
}
