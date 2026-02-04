namespace LojaVirtual.Dominio.Entidades
{
    public abstract class EntidadeBase
    {
        public Guid Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; }

        protected EntidadeBase()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
        }
    }
}
