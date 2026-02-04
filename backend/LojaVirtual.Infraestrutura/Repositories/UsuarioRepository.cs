using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore;

namespace LojaVirtual.Infraestrutura.Repositories
{
	public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
	{
		public UsuarioRepository(LojaDbContext context) : base(context) { }

		public async Task<Usuario?> ObterPorEmailAsync(string email)
		{
			return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
		}

		public async Task<bool> VerificarEmailExisteAsync(string email)
		{
			return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
		}
	}
}
