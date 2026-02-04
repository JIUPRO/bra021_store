using LojaVirtual.Dominio.Enums;

namespace LojaVirtual.Dominio.Entidades
{
	public class MovimentacaoEstoque : EntidadeBase
	{
		public int Quantidade { get; set; }
		public TipoMovimentacao Tipo { get; set; }
		public string? Motivo { get; set; }
		public DateTime DataMovimentacao { get; set; }
		public int EstoqueAnterior { get; set; }
		public int EstoqueAtual { get; set; }
		public string? Referencia { get; set; }

		// Chaves estrangeiras
		public Guid ProdutoTamanhoId { get; set; }

		// Relacionamentos
		public ProdutoTamanho ProdutoTamanho { get; set; } = null!;
	}
}
