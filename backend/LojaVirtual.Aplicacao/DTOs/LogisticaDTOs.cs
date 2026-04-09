namespace LojaVirtual.Aplicacao.DTOs
{
	public class GerarEtiquetaResponseDTO
	{
		public string PedidoId { get; set; } = string.Empty;
		public string? IntegracaoFretePedidoId { get; set; }
		public string? IntegracaoFreteProtocolo { get; set; }
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
		public string? IntegracaoFretePedidoId { get; set; }
		public string? CodigoRastreio { get; set; }
		public string? UrlRastreio { get; set; }
		public string? StatusLogistico { get; set; }
		public DateTime? DataPostagem { get; set; }
		public DateTime? DataEntrega { get; set; }
		public string? Mensagem { get; set; }
	}

	public class FrenetTrackingWebhookDTO
	{
		public string? OrderId { get; set; }
		public long? ShipmentId { get; set; }
		public string? TrackingUrl { get; set; }
		public string? TrackingNumber { get; set; }
		public string? ServiceDescrition { get; set; }
		public List<FrenetTrackingEventDTO> TrackingEvents { get; set; } = new();
	}

	public class FrenetTrackingEventDTO
	{
		public string? EventDateTime { get; set; }
		public string? EventDescription { get; set; }
		public string? EventLocation { get; set; }
		public string? EventType { get; set; }
	}
}
