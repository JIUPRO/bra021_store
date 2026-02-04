namespace LojaVirtual.Aplicacao.DTOs
{
	public class EscolaDTO
	{
		public Guid Id { get; set; }
		public string Nome { get; set; } = string.Empty;
		public string? Cep { get; set; }
		public string? Logradouro { get; set; }
		public string? Numero { get; set; }
		public string? Complemento { get; set; }
		public string? Bairro { get; set; }
		public string? Cidade { get; set; }
		public string? Estado { get; set; }
		public string? Contato { get; set; }
		public string? ProfessorResponsavel { get; set; }
		public decimal PercentualComissao { get; set; }
		public bool Ativo { get; set; }
		public DateTime DataCriacao { get; set; }
	}

	public class CriarEscolaDTO
	{
		public string Nome { get; set; } = string.Empty;
		public string? Cep { get; set; }
		public string? Logradouro { get; set; }
		public string? Numero { get; set; }
		public string? Complemento { get; set; }
		public string? Bairro { get; set; }
		public string? Cidade { get; set; }
		public string? Estado { get; set; }
		public string? Contato { get; set; }
		public string? ProfessorResponsavel { get; set; }
		public decimal PercentualComissao { get; set; }
	}

	public class AtualizarEscolaDTO
	{
		public Guid Id { get; set; }
		public string Nome { get; set; } = string.Empty;
		public string? Cep { get; set; }
		public string? Logradouro { get; set; }
		public string? Numero { get; set; }
		public string? Complemento { get; set; }
		public string? Bairro { get; set; }
		public string? Cidade { get; set; }
		public string? Estado { get; set; }
		public string? Contato { get; set; }
		public string? ProfessorResponsavel { get; set; }
		public decimal PercentualComissao { get; set; }
		public bool Ativo { get; set; }
	}
}
