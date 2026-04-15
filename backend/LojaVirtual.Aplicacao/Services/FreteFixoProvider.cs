using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Interfaces;

namespace LojaVirtual.Aplicacao.Services
{
	public class FreteFixoProvider : IFreteProvider
	{
		private readonly IUnitOfWork _unitOfWork;

		public FreteFixoProvider(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public string ProviderName => FreteProviderNames.Fixo;

		public async Task<CotacaoFreteResponseDTO> CotarAsync(
			CotacaoFreteRequestDTO dto,
			FreteProviderContext context,
			CancellationToken cancellationToken = default)
		{
			var opcaoFixa = await CalcularFreteFixoAsync(dto, cancellationToken);
			return new CotacaoFreteResponseDTO
			{
				FreteHabilitado = context.FreteHabilitado,
				ProviderConfigurado = context.ProviderConfigurado,
				ProviderUtilizado = FreteProviderNames.Fixo,
				CepOrigem = context.CepOrigem,
				CepDestino = LimparCep(dto.CepDestino),
				PrazoPreparacaoDias = context.PrazoPreparacaoDias,
				UsandoFallbackFixo = true,
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
				Provider = FreteProviderNames.Fixo,
				CodigoServico = "FIXO",
				NomeServico = "Frete fixo",
				NomeTransportadora = "Configuração interna",
				Valor = maiorFrete,
				PrazoPreparacaoDias = 0,
				PrazoEnvioDias = maiorPrazo,
				PrazoEntregaDias = maiorPrazo
			};
		}

		private static string LimparCep(string? cep)
		{
			return new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
		}
	}
}
