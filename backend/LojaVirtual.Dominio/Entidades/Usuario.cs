namespace LojaVirtual.Dominio.Entidades
{
	public class Usuario : EntidadeBase
	{
		public string Email { get; set; } = string.Empty;
		public string Nome { get; set; } = string.Empty;
		public string SenhaHash { get; set; } = string.Empty;
	}
}
