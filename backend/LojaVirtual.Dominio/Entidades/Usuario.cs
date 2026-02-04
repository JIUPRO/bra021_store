namespace LojaVirtual.Dominio.Entidades
{
	public class Usuario : EntidadeBase
	{
		public string Email { get; set; } = string.Empty;
		public string Nome { get; set; } = string.Empty;
		public string SenhaHash { get; set; } = string.Empty;
		public bool Ativo { get; set; } = true;
		public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
		public DateTime? DataAtualizacao { get; set; }
	}
}
