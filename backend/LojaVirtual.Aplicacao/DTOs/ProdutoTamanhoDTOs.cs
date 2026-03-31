namespace LojaVirtual.Aplicacao.DTOs
{
	public class ProdutoTamanhoDTOs
	{
		public class GetAll
		{
			public Guid Id { get; set; }
			public Guid ProdutoId { get; set; }
			public string Tamanho { get; set; } = string.Empty;
			public int QuantidadeEstoque { get; set; }
			public double Peso { get; set; }
			public double? Altura { get; set; }
			public double? Largura { get; set; }
			public double? Profundidade { get; set; }
			public bool Ativo { get; set; }
		}

		public class GetById
		{
			public Guid Id { get; set; }
			public Guid ProdutoId { get; set; }
			public string Tamanho { get; set; } = string.Empty;
			public int QuantidadeEstoque { get; set; }
			public double Peso { get; set; }
			public double? Altura { get; set; }
			public double? Largura { get; set; }
			public double? Profundidade { get; set; }
			public bool Ativo { get; set; }
		}

		public class Create
		{
			public Guid ProdutoId { get; set; }
			public string Tamanho { get; set; } = string.Empty;
			public double Peso { get; set; }
			public double? Altura { get; set; }
			public double? Largura { get; set; }
			public double? Profundidade { get; set; }
			public bool Ativo { get; set; } = true;
		}

		public class Update
		{
			public string Tamanho { get; set; } = string.Empty;
			public double Peso { get; set; }
			public double? Altura { get; set; }
			public double? Largura { get; set; }
			public double? Profundidade { get; set; }
			public bool Ativo { get; set; }
		}
	}
}
