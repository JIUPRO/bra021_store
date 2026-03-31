namespace LojaVirtual.Dominio.Entidades
{
    public class UsuarioTrocaSenha : EntidadeBase
    {
        public Guid UsuarioId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public DateTime DataExpiracao { get; set; }
        public bool Utilizado { get; set; } = false;

        public Usuario? Usuario { get; set; }

        public UsuarioTrocaSenha() { }

        public UsuarioTrocaSenha(Guid usuarioId, string email, string codigo)
        {
            UsuarioId = usuarioId;
            Email = email;
            Codigo = codigo;
            DataCriacao = DateTime.UtcNow;
            DataExpiracao = DataCriacao.AddMinutes(20);
            Utilizado = false;
        }

        public bool EstaValido()
        {
            return !Utilizado && DateTime.UtcNow <= DataExpiracao;
        }
    }
}
