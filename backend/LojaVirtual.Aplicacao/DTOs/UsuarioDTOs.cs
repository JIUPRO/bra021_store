using System.Text.Json.Serialization;

namespace LojaVirtual.Aplicacao.DTOs
{
	public class UsuarioDTO
	{
		[JsonPropertyName("id")]
		public Guid Id { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; } = string.Empty;

		[JsonPropertyName("nome")]
		public string Nome { get; set; } = string.Empty;

		[JsonPropertyName("ativo")]
		public bool Ativo { get; set; }
	}

	public class CriarUsuarioDTO
	{
		public string Email { get; set; } = string.Empty;
		public string Nome { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
	}

	public class AtualizarUsuarioDTO
	{
		public string Nome { get; set; } = string.Empty;
		public bool Ativo { get; set; }
	}

	public class LoginDTO
	{
		public string Email { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
	}

	public class LoginResponseDTO
	{
		[JsonPropertyName("token")]
		public string Token { get; set; } = string.Empty;

		[JsonPropertyName("usuario")]
		public UsuarioDTO Usuario { get; set; } = null!;
	}

	public class RegistroDTO
	{
		public string Email { get; set; } = string.Empty;
		public string Nome { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
	}
}
