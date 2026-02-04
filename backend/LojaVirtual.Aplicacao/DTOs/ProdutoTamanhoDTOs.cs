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
			public bool Ativo { get; set; }
		}

		public class GetById
		{
			public Guid Id { get; set; }
			public Guid ProdutoId { get; set; }
			public string Tamanho { get; set; } = string.Empty;
			public int QuantidadeEstoque { get; set; }
			public bool Ativo { get; set; }
		}

		public class Create
		{
			public Guid ProdutoId { get; set; }
			public string Tamanho { get; set; } = string.Empty;
		}

		public class Update
		{
			public string Tamanho { get; set; } = string.Empty;
			public bool Ativo { get; set; }
		}
	}
}
