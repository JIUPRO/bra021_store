using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Aplicacao.DTOs
{
	public class MovimentacaoEstoqueDTO
	{
		public Guid Id { get; set; }
		public int Quantidade { get; set; }
		public TipoMovimentacao Tipo { get; set; }
		public string TipoDescricao { get; set; } = string.Empty;
		public string? Motivo { get; set; }
		public DateTime DataMovimentacao { get; set; }
		public int EstoqueAnterior { get; set; }
		public int EstoqueAtual { get; set; }
		public string? Referencia { get; set; }
		public Guid ProdutoTamanhoId { get; set; }
		public string NomeProduto { get; set; } = string.Empty;
		public string Tamanho { get; set; } = string.Empty;
	}

	public class CriarMovimentacaoEstoqueDTO
	{
		public Guid ProdutoTamanhoId { get; set; }
		public int Quantidade { get; set; }
		public TipoMovimentacao Tipo { get; set; }
		public string? Motivo { get; set; }
		public string? Referencia { get; set; }
	}

	public class AdicionarEntradaEstoqueDTO
	{
		public Guid ProdutoTamanhoId { get; set; }
		public int Quantidade { get; set; }
		public string? Motivo { get; set; }
		public string? Referencia { get; set; }
	}

	public class AjustarEstoqueTamanhoDTO
	{
		public Guid ProdutoTamanhoId { get; set; }
		public int NovaQuantidade { get; set; }
		public string? Motivo { get; set; }
	}

	public class AlertaEstoqueDTO
	{
		public Guid ProdutoId { get; set; }
		public string NomeProduto { get; set; } = string.Empty;
		public int QuantidadeEstoque { get; set; }
		public int QuantidadeMinima { get; set; }
		public int Diferenca { get; set; }
	}

	public class AjusteEstoqueDTO
	{
		public Guid ProdutoId { get; set; }
		public int NovaQuantidade { get; set; }
		public string? Motivo { get; set; }
	}

	public class ConsultaMovimentacaoEstoqueDTO
	{
		public Guid? ProdutoId { get; set; }
		public Guid? ProdutoTamanhoId { get; set; }
		public DateTime? DataInicio { get; set; }
		public DateTime? DataFim { get; set; }
	}

	public class ResumoMovimentacaoEstoqueDTO
	{
		public Guid? ProdutoId { get; set; }
		public Guid? ProdutoTamanhoId { get; set; }
		public DateTime? DataInicio { get; set; }
		public DateTime? DataFim { get; set; }
		public int SaldoInicialPeriodo { get; set; }
		public int SaldoAtual { get; set; }
		public int TotalMovimentacoes { get; set; }
		public List<MovimentacaoEstoqueDTO> Movimentacoes { get; set; } = new();
	}
}
