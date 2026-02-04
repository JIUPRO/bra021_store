namespace LojaVirtual.Aplicacao.DTOs
{
	public class ParametroSistemaDTO
	{
		public Guid Id { get; set; }
		public string Chave { get; set; } = string.Empty;
		public string Valor { get; set; } = string.Empty;
		public string? Descricao { get; set; }
		public string Tipo { get; set; } = "String";
		public DateTime DataCriacao { get; set; }
		public DateTime DataAtualizacao { get; set; }
	}

	public class AtualizarParametroSistemaDTO
	{
		public string Valor { get; set; } = string.Empty;
	}

	public class CriarParametroSistemaDTO
	{
		public string Chave { get; set; } = string.Empty;
		public string Valor { get; set; } = string.Empty;
		public string? Descricao { get; set; }
	}
}
