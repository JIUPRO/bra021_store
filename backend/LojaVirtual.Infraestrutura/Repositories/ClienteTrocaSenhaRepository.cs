using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
    public class ClienteTrocaSenhaRepository : BaseRepository<ClienteTrocaSenha>, IClienteTrocaSenhaRepository
    {
        public ClienteTrocaSenhaRepository(LojaDbContext contexto) : base(contexto)
        {
        }

        public async Task<ClienteTrocaSenha?> ObterPorCodigoAsync(string codigo)
        {
            return await _dbSet
                .Include(cts => cts.Cliente)
                .FirstOrDefaultAsync(c => c.Codigo == codigo && !c.Utilizado && c.DataExpiracao > DateTime.UtcNow);
        }

        public async Task<IEnumerable<ClienteTrocaSenha>> ObterPorClienteIdAsync(Guid clienteId)
        {
            return await _dbSet
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();
        }

        public async Task<ClienteTrocaSenha?> ObterPorEmailECodigoAsync(string email, string codigo)
        {
            return await _dbSet
                .Include(cts => cts.Cliente)
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
