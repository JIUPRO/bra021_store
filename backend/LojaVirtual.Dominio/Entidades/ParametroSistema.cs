namespace LojaVirtual.Dominio.Entidades
{
	public class ParametroSistema : EntidadeBase
	{
		public string Chave { get; set; } = string.Empty;
		public string Valor { get; set; } = string.Empty;
		public string? Descricao { get; set; }
		public string Tipo { get; set; } = "String"; // String, Numero, Booleano, Lista
	}
}
