using MailKit.Net.Smtp;
using MimeKit;
using LojaVirtual.Dominio.Entidades;
using LojaVirtual.Dominio.Enums;
using LojaVirtual.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LojaVirtual.Infraestrutura.Services
{
	public class NotificacaoService : INotificacaoService
	{
		private readonly IConfiguration _configuracao;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<NotificacaoService> _logger;

		public NotificacaoService(IConfiguration configuracao, IUnitOfWork unitOfWork, ILogger<NotificacaoService> logger)
		{
			_configuracao = configuracao;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task EnviarEmailNovaVendaAsync(Pedido pedido)
		{
			_logger.LogInformation("[Email] Iniciando envio de email de nova venda para Pedido {PedidoId}", pedido.NumeroPedido);

			var emailSettings = _configuracao.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) && ssl;
			var username = emailSettings["Username"] ?? "";
			var password = emailSettings["Password"] ?? "";
			var fromEmail = emailSettings["FromEmail"] ?? username;
			var toEmail = emailSettings["ToEmail"] ?? "";

			_logger.LogDebug("[Email] SMTP Config - Server: {SmtpServer}, Port: {Port}, SSL: {EnableSsl}", smtpServer, port, enableSsl);

			if (string.IsNullOrWhiteSpace(toEmail))
			{
				var parametro = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("EmailAdministrador");
				toEmail = parametro?.Valor ?? "";
				_logger.LogDebug("[Email] Email do administrador obtido do parâmetro do sistema: {Email}", toEmail);
			}

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				_logger.LogWarning("[Email] Configurações de email incompletas (Username ou Password vazios). Email não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			if (string.IsNullOrWhiteSpace(toEmail))
			{
				_logger.LogWarning("[Email] Email do administrador não definido. Email não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			var mensagem = new MimeMessage();
			mensagem.From.Add(new MailboxAddress("Loja Brazil-021 School of Jiu-Jitsu", fromEmail));
			mensagem.To.Add(new MailboxAddress("Administrador", toEmail));
			mensagem.Subject = $"Nova Venda - Pedido {pedido.NumeroPedido}";

			var corpo = new StringBuilder();
			corpo.AppendLine("<h2>Nova Venda Realizada!</h2>");
			corpo.AppendLine($"<p><strong>Número do Pedido:</strong> {pedido.NumeroPedido}</p>");
			corpo.AppendLine($"<p><strong>Data:</strong> {pedido.DataPedido:dd/MM/yyyy HH:mm}</p>");
			corpo.AppendLine($"<p><strong>Cliente:</strong> {pedido.NomeEntrega}</p>");
			corpo.AppendLine($"<p><strong>Telefone:</strong> {pedido.TelefoneEntrega}</p>");
			corpo.AppendLine($"<p><strong>Valor Total:</strong> R$ {pedido.ValorTotal:N2}</p>");
			corpo.AppendLine("<h3>Itens do Pedido:</h3>");
			corpo.AppendLine("<ul>");

			foreach (var item in pedido.Itens)
			{
				corpo.AppendLine($"<li>{item.Produto.Nome} - Qtd: {item.Quantidade} - R$ {item.ValorTotal:N2}</li>");
			}

			corpo.AppendLine("</ul>");
			corpo.AppendLine("<h3>Endereço de Entrega:</h3>");
			corpo.AppendLine($"<p>{pedido.LogradouroEntrega}, {pedido.NumeroEntrega}</p>");
			if (!string.IsNullOrEmpty(pedido.ComplementoEntrega))
				corpo.AppendLine($"<p>{pedido.ComplementoEntrega}</p>");
			corpo.AppendLine($"<p>{pedido.BairroEntrega} - {pedido.CidadeEntrega}/{pedido.EstadoEntrega}</p>");
			corpo.AppendLine($"<p>CEP: {pedido.CepEntrega}</p>");

			mensagem.Body = new TextPart("html")
			{
				Text = corpo.ToString()
			};

			try
			{
				using var cliente = new SmtpClient();
				var secureSocketOptions = enableSsl
					? MailKit.Security.SecureSocketOptions.StartTls
					: MailKit.Security.SecureSocketOptions.Auto;

				_logger.LogDebug("[Email] Conectando ao servidor SMTP {SmtpServer}:{Port}", smtpServer, port);
				await cliente.ConnectAsync(smtpServer, port, secureSocketOptions);

				_logger.LogDebug("[Email] Autenticando no servidor SMTP com usuário {Username}", username);
				await cliente.AuthenticateAsync(username, password);

				_logger.LogDebug("[Email] Enviando email para {Email}", toEmail);
				await cliente.SendAsync(mensagem);
				await cliente.DisconnectAsync(true);

				_logger.LogInformation("[Email] ✓ Email de nova venda enviado com SUCESSO para {Email} (Pedido: {PedidoId})", toEmail, pedido.NumeroPedido);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Email] ✗ ERRO ao enviar email de nova venda para {Email} (Pedido: {PedidoId}). Tipo: {ExceptionType}, Mensagem: {ErrorMessage}",
					toEmail, pedido.NumeroPedido, ex.GetType().Name, ex.Message);
			}
		}

		public async Task EnviarWhatsAppNovaVendaAsync(Pedido pedido)
		{
			var whatsappSettings = _configuracao.GetSection("WhatsAppSettings");
			var apiUrl = whatsappSettings["ApiUrl"] ?? "";
			var apiKey = whatsappSettings["ApiKey"] ?? "";
			var phoneNumber = whatsappSettings["PhoneNumber"] ?? "";

			if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(phoneNumber))
			{
				_logger.LogInformation("[WhatsApp] Configurações de WhatsApp não definidas. [SIMULAÇÃO]: Nova venda - Pedido {PedidoId} - Cliente: {Cliente} - Valor: R$ {Valor:N2}",
					pedido.NumeroPedido, pedido.NomeEntrega, pedido.ValorTotal);
				return;
			}

			var mensagem = $"🛒 *NOVA VENDA*\n\n" +
							  $"Pedido: *{pedido.NumeroPedido}*\n" +
							  $"Cliente: {pedido.NomeEntrega}\n" +
							  $"Valor: *R$ {pedido.ValorTotal:N2}*\n" +
							  $"Itens: {pedido.Itens.Count}\n\n" +
							  $"Acesse o backoffice para mais detalhes.";

			try
			{
				using var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

				var conteudo = new StringContent(
					 $"{{\"to\":\"{phoneNumber}\",\"body\":\"{mensagem}\"}}",
					 System.Text.Encoding.UTF8,
					 "application/json");

				_logger.LogDebug("[WhatsApp] Enviando mensagem para {PhoneNumber}", phoneNumber);
				var resposta = await httpClient.PostAsync(apiUrl, conteudo);

				if (resposta.IsSuccessStatusCode)
				{
					_logger.LogInformation("[WhatsApp] ✓ Mensagem WhatsApp enviada com SUCESSO para Pedido {PedidoId}", pedido.NumeroPedido);
				}
				else
				{
					_logger.LogWarning("[WhatsApp] ✗ Erro ao enviar WhatsApp para Pedido {PedidoId}: StatusCode {StatusCode}", pedido.NumeroPedido, resposta.StatusCode);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[WhatsApp] ✗ ERRO ao enviar WhatsApp para Pedido {PedidoId}: {ErrorMessage}", pedido.NumeroPedido, ex.Message);
			}
		}

		public async Task EnviarEmailEstoqueBaixoAsync(Produto produto, int estoqueAtual)
		{
			_logger.LogInformation("[Email] Enviando alerta de estoque baixo para produto {Produto}", produto.Nome);

			var emailSettings = _configuracao.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) && ssl;
			var username = emailSettings["Username"] ?? "";
			var password = emailSettings["Password"] ?? "";
			var fromEmail = emailSettings["FromEmail"] ?? username;
			var toEmail = emailSettings["ToEmail"] ?? "";

			if (string.IsNullOrWhiteSpace(toEmail))
			{
				var parametro = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("EmailAdministrador");
				toEmail = parametro?.Valor ?? "";
			}

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				_logger.LogWarning("[Email] Configurações de email incompletas. Alerta de estoque baixo não enviado para {Produto}", produto.Nome);
				return;
			}

			if (string.IsNullOrWhiteSpace(toEmail))
			{
				_logger.LogWarning("[Email] Email do administrador não definido. Alerta de estoque baixo não enviado para {Produto}", produto.Nome);
				return;
			}

			var mensagem = new MimeMessage();
			mensagem.From.Add(new MailboxAddress("Loja Brazil-021 School of Jiu-Jitsu", fromEmail));
			mensagem.To.Add(new MailboxAddress("Administrador", toEmail));
			mensagem.Subject = $"Alerta de Estoque Baixo - {produto.Nome}";

			var corpo = new StringBuilder();
			corpo.AppendLine("<h2>Alerta de Estoque Baixo!</h2>");
			corpo.AppendLine($"<p>O produto <strong>{produto.Nome}</strong> está com estoque baixo.</p>");
			corpo.AppendLine($"<p><strong>Estoque Atual:</strong> {estoqueAtual}</p>");
			corpo.AppendLine($"<p><strong>Estoque Mínimo:</strong> {produto.QuantidadeMinimaEstoque}</p>");
			corpo.AppendLine($"<p><strong>Nome:</strong> {produto.Nome}</p>");
			corpo.AppendLine("<p>Por favor, realize a reposição do produto o quanto antes.</p>");

			mensagem.Body = new TextPart("html")
			{
				Text = corpo.ToString()
			};

			try
			{
				using var cliente = new SmtpClient();
				var secureSocketOptions = enableSsl
					? MailKit.Security.SecureSocketOptions.StartTls
					: MailKit.Security.SecureSocketOptions.Auto;
				await cliente.ConnectAsync(smtpServer, port, secureSocketOptions);
				await cliente.AuthenticateAsync(username, password);
				await cliente.SendAsync(mensagem);
				await cliente.DisconnectAsync(true);

				_logger.LogInformation("[Email] ✓ Alerta de estoque enviado com SUCESSO para {Email} (Produto: {Produto})", toEmail, produto.Nome);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Email] ✗ ERRO ao enviar alerta de estoque para {Email} (Produto: {Produto}): {ErrorMessage}", toEmail, produto.Nome, ex.Message);
			}
		}

		public async Task EnviarEmailClienteNovaVendaAsync(Pedido pedido)
		{
			_logger.LogInformation("[Email] Enviando email de confirmação para cliente do Pedido {PedidoId}", pedido.NumeroPedido);

			var emailSettings = _configuracao.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) && ssl;
			var username = emailSettings["Username"] ?? "";
			var password = emailSettings["Password"] ?? "";
			var fromEmail = emailSettings["FromEmail"] ?? username;
			var clienteEmail = pedido.Cliente?.Email ?? "";

			_logger.LogDebug("[Email] Email do cliente obtido: {Email}", clienteEmail);

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				_logger.LogWarning("[Email] Configurações de email incompletas. Email de confirmação não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			if (string.IsNullOrWhiteSpace(clienteEmail))
			{
				_logger.LogWarning("[Email] Email do cliente não definido. Email de confirmação não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			var mensagem = new MimeMessage();
			mensagem.From.Add(new MailboxAddress("Loja Brazil-021 School of Jiu-Jitsu", fromEmail));
			mensagem.To.Add(new MailboxAddress(pedido.NomeEntrega, clienteEmail));
			mensagem.Subject = $"Pedido Confirmado - #{pedido.NumeroPedido}";

			var corpo = new StringBuilder();
			corpo.AppendLine("<h2>Seu Pedido foi Confirmado! 🎉</h2>");
			corpo.AppendLine($"<p>Obrigado por sua compra!</p>");
			corpo.AppendLine($"<p><strong>Número do Pedido:</strong> {pedido.NumeroPedido}</p>");
			corpo.AppendLine($"<p><strong>Data do Pedido:</strong> {pedido.DataPedido:dd/MM/yyyy HH:mm}</p>");
			corpo.AppendLine($"<p><strong>Status:</strong> {ObterStatusPorExtenso(pedido.Status)}</p>");
			corpo.AppendLine("<h3>Itens do Pedido:</h3>");
			corpo.AppendLine("<ul>");

			foreach (var item in pedido.Itens)
			{
				corpo.AppendLine($"<li>{item.Produto.Nome} - Qtd: {item.Quantidade} - R$ {item.ValorTotal:N2}</li>");
			}

			corpo.AppendLine("</ul>");
			corpo.AppendLine("<h3>Resumo do Pedido:</h3>");
			corpo.AppendLine($"<p><strong>Subtotal:</strong> R$ {pedido.ValorSubtotal:N2}</p>");
			if (pedido.ValorDesconto > 0)
				corpo.AppendLine($"<p><strong>Desconto:</strong> -R$ {pedido.ValorDesconto:N2}</p>");
			corpo.AppendLine($"<p><strong>Frete:</strong> R$ {pedido.ValorFrete:N2}</p>");
			corpo.AppendLine($"<p><strong style='font-size: 18px;'>Valor Total: R$ {pedido.ValorTotal:N2}</strong></p>");
			corpo.AppendLine("<h3>Endereço de Entrega:</h3>");
			corpo.AppendLine($"<p>{pedido.LogradouroEntrega}, {pedido.NumeroEntrega}</p>");
			if (!string.IsNullOrEmpty(pedido.ComplementoEntrega))
				corpo.AppendLine($"<p>{pedido.ComplementoEntrega}</p>");
			corpo.AppendLine($"<p>{pedido.BairroEntrega} - {pedido.CidadeEntrega}/{pedido.EstadoEntrega}</p>");
			corpo.AppendLine($"<p>CEP: {pedido.CepEntrega}</p>");
			corpo.AppendLine($"<p><strong>Contato:</strong> {pedido.TelefoneEntrega}</p>");
			corpo.AppendLine("<p>Você receberá atualizações sobre o status do seu pedido em breve.</p>");

			mensagem.Body = new TextPart("html")
			{
				Text = corpo.ToString()
			};

			try
			{
				using var cliente = new SmtpClient();
				var secureSocketOptions = enableSsl
					? MailKit.Security.SecureSocketOptions.StartTls
					: MailKit.Security.SecureSocketOptions.Auto;
				await cliente.ConnectAsync(smtpServer, port, secureSocketOptions);
				await cliente.AuthenticateAsync(username, password);
				await cliente.SendAsync(mensagem);
				await cliente.DisconnectAsync(true);

				_logger.LogInformation("[Email] ✓ Email de confirmação enviado com SUCESSO para {Email} (Pedido: {PedidoId})", clienteEmail, pedido.NumeroPedido);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Email] ✗ ERRO ao enviar email de confirmação para {Email} (Pedido: {PedidoId}): {ErrorMessage}", clienteEmail, pedido.NumeroPedido, ex.Message);
			}
		}

		public async Task EnviarEmailAlteracaoStatusAsync(Pedido pedido)
		{
			_logger.LogInformation("[Email] Enviando email de mudança de status para cliente do Pedido {PedidoId}", pedido.NumeroPedido);

			var emailSettings = _configuracao.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) && ssl;
			var username = emailSettings["Username"] ?? "";
			var password = emailSettings["Password"] ?? "";
			var fromEmail = emailSettings["FromEmail"] ?? username;
			var clienteEmail = pedido.Cliente?.Email ?? "";

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				_logger.LogWarning("[Email] Configurações de email incompletas. Email de mudança de status não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			if (string.IsNullOrWhiteSpace(clienteEmail))
			{
				_logger.LogWarning("[Email] Email do cliente não definido. Email de mudança de status não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			var statusExtenso = ObterStatusPorExtenso(pedido.Status);
			var mensagem = new MimeMessage();
			mensagem.From.Add(new MailboxAddress("Loja Brazil-021 School of Jiu-Jitsu", fromEmail));
			mensagem.To.Add(new MailboxAddress(pedido.NomeEntrega, clienteEmail));
			mensagem.Subject = $"Pedido #{pedido.NumeroPedido} - Status Atualizado";

			var corpo = new StringBuilder();
			corpo.AppendLine("<h2>Atualização de Status do Seu Pedido 📦</h2>");
			corpo.AppendLine($"<p>Seu pedido <strong>#{pedido.NumeroPedido}</strong> foi atualizado!</p>");
			corpo.AppendLine($"<p><strong>Novo Status:</strong> <span style='color: #007bff; font-weight: bold;'>{statusExtenso}</span></p>");
			corpo.AppendLine($"<p><strong>Data da Atualização:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>");
			corpo.AppendLine("<h3>Detalhes do Pedido:</h3>");
			corpo.AppendLine($"<p><strong>Valor Total:</strong> R$ {pedido.ValorTotal:N2}</p>");
			corpo.AppendLine($"<p><strong>Endereço:</strong> {pedido.LogradouroEntrega}, {pedido.NumeroEntrega} - {pedido.BairroEntrega}, {pedido.CidadeEntrega}/{pedido.EstadoEntrega}</p>");
			corpo.AppendLine("<p>Qualquer dúvida, entre em contato conosco!</p>");

			mensagem.Body = new TextPart("html")
			{
				Text = corpo.ToString()
			};

			try
			{
				using var cliente = new SmtpClient();
				var secureSocketOptions = enableSsl
					? MailKit.Security.SecureSocketOptions.StartTls
					: MailKit.Security.SecureSocketOptions.Auto;
				await cliente.ConnectAsync(smtpServer, port, secureSocketOptions);
				await cliente.AuthenticateAsync(username, password);
				await cliente.SendAsync(mensagem);
				await cliente.DisconnectAsync(true);

				_logger.LogInformation("[Email] ✓ Email de mudança de status enviado com SUCESSO para {Email} (Pedido: {PedidoId}, Novo Status: {Status})",
					clienteEmail, pedido.NumeroPedido, statusExtenso);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Email] ✗ ERRO ao enviar email de mudança de status para {Email} (Pedido: {PedidoId}): {ErrorMessage}", clienteEmail, pedido.NumeroPedido, ex.Message);
			}
		}

		public async Task EnviarEmailCancelamentoPedidoAsync(Pedido pedido)
		{
			_logger.LogInformation("[Email] Enviando email de cancelamento para admin do Pedido {PedidoId}", pedido.NumeroPedido);

			var emailSettings = _configuracao.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) && ssl;
			var username = emailSettings["Username"] ?? "";
			var password = emailSettings["Password"] ?? "";
			var fromEmail = emailSettings["FromEmail"] ?? username;
			var adminEmail = emailSettings["ToEmail"] ?? "";

			if (string.IsNullOrWhiteSpace(adminEmail))
			{
				var parametro = await _unitOfWork.ParametrosSistema.ObterPorChaveAsync("EmailAdministrador");
				adminEmail = parametro?.Valor ?? "";
				_logger.LogDebug("[Email] Email do administrador obtido do parâmetro do sistema: {Email}", adminEmail);
			}

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				_logger.LogWarning("[Email] Configurações de email incompletas. Email de cancelamento não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			if (string.IsNullOrWhiteSpace(adminEmail))
			{
				_logger.LogWarning("[Email] Email do administrador (ToEmail) não definido. Email de cancelamento não enviado para Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			var mensagem = new MimeMessage();
			mensagem.From.Add(new MailboxAddress("Loja Brazil-021 School of Jiu-Jitsu", fromEmail));
			mensagem.To.Add(new MailboxAddress("Administrador", adminEmail));
			mensagem.Subject = $"Pedido #{pedido.NumeroPedido} - Cancelado";

			var corpo = new StringBuilder();
			corpo.AppendLine("<h2>Pedido Cancelado ⚠️</h2>");
			corpo.AppendLine($"<p>O pedido <strong>#{pedido.NumeroPedido}</strong> foi cancelado.</p>");
			corpo.AppendLine($"<p><strong>PagamentoId:</strong> {pedido.PagamentoId ?? "(não informado)"}</p>");
			corpo.AppendLine("<p>Verifique o estorno do pagamento correspondente.</p>");

			mensagem.Body = new TextPart("html")
			{
				Text = corpo.ToString()
			};

			try
			{
				using var cliente = new SmtpClient();
				var secureSocketOptions = enableSsl
					? MailKit.Security.SecureSocketOptions.StartTls
					: MailKit.Security.SecureSocketOptions.Auto;
				await cliente.ConnectAsync(smtpServer, port, secureSocketOptions);
				await cliente.AuthenticateAsync(username, password);
				await cliente.SendAsync(mensagem);
				await cliente.DisconnectAsync(true);

				_logger.LogInformation("[Email] ✓ Email de cancelamento enviado com SUCESSO para admin (Pedido: {PedidoId})", pedido.NumeroPedido);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Email] ✗ ERRO ao enviar email de cancelamento para admin (Pedido: {PedidoId}): {ErrorMessage}", pedido.NumeroPedido, ex.Message);
			}
		}

		public async Task EnviarEmailNotaFiscalAsync(Pedido pedido)
		{
			_logger.LogInformation("[Email] Iniciando envio de email de nota fiscal para Cliente (Pedido: {PedidoId})", pedido.NumeroPedido);

			var emailSettings = _configuracao.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
			var port = int.Parse(emailSettings["Port"] ?? "587");
			var enableSsl = bool.TryParse(emailSettings["EnableSsl"], out var ssl) && ssl;
			var username = emailSettings["Username"] ?? "";
			var password = emailSettings["Password"] ?? "";
			var fromEmail = emailSettings["FromEmail"] ?? username;

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				_logger.LogWarning("[Email] Configurações de email incompletas. Email não enviado.");
				return;
			}

			if (pedido.Cliente == null)
			{
				_logger.LogWarning("[Email] Cliente não encontrado para o Pedido {PedidoId}", pedido.NumeroPedido);
				return;
			}

			var clienteEmail = pedido.Cliente.Email;
			if (string.IsNullOrEmpty(clienteEmail))
			{
				_logger.LogWarning("[Email] Cliente sem email cadastrado (Pedido: {PedidoId})", pedido.NumeroPedido);
				return;
			}

			var mensagem = new MimeMessage();
			mensagem.From.Add(new MailboxAddress("Loja Brazil-021 School of Jiu-Jitsu", fromEmail));
			mensagem.To.Add(new MailboxAddress(pedido.Cliente.Nome, clienteEmail));
			mensagem.Subject = $"Nota Fiscal Disponível - Pedido #{pedido.NumeroPedido}";

			var corpo = new StringBuilder();
			corpo.AppendLine("<h2>Nota Fiscal Disponível 📄</h2>");
			corpo.AppendLine($"<p>Olá <strong>{pedido.Cliente.Nome}</strong>,</p>");
			corpo.AppendLine($"<p>A nota fiscal do seu pedido <strong>#{pedido.NumeroPedido}</strong> está disponível!</p>");
			corpo.AppendLine($"<p><strong>Data do Pedido:</strong> {pedido.DataPedido:dd/MM/yyyy HH:mm}</p>");
			corpo.AppendLine($"<p><strong>Valor Total:</strong> R$ {pedido.ValorTotal:F2}</p>");
			corpo.AppendLine("<br/>");
			corpo.AppendLine($"<p><a href=\"{pedido.NotaFiscalUrl}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;\">Baixar Nota Fiscal</a></p>");
			corpo.AppendLine("<br/>");
			corpo.AppendLine("<p>Você também pode visualizar a nota fiscal acessando os detalhes do seu pedido em nossa loja.</p>");
			corpo.AppendLine("<hr/>");
			corpo.AppendLine("<p style=\"font-size: 12px; color: #666;\">Atenciosamente,<br/>Brazil-021 School of Jiu-Jitsu</p>");

			mensagem.Body = new TextPart("html")
			{
				Text = corpo.ToString()
			};

			try
			{
				using var cliente = new SmtpClient();
				var secureSocketOptions = enableSsl
					? MailKit.Security.SecureSocketOptions.StartTls
					: MailKit.Security.SecureSocketOptions.Auto;
				await cliente.ConnectAsync(smtpServer, port, secureSocketOptions);
				await cliente.AuthenticateAsync(username, password);
				await cliente.SendAsync(mensagem);
				await cliente.DisconnectAsync(true);

				_logger.LogInformation("[Email] ✓ Email de nota fiscal enviado com SUCESSO para {ClienteEmail} (Pedido: {PedidoId})", clienteEmail, pedido.NumeroPedido);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Email] ✗ ERRO ao enviar email de nota fiscal (Pedido: {PedidoId}): {ErrorMessage}", pedido.NumeroPedido, ex.Message);
			}
		}

		private string ObterStatusPorExtenso(StatusPedido status)
		{
			return status switch
			{
				StatusPedido.Pendente => "Pendente",
				StatusPedido.AguardandoPagamento => "Aguardando Pagamento",
				StatusPedido.Pago => "Pago",
				StatusPedido.EmSeparacao => "Em Separação",
				StatusPedido.Enviado => "Enviado",
				StatusPedido.Entregue => "Entregue",
				StatusPedido.Cancelado => "Cancelado",
				_ => "Desconhecido"
			};
		}
	}
}
