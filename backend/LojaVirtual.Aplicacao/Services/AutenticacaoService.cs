using System.Security.Cryptography;
using System.Text;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace LojaVirtual.Aplicacao.Services
{
	public class AutenticacaoService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<AutenticacaoService>? _logger;
		private readonly INotificacaoService? _notificacaoService;

		public AutenticacaoService(IUnitOfWork unitOfWork, ILogger<AutenticacaoService>? logger = null, INotificacaoService? notificacaoService = null)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_notificacaoService = notificacaoService;
		}

		public async Task<UsuarioDTO?> RegistrarAsync(RegistroDTO registro)
		{
			var usuarioExistente = await _unitOfWork.Usuarios.ObterPorEmailAsync(registro.Email);
			if (usuarioExistente != null)
			{
				throw new InvalidOperationException("Email já cadastrado");
			}

			var usuario = new Usuario
			{
				Email = registro.Email,
				Nome = registro.Nome,
				SenhaHash = CriptografarSenha(registro.Senha),
				Ativo = true,
				DataCriacao = DateTime.UtcNow
			};

			await _unitOfWork.Usuarios.AdicionarAsync(usuario);
			await _unitOfWork.SalvarMudancasAsync();

			return usuario.Adapt<UsuarioDTO>();
		}

		public async Task<UsuarioDTO?> AutenticarAsync(string email, string senha)
		{
			var usuario = await _unitOfWork.Usuarios.ObterPorEmailAsync(email);

			if (usuario == null)
			{
				Console.WriteLine($"[AUTH] Usuário não encontrado: {email}");
				return null;
			}

			var hashCalculado = CriptografarSenha(senha);
			Console.WriteLine($"[AUTH] Email: {email}");
			Console.WriteLine($"[AUTH] Hash enviado: {hashCalculado}");
			Console.WriteLine($"[AUTH] Hash no banco: {usuario.SenhaHash}");
			Console.WriteLine($"[AUTH] Senhas batem: {hashCalculado == usuario.SenhaHash}");

			if (!VerificarSenha(senha, usuario.SenhaHash))
			{
				Console.WriteLine($"[AUTH] Senha incorreta para {email}");
				return null;
			}

			if (!usuario.Ativo)
			{
				throw new InvalidOperationException("Usuário inativo");
			}

			Console.WriteLine($"[AUTH] Login bem-sucedido para {email}");
			return usuario.Adapt<UsuarioDTO>();
		}

		public async Task<UsuarioDTO?> ObterPorIdAsync(Guid id)
		{
			var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(id);
			return usuario?.Adapt<UsuarioDTO>();
		}

		public async Task<IEnumerable<UsuarioDTO>> ListarTodosAsync()
		{
			var usuarios = await _unitOfWork.Usuarios.ObterTodosAsync();
			return usuarios.Adapt<IEnumerable<UsuarioDTO>>();
		}

		public async Task<UsuarioDTO?> AtualizarAsync(Guid id, AtualizarUsuarioDTO atualizacao)
		{
			var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(id);
			if (usuario == null)
			{
				return null;
			}

			usuario.Nome = atualizacao.Nome;
			usuario.Ativo = atualizacao.Ativo;
			usuario.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Usuarios.AtualizarAsync(usuario);
			await _unitOfWork.SalvarMudancasAsync();

			return usuario.Adapt<UsuarioDTO>();
		}

		public async Task<bool> DeletarAsync(Guid id)
		{
			var resultado = await _unitOfWork.Usuarios.RemoverAsync(id);
			if (resultado)
			{
				await _unitOfWork.SalvarMudancasAsync();
			}
			return resultado;
		}

		public async Task<UsuarioDTO?> CriarUsuarioAsync(CriarUsuarioDTO criacaoDTO)
		{
			var usuarioExistente = await _unitOfWork.Usuarios.ObterPorEmailAsync(criacaoDTO.Email);
			if (usuarioExistente != null)
			{
				throw new InvalidOperationException("Email já cadastrado");
			}

			var usuario = new Usuario
			{
				Email = criacaoDTO.Email,
				Nome = criacaoDTO.Nome,
				SenhaHash = CriptografarSenha(criacaoDTO.Senha),
				Ativo = true,
				DataCriacao = DateTime.UtcNow
			};

			await _unitOfWork.Usuarios.AdicionarAsync(usuario);
			await _unitOfWork.SalvarMudancasAsync();

			return usuario.Adapt<UsuarioDTO>();
		}

		private static string CriptografarSenha(string senha)
		{
			using var sha256 = SHA256.Create();
			var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
			return Convert.ToBase64String(hashedBytes);
		}

		private static bool VerificarSenha(string senha, string senhaHash)
		{
			var hashDoInput = CriptografarSenha(senha);
			return hashDoInput == senhaHash;
		}

		public async Task<(bool sucesso, string mensagem)> EsqueceuSenhaAsync(EsqueceuSenhaDTO dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.Email))
				{
					return (false, "Email é obrigatório");
				}

				var cliente = await _unitOfWork.Clientes.ObterPorEmailAsync(dto.Email);
				if (cliente == null)
				{
					_logger?.LogInformation($"Tentativa de reset de senha com email inexistente: {dto.Email}");
					return (true, "Foi enviado o codigo pro seu email");
				}

				var codigo = GerarCodigoAlfanumerico(6);
				var tokensAntigos = await _unitOfWork.ClientesTrocaSenha.ObterPorClienteIdAsync(cliente.Id);
				foreach (var tokenAntigo in tokensAntigos.Where(t => !t.Utilizado))
				{
					await _unitOfWork.ClientesTrocaSenha.RemoverAsync(tokenAntigo.Id);
				}

				var clienteTrocaSenha = new ClienteTrocaSenha(cliente.Id, cliente.Email, codigo);
				await _unitOfWork.ClientesTrocaSenha.AdicionarAsync(clienteTrocaSenha);
				await _unitOfWork.SalvarMudancasAsync();

				if (_notificacaoService != null)
				{
					await _notificacaoService.EnviarEmailRecuperacaoSenhaAsync(cliente.Email, codigo);
				}

				_logger?.LogInformation($"Código de reset gerado para: {dto.Email} - Código: {codigo}");
				return (true, "Foi enviado o codigo pro seu email");
			}
			catch (Exception ex)
			{
				_logger?.LogError($"Erro ao processar esqueceu senha: {ex.Message}");
				return (false, "Erro ao processar solicitação");
			}
		}

		public async Task<(bool sucesso, string mensagem)> ResetarSenhaAsync(ResetarSenhaDTO dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.Email) ||
					string.IsNullOrWhiteSpace(dto.Codigo) ||
					string.IsNullOrWhiteSpace(dto.NovaSenha))
				{
					return (false, "Email, código e nova senha são obrigatórios");
				}

				if (dto.NovaSenha != dto.ConfirmaSenha)
				{
					return (false, "Senhas não correspondem");
				}

				if (dto.NovaSenha.Length < 6)
				{
					return (false, "Senha deve ter no mínimo 6 caracteres");
				}

				var clienteTrocaSenha = await _unitOfWork.ClientesTrocaSenha.ObterPorEmailECodigoAsync(dto.Email, dto.Codigo);
				if (clienteTrocaSenha == null || !clienteTrocaSenha.EstaValido())
				{
					return (false, "Código inválido ou expirado");
				}

				var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(clienteTrocaSenha.ClienteId);
				if (cliente == null)
				{
					return (false, "Cliente não encontrado");
				}

				cliente.SenhaHash = CriptografarSenha(dto.NovaSenha);
				cliente.DataAtualizacao = DateTime.UtcNow;

				await _unitOfWork.Clientes.AtualizarAsync(cliente);

				clienteTrocaSenha.Utilizado = true;
				await _unitOfWork.ClientesTrocaSenha.AtualizarAsync(clienteTrocaSenha);

				await _unitOfWork.SalvarMudancasAsync();

				_logger?.LogInformation($"Senha resetada para: {dto.Email}");
				return (true, "Senha redefinida com sucesso");
			}
			catch (Exception ex)
			{
				_logger?.LogError($"Erro ao resetar senha: {ex.Message}");
				return (false, "Erro ao redefinir senha");
			}
		}

		private static string GerarCodigoAlfanumerico(int tamanho)
		{
			const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var bytes = new byte[tamanho];
			using var random = RandomNumberGenerator.Create();
			random.GetBytes(bytes);

			var codigo = new char[tamanho];
			for (var i = 0; i < tamanho; i++)
			{
				codigo[i] = caracteres[bytes[i] % caracteres.Length];
			}

			return new string(codigo);
		}
	}
}
