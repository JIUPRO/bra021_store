namespace LojaVirtual.Aplicacao.DTOs
{
	public class ProdutoDTO
	{
		public Guid Id { get; set; }
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
		public bool Ativo { get; set; }
		public Guid CategoriaId { get; set; }
		public string NomeCategoria { get; set; } = string.Empty;
	}

	public class CriarProdutoDTO
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
		public Guid CategoriaId { get; set; }
	}

	public class AtualizarProdutoDTO
	{
		public Guid Id { get; set; }
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
		public bool Ativo { get; set; }
		public Guid CategoriaId { get; set; }
	}

	public class ProdutoResumoDTO
	{
		public Guid Id { get; set; }
		public string Nome { get; set; } = string.Empty;
		public decimal Preco { get; set; }
		public decimal? PrecoPromocional { get; set; }
		public string? ImagemUrl { get; set; }
		public string NomeCategoria { get; set; } = string.Empty;
	}
}
