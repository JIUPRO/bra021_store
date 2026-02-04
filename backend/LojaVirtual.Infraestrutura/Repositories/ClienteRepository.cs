using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
    public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(LojaDbContext contexto) : base(contexto)
        {
        }

        public override async Task<Cliente?> ObterPorIdAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Pedidos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cliente?> ObterPorEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower() && c.Ativo);
        }

        public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Cpf == cpf && c.Ativo);
        }

        public async Task<IEnumerable<Cliente>> ObterClientesComPedidosAsync()
        {
            return await _dbSet
                .Include(c => c.Pedidos)
                .Where(c => c.Ativo && c.Pedidos.Any())
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }
    }
}
