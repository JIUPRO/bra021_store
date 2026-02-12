namespace LojaVirtual.Dominio.Entidades
{
    public class ClienteTrocaSenha : EntidadeBase
    {
        public Guid ClienteId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public DateTime DataExpiracao { get; set; }
        public bool Utilizado { get; set; } = false;

        // Relacionamento
        public Cliente? Cliente { get; set; }

        public ClienteTrocaSenha() { }

        public ClienteTrocaSenha(Guid clienteId, string email, string codigo)
        {
            ClienteId = clienteId;
            Email = email;
            Codigo = codigo;
            DataCriacao = DateTime.UtcNow;
            DataExpiracao = DataCriacao.AddMinutes(20); // Válido por 20 minutos
            Utilizado = false;
        }

        public bool EstaValido()
        {
            return !Utilizado && DateTime.UtcNow <= DataExpiracao;
        }
    }
}
