using System.Security.Claims;
using LojaVirtual.API.Utilitarios;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly AutenticacaoService _AutenticacaoService;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthController> _logger;

		public AuthController(AutenticacaoService AutenticacaoService, IConfiguration configuration, ILogger<AuthController> logger)
		{
			_AutenticacaoService = AutenticacaoService;
			_configuration = configuration;
			_logger = logger;
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDTO)
		{
			try
			{
				if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Senha))
				{
					return BadRequest("Email e senha são obrigatórios");
				}

				var usuario = await _AutenticacaoService.AutenticarAsync(loginDTO.Email, loginDTO.Senha);
				if (usuario == null)
				{
					return Unauthorized("Email ou senha incorretos");
				}

				var token = GeradorToken.GerarToken(usuario.Id, usuario.Email, _configuration);

				return Ok(new
				{
					token,
					usuario
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao fazer login");
				return StatusCode(500, "Erro ao fazer login");
			}
		}

		[AllowAnonymous]
		[HttpPost("registro")]
		public async Task<ActionResult<LoginResponseDTO>> Registro([FromBody] RegistroDTO registroDTO)
		{
			try
			{
				if (string.IsNullOrEmpty(registroDTO.Email) || string.IsNullOrEmpty(registroDTO.Senha) || string.IsNullOrEmpty(registroDTO.Nome))
				{
					return BadRequest("Email, nome e senha são obrigatórios");
				}

				var usuario = await _AutenticacaoService.RegistrarAsync(registroDTO);
				if (usuario == null)
				{
					return BadRequest("Erro ao registrar usuário");
				}

				var token = GeradorToken.GerarToken(usuario.Id, usuario.Email, _configuration);

				return Ok(new
				{
					token,
					usuario
				});
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao registrar usuário");
				return StatusCode(500, "Erro ao registrar usuário");
			}
		}

		[Authorize]
		[HttpGet("usuarios")]
		public async Task<ActionResult<IEnumerable<UsuarioDTO>>> ListarUsuarios()
		{
			try
			{
				var usuarios = await _AutenticacaoService.ListarTodosAsync();
				return Ok(usuarios);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao listar usuários");
				return StatusCode(500, "Erro ao listar usuários");
			}
		}

		[Authorize]
		[HttpGet("usuarios/{id}")]
		public async Task<ActionResult<UsuarioDTO>> ObterUsuario(Guid id)
		{
			try
			{
				var usuario = await _AutenticacaoService.ObterPorIdAsync(id);
				if (usuario == null)
				{
					return NotFound("Usuário não encontrado");
				}

				return Ok(usuario);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao obter usuário");
				return StatusCode(500, "Erro ao obter usuário");
			}
		}

		[Authorize]
		[HttpPost("usuarios")]
		public async Task<ActionResult<UsuarioDTO>> CriarUsuario([FromBody] CriarUsuarioDTO criacaoDTO)
		{
			try
			{
				if (string.IsNullOrEmpty(criacaoDTO.Email) || string.IsNullOrEmpty(criacaoDTO.Senha) || string.IsNullOrEmpty(criacaoDTO.Nome))
				{
					return BadRequest("Email, nome e senha são obrigatórios");
				}

				var usuario = await _AutenticacaoService.CriarUsuarioAsync(criacaoDTO);
				return CreatedAtAction(nameof(ObterUsuario), new { id = usuario.Id }, usuario);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao criar usuário");
				return StatusCode(500, "Erro ao criar usuário");
			}
		}

		[Authorize]
		[HttpPut("usuarios/{id}")]
		public async Task<ActionResult<UsuarioDTO>> AtualizarUsuario(Guid id, [FromBody] AtualizarUsuarioDTO atualizacaoDTO)
		{
			try
			{
				var usuario = await _AutenticacaoService.AtualizarAsync(id, atualizacaoDTO);
				if (usuario == null)
				{
					return NotFound("Usuário não encontrado");
				}

				return Ok(usuario);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao atualizar usuário");
				return StatusCode(500, "Erro ao atualizar usuário");
			}
		}

		[Authorize]
		[HttpDelete("usuarios/{id}")]
		public async Task<ActionResult> DeletarUsuario(Guid id)
		{
			try
			{
				var sucesso = await _AutenticacaoService.DeletarAsync(id);
				if (!sucesso)
				{
					return NotFound("Usuário não encontrado");
				}

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao deletar usuário");
				return StatusCode(500, "Erro ao deletar usuário");
			}
		}
	}
}
