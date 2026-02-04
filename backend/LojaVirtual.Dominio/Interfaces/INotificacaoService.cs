using LojaVirtual.Dominio.Entidades;

namespace LojaVirtual.Dominio.Interfaces
{
	public interface INotificacaoService
	{
		Task EnviarEmailNovaVendaAsync(Pedido pedido);
		Task EnviarWhatsAppNovaVendaAsync(Pedido pedido);
		Task EnviarEmailClienteNovaVendaAsync(Pedido pedido);
		Task EnviarEmailAlteracaoStatusAsync(Pedido pedido);
		Task EnviarEmailCancelamentoPedidoAsync(Pedido pedido);
		Task EnviarEmailEstoqueBaixoAsync(Produto produto, int estoqueAtual);
		Task EnviarEmailNotaFiscalAsync(Pedido pedido);
	}
}
