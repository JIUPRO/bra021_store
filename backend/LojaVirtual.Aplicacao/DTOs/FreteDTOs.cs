namespace LojaVirtual.Aplicacao.DTOs
{
	public class CotacaoFreteRequestDTO
	{
		public string CepDestino { get; set; } = string.Empty;
		public string? CodigoServico { get; set; }
		public List<CotacaoFreteItemDTO> Itens { get; set; } = new();
	}

	public class CotacaoFreteItemDTO
	{
		public Guid ProdutoId { get; set; }
		public Guid? ProdutoTamanhoId { get; set; }
		public int Quantidade { get; set; }
	}

	public class CotacaoFreteResponseDTO
	{
		public bool FreteHabilitado { get; set; }
		public string ProviderConfigurado { get; set; } = "Fixo";
		public string ProviderUtilizado { get; set; } = "Fixo";
		public string CepOrigem { get; set; } = string.Empty;
		public string CepDestino { get; set; } = string.Empty;
		public int PrazoPreparacaoDias { get; set; }
		public bool UsandoFallbackFixo { get; set; }
		public string? Mensagem { get; set; }
		public List<OpcaoFreteDTO> Opcoes { get; set; } = new();
	}

	public class OpcaoFreteDTO
	{
		public string Provider { get; set; } = "Fixo";
		public string? CodigoServico { get; set; }
		public string NomeServico { get; set; } = string.Empty;
		public string? NomeTransportadora { get; set; }
		public decimal Valor { get; set; }
		public int PrazoPreparacaoDias { get; set; }
		public int PrazoEnvioDias { get; set; }
		public int PrazoEntregaDias { get; set; }
	}
}
