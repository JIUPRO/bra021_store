using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using LojaVirtual.Aplicacao.DTOs;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;

namespace LojaVirtual.Aplicacao.Services
{
	public class AutenticacaoService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AutenticacaoService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<UsuarioDTO?> RegistrarAsync(RegistroDTO registro)
		{
			// Verificar se email já existe
			var usuarioExistente = await _unitOfWork.Usuarios.ObterPorEmailAsync(registro.Email);
			if (usuarioExistente != null)
			{
				throw new InvalidOperationException("Email já cadastrado");
			}

			// Criar novo usuário
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

			return _mapper.Map<UsuarioDTO>(usuario);
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
			return _mapper.Map<UsuarioDTO>(usuario);
		}

		public async Task<UsuarioDTO?> ObterPorIdAsync(Guid id)
		{
			var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(id);
			return usuario != null ? _mapper.Map<UsuarioDTO>(usuario) : null;
		}

		public async Task<IEnumerable<UsuarioDTO>> ListarTodosAsync()
		{
			var usuarios = await _unitOfWork.Usuarios.ObterTodosAsync();
			return _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
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

			return _mapper.Map<UsuarioDTO>(usuario);
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
			// Verificar se email já existe
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

			return _mapper.Map<UsuarioDTO>(usuario);
		}

		private static string CriptografarSenha(string senha)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
				return Convert.ToBase64String(hashedBytes);
			}
		}

		private static bool VerificarSenha(string senha, string senhaHash)
		{
			var hashDoInput = CriptografarSenha(senha);
			return hashDoInput == senhaHash;
		}
	}
}
