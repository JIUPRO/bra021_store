namespace LojaVirtual.Dominio.Entidades
{
	public class ProdutoTamanho : EntidadeBase
	{
		public Guid ProdutoId { get; set; }
		public string Tamanho { get; set; } = string.Empty;
		public int QuantidadeEstoque { get; set; }
		public double Peso { get; set; }
		public double? Altura { get; set; }
		public double? Largura { get; set; }
		public double? Profundidade { get; set; }

		// Relacionamentos
		public Produto Produto { get; set; } = null!;
		public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
		public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new List<MovimentacaoEstoque>();
	}
}
