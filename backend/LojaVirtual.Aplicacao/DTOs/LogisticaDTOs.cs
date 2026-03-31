namespace LojaVirtual.Aplicacao.DTOs
{
	public class GerarEtiquetaResponseDTO
	{
		public string PedidoId { get; set; } = string.Empty;
		public string? MelhorEnvioPedidoId { get; set; }
		public string? MelhorEnvioProtocolo { get; set; }
		public string? CodigoRastreio { get; set; }
		public string? UrlRastreio { get; set; }
		public string? UrlEtiqueta { get; set; }
		public string? StatusLogistico { get; set; }
		public DateTime? DataEtiquetaGerada { get; set; }
		public string? Mensagem { get; set; }
	}

	public class SincronizarRastreioResponseDTO
	{
		public string PedidoId { get; set; } = string.Empty;
		public string? MelhorEnvioPedidoId { get; set; }
		public string? CodigoRastreio { get; set; }
		public string? UrlRastreio { get; set; }
		public string? StatusLogistico { get; set; }
		public DateTime? DataPostagem { get; set; }
		public DateTime? DataEntrega { get; set; }
		public string? Mensagem { get; set; }
	}

	public class MelhorEnvioWebhookDTO
	{
		public string Event { get; set; } = string.Empty;
		public MelhorEnvioWebhookDataDTO Data { get; set; } = new();
	}

	public class MelhorEnvioWebhookDataDTO
	{
		public string? Id { get; set; }
		public string? Protocol { get; set; }
		public string? Status { get; set; }
		public string? Tracking { get; set; }
		public string? Tracking_Url { get; set; }
		public DateTimeOffset? Created_At { get; set; }
		public DateTimeOffset? Paid_At { get; set; }
		public DateTimeOffset? Generated_At { get; set; }
		public DateTimeOffset? Posted_At { get; set; }
		public DateTimeOffset? Delivered_At { get; set; }
	}
}
