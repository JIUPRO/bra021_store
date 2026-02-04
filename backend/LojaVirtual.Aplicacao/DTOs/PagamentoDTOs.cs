namespace LojaVirtual.Aplicacao.DTOs
{
	public class CriarPagamentoRequestDTO
	{
		public Guid PedidoId { get; set; }
		public string MetodoPagamento { get; set; } = string.Empty; // pix, credit_card, debit_card
		public DadosCartaoDTO? DadosCartao { get; set; }
		public int Parcelas { get; set; } = 1; // Número de parcelas (1-3 para cartão, ignorado para PIX)
	}

	public class DadosCartaoDTO
	{
		public string CardNumber { get; set; } = string.Empty;
		public string CardholderName { get; set; } = string.Empty;
		public string ExpirationMonth { get; set; } = string.Empty;
		public string ExpirationYear { get; set; } = string.Empty;
		public string SecurityCode { get; set; } = string.Empty;
		public string CardToken { get; set; } = string.Empty;
	}

	public class PagamentoResponseDTO
	{
		public bool Sucesso { get; set; }
		public string? PagamentoId { get; set; }
		public string? Status { get; set; }
		public string? StatusDetalhe { get; set; }
		public string? QrCodeBase64 { get; set; }
		public string? QrCode { get; set; }
		public string? TicketUrl { get; set; }
		public string? Mensagem { get; set; }
		public int? Parcelas { get; set; }
		public decimal? ValorParcela { get; set; }
		public string? CodigoErro { get; set; } // Código do erro do Mercado Pago
	}

	public class WebhookNotificacaoDTO
	{
		public string Action { get; set; } = string.Empty;
		public string ApiVersion { get; set; } = string.Empty;
		public WebhookDataDTO Data { get; set; } = new();
		public DateTime DateCreated { get; set; }
		public long Id { get; set; }
		public bool LiveMode { get; set; }
		public string Type { get; set; } = string.Empty;
		public long UserId { get; set; }
	}

	public class WebhookDataDTO
	{
		public string Id { get; set; } = string.Empty;
	}
}
