using Microsoft.EntityFrameworkCore;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Interfaces;
using LojaVirtual.Infraestrutura.Data;

namespace LojaVirtual.Infraestrutura.Repositories
{
    public class CategoriaRepository : BaseRepository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(LojaDbContext contexto) : base(contexto)
        {
        }

        public async Task<Categoria?> ObterComProdutosAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Produtos.Where(p => p.Ativo))
                .FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
        }

        public override async Task<IEnumerable<Categoria>> ObterTodosAsync()
        {
            return await _dbSet
                .Where(c => c.Ativo)
                .OrderBy(c => c.OrdemExibicao)
                .ThenBy(c => c.Nome)
                .ToListAsync();
        }
    }
}
