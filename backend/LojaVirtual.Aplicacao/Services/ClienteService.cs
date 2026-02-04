using AutoMapper;
using System.Security.Cryptography;
using System.Text;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Aplicacao.DTOs;

namespace LojaVirtual.Aplicacao.Services
{
	public interface IClienteService
	{
		Task<IEnumerable<ClienteDTO>> ObterTodosAsync();
		Task<IEnumerable<ClienteDTO>> ObterTodosComPedidosAsync();
		Task<ClienteDTO?> ObterPorIdAsync(Guid id);
		Task<ClienteDTO?> ObterPorEmailAsync(string email);
		Task<ClienteDTO?> ObterPorCpfAsync(string cpf);
		Task<ClienteDTO> CriarAsync(CriarClienteDTO dto);
		Task<ClienteDTO?> AtualizarAsync(AtualizarClienteDTO dto);
		Task<bool> RemoverAsync(Guid id);
		Task<ClienteDTO?> AutenticarAsync(LoginClienteDTO dto);
	}

	public class ClienteService : IClienteService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapeador;

		public ClienteService(IUnitOfWork unitOfWork, IMapper mapeador)
		{
			_unitOfWork = unitOfWork;
			_mapeador = mapeador;
		}

		public async Task<IEnumerable<ClienteDTO>> ObterTodosAsync()
		{
			var clientes = await _unitOfWork.Clientes.ObterTodosAsync();
			return _mapeador.Map<IEnumerable<ClienteDTO>>(clientes);
		}

		public async Task<IEnumerable<ClienteDTO>> ObterTodosComPedidosAsync()
		{
			var clientes = await _unitOfWork.Clientes.ObterTodosAsync();
			var resultado = new List<ClienteDTO>();

			foreach (var cliente in clientes)
			{
				var pedidos = await _unitOfWork.Pedidos.ObterPorClienteAsync(cliente.Id);
				var dto = _mapeador.Map<ClienteDTO>(cliente);
				// Assumindo que ClienteDTO tem uma propriedade QuantidadePedidos
				resultado.Add(dto);
			}

			return resultado;
		}

		public async Task<ClienteDTO?> ObterPorEmailAsync(string email)
		{
			var cliente = await _unitOfWork.Clientes.ObterPorEmailAsync(email);
			return cliente == null ? null : _mapeador.Map<ClienteDTO>(cliente);
		}

		public async Task<ClienteDTO?> ObterPorIdAsync(Guid id)
		{
			var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(id);
			return cliente == null ? null : _mapeador.Map<ClienteDTO>(cliente);
		}

		public async Task<ClienteDTO?> ObterPorCpfAsync(string cpf)
		{
			var cliente = await _unitOfWork.Clientes.ObterPorCpfAsync(cpf);
			return cliente == null ? null : _mapeador.Map<ClienteDTO>(cliente);
		}

		public async Task<ClienteDTO> CriarAsync(CriarClienteDTO dto)
		{
			var cliente = _mapeador.Map<Cliente>(dto);
			cliente.DataCriacao = DateTime.UtcNow;
			cliente.Ativo = true;
			cliente.EmailConfirmado = false;

			// Hash da senha
			if (!string.IsNullOrEmpty(dto.Senha))
			{
				cliente.SenhaHash = GerarHashSenha(dto.Senha);
			}

			await _unitOfWork.Clientes.AdicionarAsync(cliente);
			await _unitOfWork.SalvarMudancasAsync();

			return _mapeador.Map<ClienteDTO>(cliente);
		}

		public async Task<ClienteDTO?> AtualizarAsync(AtualizarClienteDTO dto)
		{
			var clienteExistente = await _unitOfWork.Clientes.ObterPorIdAsync(dto.Id);
			if (clienteExistente == null)
				return null;

			_mapeador.Map(dto, clienteExistente);
			clienteExistente.DataAtualizacao = DateTime.UtcNow;

			await _unitOfWork.Clientes.AtualizarAsync(clienteExistente);
			await _unitOfWork.SalvarMudancasAsync();

			return _mapeador.Map<ClienteDTO>(clienteExistente);
		}

		public async Task<bool> RemoverAsync(Guid id)
		{
			var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(id);
			if (cliente == null)
				return false;

			cliente.Ativo = false;
			cliente.DataAtualizacao = DateTime.UtcNow;
			await _unitOfWork.Clientes.AtualizarAsync(cliente);
			await _unitOfWork.SalvarMudancasAsync();

			return true;
		}

		public async Task<ClienteDTO?> AutenticarAsync(LoginClienteDTO dto)
		{
			var cliente = await _unitOfWork.Clientes.ObterPorEmailAsync(dto.Email);
			if (cliente == null)
				return null;

			var hashSenha = GerarHashSenha(dto.Senha);
			if (cliente.SenhaHash != hashSenha)
				return null;

			return _mapeador.Map<ClienteDTO>(cliente);
		}

		private static string GerarHashSenha(string senha)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(senha);
			var hash = sha256.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}
