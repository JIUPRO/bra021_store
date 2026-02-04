namespace LojaVirtual.Dominio.Entidades
{
	public class ItemPedido : EntidadeBase
	{
		public int Quantidade { get; set; }
		public decimal PrecoUnitario { get; set; }
		public decimal ValorTotal { get; set; }
		public string? Observacoes { get; set; }

		// Chaves estrangeiras
		public Guid PedidoId { get; set; }
		public Guid ProdutoId { get; set; }
		public Guid? ProdutoTamanhoId { get; set; }  // Opcional, para produtos com tamanhos

		// Relacionamentos
		public Pedido Pedido { get; set; } = null!;
		public Produto Produto { get; set; } = null!;
		public ProdutoTamanho? ProdutoTamanho { get; set; }
	}
}
