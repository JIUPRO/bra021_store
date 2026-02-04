namespace LojaVirtual.Dominio.Entidades
{
	public class Escola : EntidadeBase
	{
		public string Nome { get; set; } = string.Empty;

		// Endereço completo
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

		// Navegação
		public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
	}
}
