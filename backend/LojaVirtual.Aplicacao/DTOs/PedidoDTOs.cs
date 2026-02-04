using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Aplicacao.DTOs
{
	public class PedidoDTO
	{
		public Guid Id { get; set; }
		public string NumeroPedido { get; set; } = string.Empty;
		public DateTime DataPedido { get; set; }
		public StatusPedido Status { get; set; }
		public string StatusDescricao { get; set; } = string.Empty;
		public decimal ValorSubtotal { get; set; }
		public decimal ValorFrete { get; set; }
		public decimal ValorDesconto { get; set; }
		public decimal ValorTotal { get; set; }
		public int PrazoEntregaDias { get; set; }
		public string? Observacoes { get; set; }
		public string? PagamentoId { get; set; }
		public string? MetodoPagamento { get; set; }
		public string? NotaFiscalUrl { get; set; }
		public string NomeEntrega { get; set; } = string.Empty;
		public string TelefoneEntrega { get; set; } = string.Empty;
		public string CepEntrega { get; set; } = string.Empty;
		public string LogradouroEntrega { get; set; } = string.Empty;
		public string NumeroEntrega { get; set; } = string.Empty;
		public string? ComplementoEntrega { get; set; }
		public string BairroEntrega { get; set; } = string.Empty;
		public string CidadeEntrega { get; set; } = string.Empty;
		public string EstadoEntrega { get; set; } = string.Empty;
		public Guid ClienteId { get; set; }
		public string NomeCliente { get; set; } = string.Empty;
		public string EmailCliente { get; set; } = string.Empty;
		public string TelefoneCliente { get; set; } = string.Empty;
		public Guid? EscolaId { get; set; }
		public string? NomeEscola { get; set; }
		public List<ItemPedidoDTO> Itens { get; set; } = new();
	}

	public class ItemPedidoDTO
	{
		public Guid Id { get; set; }
		public int Quantidade { get; set; }
		public decimal PrecoUnitario { get; set; }
		public decimal ValorTotal { get; set; }
		public string? Observacoes { get; set; }
		public Guid ProdutoId { get; set; }
		public string NomeProduto { get; set; } = string.Empty;
		public string? ImagemProduto { get; set; }
		public Guid? ProdutoTamanhoId { get; set; }
		public string? TamanhoVariacao { get; set; }
	}

	public class CriarItemPedidoDTO
	{
		public Guid ProdutoId { get; set; }
		public Guid? ProdutoTamanhoId { get; set; }
		public int Quantidade { get; set; }
		public string? Observacoes { get; set; }
	}

	public class CriarPedidoDTO
	{
		public Guid ClienteId { get; set; }
		public Guid? EscolaId { get; set; }
		public List<CriarItemPedidoDTO> Itens { get; set; } = new();
		public decimal ValorFrete { get; set; }
		public decimal ValorDesconto { get; set; }
		public int PrazoEntregaDias { get; set; }
		public string? Observacoes { get; set; }
		public string NomeEntrega { get; set; } = string.Empty;
		public string TelefoneEntrega { get; set; } = string.Empty;
		public string CepEntrega { get; set; } = string.Empty;
		public string LogradouroEntrega { get; set; } = string.Empty;
		public string NumeroEntrega { get; set; } = string.Empty;
		public string? ComplementoEntrega { get; set; }
		public string BairroEntrega { get; set; } = string.Empty;
		public string CidadeEntrega { get; set; } = string.Empty;
		public string EstadoEntrega { get; set; } = string.Empty;
	}

	public class AtualizarStatusPedidoDTO
	{
		public Guid Id { get; set; }
		public StatusPedido Status { get; set; }
	}

	public class AtualizarNotaFiscalDTO
	{
		public Guid Id { get; set; }
		public string NotaFiscalUrl { get; set; } = string.Empty;
	}

	public class ResumoPedidoDTO
	{
		public Guid Id { get; set; }
		public string NumeroPedido { get; set; } = string.Empty;
		public DateTime DataPedido { get; set; }
		public StatusPedido Status { get; set; }
		public string StatusDescricao { get; set; } = string.Empty;
		public decimal ValorTotal { get; set; }
		public int QuantidadeItens { get; set; }
		public int PrazoEntregaDias { get; set; }
		public string NomeCliente { get; set; } = string.Empty;
		public string? NomeEscola { get; set; }
		public string? MetodoPagamento { get; set; }
		public List<ResumoProdutoDTO> Produtos { get; set; } = new();
	}

	public class ResumoProdutoDTO
	{
		public string Nome { get; set; } = string.Empty;
		public string? Tamanho { get; set; }
		public int Quantidade { get; set; }
	}
}
