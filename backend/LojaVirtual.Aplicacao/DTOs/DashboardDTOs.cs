using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Aplicacao.DTOs
{
	public class DashboardDTO
	{
		public DashboardEstatisticasDTO Estatisticas { get; set; } = new();
		public List<DashboardPedidoRecenteDTO> PedidosRecentes { get; set; } = new();
		public List<DashboardAlertaDTO> Alertas { get; set; } = new();
		public List<DashboardEstoqueBaixoDTO> EstoqueBaixo { get; set; } = new();
	}

	public class DashboardEstatisticasDTO
	{
		public int TotalPedidos { get; set; }
		public decimal VendasMes { get; set; }
		public int TotalProdutos { get; set; }
		public int TotalClientes { get; set; }
	}

	public class DashboardPedidoRecenteDTO
	{
		public Guid Id { get; set; }
		public string NumeroPedido { get; set; } = string.Empty;
		public string NomeCliente { get; set; } = string.Empty;
		public DateTime DataPedido { get; set; }
		public StatusPedido Status { get; set; }
		public string StatusDescricao { get; set; } = string.Empty;
		public decimal ValorTotal { get; set; }
	}

	public class DashboardAlertaDTO
	{
		public string Titulo { get; set; } = string.Empty;
		public string Mensagem { get; set; } = string.Empty;
	}

	public class DashboardEstoqueBaixoDTO
	{
		public string Produto { get; set; } = string.Empty;
		public string Sku { get; set; } = string.Empty;
		public int Quantidade { get; set; }
	}
}