using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
    public class UsuarioTrocaSenhaRepository : BaseRepository<UsuarioTrocaSenha>, IUsuarioTrocaSenhaRepository
    {
        public UsuarioTrocaSenhaRepository(LojaDbContext contexto) : base(contexto)
        {
        }

        public async Task<UsuarioTrocaSenha?> ObterPorCodigoAsync(string codigo)
        {
            return await _dbSet
                .Include(uts => uts.Usuario)
                .FirstOrDefaultAsync(c => c.Codigo == codigo && !c.Utilizado && c.DataExpiracao > DateTime.UtcNow);
        }

        public async Task<IEnumerable<UsuarioTrocaSenha>> ObterPorUsuarioIdAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();
        }

        public async Task<UsuarioTrocaSenha?> ObterPorEmailECodigoAsync(string email, string codigo)
        {
            return await _dbSet
                .Include(uts => uts.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.Email.ToLower() == email.ToLower() &&
                    c.Codigo == codigo &&
                    !c.Utilizado &&
                    c.DataExpiracao > DateTime.UtcNow);
        }

        public async Task<bool> LimparExpiradosAsync()
        {
            var expirados = await _dbSet
                .Where(c => !c.Utilizado && c.DataExpiracao <= DateTime.UtcNow)
                .ToListAsync();

            if (expirados.Any())
            {
                _dbSet.RemoveRange(expirados);
                await _contexto.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
