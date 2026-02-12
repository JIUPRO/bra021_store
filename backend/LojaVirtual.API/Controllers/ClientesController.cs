using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Aplicacao.Services;
using LojaVirtual.API.Utilitarios;

namespace LojaVirtual.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ClientesController : ControllerBase
	{
		private readonly IClienteService _clienteService;
		private readonly IConfiguration _configuration;

		public ClientesController(IClienteService servicoCliente, IConfiguration configuration)
		{
			_clienteService = servicoCliente;
			_configuration = configuration;
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ClienteDTO>>> ObterTodos()
		{
			var clientes = await _clienteService.ObterTodosComPedidosAsync();
			return Ok(clientes);
		}

		[AllowAnonymous]
		[HttpGet("{id}")]
		public async Task<ActionResult<ClienteDTO>> ObterPorId(Guid id)
		{
			var cliente = await _clienteService.ObterPorIdAsync(id);
			if (cliente == null)
				return NotFound(new { mensagem = "Cliente não encontrado" });

			return Ok(cliente);
		}

		[AllowAnonymous]
		[HttpGet("email/{email}")]
		public async Task<ActionResult<ClienteDTO>> ObterPorEmail(string email)
		{
			var cliente = await _clienteService.ObterPorEmailAsync(email);
			if (cliente == null)
				return NotFound(new { mensagem = "Cliente não encontrado" });

			return Ok(cliente);
		}

		[AllowAnonymous]
		[HttpGet("cpf/{cpf}")]
		public async Task<ActionResult<ClienteDTO>> ObterPorCpf(string cpf)
		{
			var cliente = await _clienteService.ObterPorCpfAsync(cpf);
			if (cliente == null)
				return NotFound(new { mensagem = "Cliente não encontrado" });

			return Ok(cliente);
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult<ClienteDTO>> Criar([FromBody] CriarClienteDTO dto)
		{
			try
			{
				var cliente = await _clienteService.CriarAsync(dto);
				return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpPut("{id}")]
		public async Task<ActionResult<ClienteDTO>> Atualizar(Guid id, [FromBody] AtualizarClienteDTO dto)
		{
			if (id != dto.Id)
				return BadRequest(new { mensagem = "ID da URL não corresponde ao ID do corpo" });

			try
			{
				var cliente = await _clienteService.AtualizarAsync(dto);
				if (cliente == null)
					return NotFound(new { mensagem = "Cliente não encontrado" });

				return Ok(cliente);
			}
			catch (Exception ex)
			{
				return BadRequest(new { mensagem = ex.Message });
			}
		}

		[Authorize]
		[HttpDelete("{id}")]
		public async Task<IActionResult> Remover(Guid id)
		{
			var resultado = await _clienteService.RemoverAsync(id);
			if (!resultado)
				return NotFound(new { mensagem = "Cliente não encontrado" });

			return NoContent();
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<ActionResult> Login([FromBody] LoginClienteDTO dto)
		{
			var cliente = await _clienteService.AutenticarAsync(dto);
			if (cliente == null)
				return Unauthorized(new { mensagem = "Email ou senha inválidos" });

			// Gerar token JWT para o cliente
			var token = GeradorToken.GerarToken(cliente.Id, cliente.Email, _configuration);

			return Ok(new
			{
				token,
				cliente
			});
		}
	}
}
