namespace LojaVirtual.Aplicacao.DTOs
{
    public class CategoriaDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public int OrdemExibicao { get; set; }
        public int QuantidadeProdutos { get; set; }
    }

    public class CriarCategoriaDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public int OrdemExibicao { get; set; }
    }

    public class AtualizarCategoriaDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public int OrdemExibicao { get; set; }
    }
}
