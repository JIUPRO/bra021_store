namespace LojaVirtual.Dominio.Entidades
{
	public class ProdutoTamanho : EntidadeBase
	{
		public Guid ProdutoId { get; set; }
		public string Tamanho { get; set; } = string.Empty;
		public int QuantidadeEstoque { get; set; }
		public bool Ativo { get; set; } = true;

		// Relacionamentos
		public Produto Produto { get; set; } = null!;
		public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
		public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new List<MovimentacaoEstoque>();
	}
}
