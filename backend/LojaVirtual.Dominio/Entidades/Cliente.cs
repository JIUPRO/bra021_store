namespace LojaVirtual.Dominio.Entidades
{
    public class Cliente : EntidadeBase
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Cpf { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? SenhaHash { get; set; }
        public bool EmailConfirmado { get; set; }
        public string? TokenConfirmacaoEmail { get; set; }
        
        // Endereço
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        
        // Relacionamentos
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
