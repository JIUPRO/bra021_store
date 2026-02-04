using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LojaVirtual.API.Utilitarios
{
	public class GeradorToken
	{
		public static string GerarToken(Guid usuarioId, string email, IConfiguration configuration)
		{
			var secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey não configurada");
			var issuer = configuration["Jwt:Issuer"] ?? "LojaVirtual";
			var audience = configuration["Jwt:Audience"] ?? "LojaVirtualClient";
			var expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
				new Claim(ClaimTypes.Email, email),
				new Claim("sub", usuarioId.ToString())
			};

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
				signingCredentials: credentials
			);

			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.WriteToken(token);
		}
	}
}
