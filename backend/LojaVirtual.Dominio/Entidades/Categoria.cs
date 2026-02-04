namespace LojaVirtual.Dominio.Entidades
{
    public class Categoria : EntidadeBase
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public int OrdemExibicao { get; set; }
        
        // Relacionamentos
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
