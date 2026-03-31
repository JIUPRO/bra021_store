namespace LojaVirtual.Dominio.Entidades
{
	public class Produto : EntidadeBase
	{
		public string Nome { get; set; } = string.Empty;
		public string? Descricao { get; set; }
		public string? DescricaoCurta { get; set; }
		public decimal Preco { get; set; }
		public decimal? PrecoPromocional { get; set; }
		public decimal ValorFrete { get; set; }
		public int PrazoEntregaDias { get; set; }
		public string? ImagemUrl { get; set; }
		public string? ImagemKey { get; set; }
		public int QuantidadeMinimaEstoque { get; set; }
		public bool Destaque { get; set; }

		// Chaves estrangeiras
		public Guid CategoriaId { get; set; }

		// Relacionamentos
		public Categoria Categoria { get; set; } = null!;
		public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
		public ICollection<ProdutoTamanho> Tamanhos { get; set; } = new List<ProdutoTamanho>();
	}
}
