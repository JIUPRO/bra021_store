using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Dominio.Entidades
{
	public class Pedido : EntidadeBase
	{
		public string NumeroPedido { get; set; } = string.Empty;
		public DateTime DataPedido { get; set; }
		public StatusPedido Status { get; set; }
		public decimal ValorSubtotal { get; set; }
		public decimal ValorFrete { get; set; }
		public decimal ValorDesconto { get; set; }
		public decimal ValorTotal { get; set; }
		public int PrazoPreparacaoDias { get; set; }
		public int PrazoEnvioDias { get; set; }
		public int PrazoEntregaDias { get; set; }
		public string? Observacoes { get; set; }
		public string? PagamentoId { get; set; }
		public string? MetodoPagamento { get; set; }
		public string? NotaFiscalUrl { get; set; }
		public string? NotaFiscalKey { get; set; }
		public string? TipoEntrega { get; set; }
		public string? TransportadoraFrete { get; set; }
		public string? ServicoFrete { get; set; }
		public string? CodigoServicoFrete { get; set; }
		public string? IntegracaoFretePedidoId { get; set; }
		public string? IntegracaoFreteProtocolo { get; set; }
		public string? CodigoRastreio { get; set; }
		public string? UrlRastreio { get; set; }
		public string? UrlEtiqueta { get; set; }
		public string? StatusLogistico { get; set; }
		public DateTime? DataEtiquetaGerada { get; set; }
		public DateTime? DataPostagem { get; set; }
		public DateTime? DataEntrega { get; set; }

		// Dados de entrega
		public string NomeEntrega { get; set; } = string.Empty;
		public string TelefoneEntrega { get; set; } = string.Empty;
		public string CepEntrega { get; set; } = string.Empty;
		public string LogradouroEntrega { get; set; } = string.Empty;
		public string NumeroEntrega { get; set; } = string.Empty;
		public string? ComplementoEntrega { get; set; }
		public string BairroEntrega { get; set; } = string.Empty;
		public string CidadeEntrega { get; set; } = string.Empty;
		public string EstadoEntrega { get; set; } = string.Empty;

		// Chaves estrangeiras
		public Guid ClienteId { get; set; }
		public Guid? EscolaId { get; set; }

		// Relacionamentos
		public Cliente Cliente { get; set; } = null!;
		public Escola? Escola { get; set; }
		public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
	}
}
