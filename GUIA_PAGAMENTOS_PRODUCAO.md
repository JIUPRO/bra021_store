# 🚀 GUIA: Testar Pagamentos REAIS em Produção (Cartão + PIX)

## 📋 SUMÁRIO
1. Preparação da conta Mercado Pago
2. Configuração do Webhook
3. Deploy em produção
4. Testes com pagamentos de verdade
5. Troubleshooting

---

## 1️⃣ PREPARAÇÃO - Conta Mercado Pago

### A) Upgrade para Produção
1. Acesse: https://www.mercadopago.com.br
2. Faça login com sua conta
3. Vá para **Configurações** → **Conta** → **Informações da Conta**
4. Clique em **"Ativar vendas em produção"**
5. Complete o cadastro (CNPJ/CPF, dados bancários, etc)
6. Aguarde aprovação (geralmente 1-2 dias úteis)

### B) Gerar Credenciais de Produção
1. Após aprovação, vá para: https://www.mercadopago.com.br/developers/panel
2. Clique em **"Suas integrações"** → **"Credenciais de produção"**
3. Copie:
   - **Access Token** (começa com `APP_...`)
   - **Public Key** (começa com `APP_...`)
4. Essas são as credenciais REAIS para cobrar dinheiro

### C) Diferença Credenciais
```
TESTE (Sandbox):
- Access Token: TEST-1234567890...
- Public Key: TEST-abcdefgh...
- Usa cartões de teste (4111111111111111)
- PIX fake (não processa de verdade)

PRODUÇÃO (Live):
- Access Token: APP_abcdefgh1234...
- Public Key: APP_1234567890abc...
- Cartões REAIS (cobra mesmo)
- PIX REAL (transferência verdadeira)
```

---

## 2️⃣ WEBHOOK - Como funciona?

### O que é Webhook?
É uma URL que você fornece ao Mercado Pago. Quando algo acontece (pagamento aprovado, PIX recebido, etc), o Mercado Pago **ENVIA UMA REQUISIÇÃO** para essa URL avisando.

```
Timeline do PIX:
┌──────────────────────────────────────────────────────┐
│ 1. Cliente scanneia QR Code do PIX                   │
│ 2. Cliente confirma transferência no app do banco     │
│ 3. Banco credita em 30 segundos                      │
│ 4. Mercado Pago recebe confirmação                   │
│ 5. Mercado Pago **envia POST para seu WEBHOOK**      │
│ 6. Seu backend marca pedido como PAGO                │
└──────────────────────────────────────────────────────┘
```

### A) Registrar seu Webhook

**URL Webhook (seu backend):**
```
https://seu-dominio.com/api/pagamentos/webhook
```

**Como registrar:**
1. Vá para: https://www.mercadopago.com.br/developers/panel
2. **Notificações** → **URL de Webhook**
3. Cole sua URL: `https://seu-dominio.com/api/pagamentos/webhook`
4. Selecione eventos:
   - ✅ `payment.created`
   - ✅ `payment.updated`
5. Clique em **"Salvar"**

### B) O que você recebe no Webhook

**POST** para `https://seu-dominio.com/api/pagamentos/webhook`

```json
{
  "id": 1234567890,
  "type": "payment",
  "action": "payment.updated",
  "data": {
    "id": "1234567890"
  }
}
```

Não vem os dados completos! Você precisa:
1. Pegar o `data.id`
2. Fazer uma **nova requisição** para Mercado Pago pedindo os detalhes
3. Verificar o `status` do pagamento

---

## 3️⃣ DEPLOY EM PRODUÇÃO

### A) Atualizar Variáveis de Ambiente

**Em Portainer** (seu container):

```
MERCADO_PAGO_ACCESS_TOKEN = APP_1234567890abcdefgh_PROD
MERCADO_PAGO_PUBLIC_KEY = APP_abcdefgh1234567890_PROD
WEBHOOK_SECRET = seu-secret-aleatorio-seguro
```

### B) Gerar e Registrar WEBHOOK_SECRET

```bash
# Gere uma string aleatória segura
openssl rand -hex 32
# Output: a3b2c1d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0

# Use como WEBHOOK_SECRET
```

### C) Deploy Docker

```bash
cd /path/to/LojaBrazil021
docker-compose down
docker-compose up -d --build
```

Agora o backend está usando credenciais REAIS!

---

## 4️⃣ TESTES COM PAGAMENTOS REAIS

### ⚠️ AVISO IMPORTANTE
- Cada teste cobra **DINHEIRO REAL**
- Cartões podem rejeitar por segurança (normal)
- Tenha pelo menos R$ 50-100 disponíveis para testes

### A) Teste 1: Cartão de Crédito

**Frontend:**
1. Acesse: `https://seu-dominio.com`
2. Adicione produtos ao carrinho
3. Vá para checkout
4. Clique em "Pagar com Cartão"

**Use um cartão de teste:**
```
Número: 4539 0000 0000 9903
CPF: 123.456.789-09
Vencimento: 12/25
CVV: 123
Titulal: TESTE
```

**Backend verifica:**
- Comunica com Mercado Pago (credenciais REAIS)
- Se aprovado → Status `approved`
- Webhook é acionado
- Pedido marca como `Pago`

### B) Teste 2: PIX

**Frontend:**
1. Carrinho + Checkout
2. Clique em "Pagar com PIX"
3. Vai aparecer QR Code

**Terminal/Celular:**
1. Abra seu app de banco (Nubank, Itaú, etc)
2. Faça PIX por QR Code (valor real)
3. Confirme a transferência

**Backend verifica:**
- Cria ordem PIX no Mercado Pago
- Retorna QR Code ao frontend
- Frontend exibe QR Code para cliente
- Cliente transfere

**Webhook recebe:**
1. Mercado Pago nota o PIX (em 30 seg)
2. POST para seu webhook
3. Backend consulta status
4. Atualiza para `Pago`
5. Notificação ao cliente

---

## 5️⃣ CONFIGURAR WEBHOOK NO CÓDIGO

### Seu endpoint já existe?

Verifique em:
```
backend/LojaVirtual.API/Controllers/PagamentosController.cs
```

Se não tiver, crie:

```csharp
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> Webhook([FromBody] dynamic notification)
{
    try
    {
        _logger.LogInformation("Webhook recebido: {Notification}", notification);
        
        // Extrair ID do pagamento
        var paymentId = notification.data.id;
        
        // Consultar status no Mercado Pago
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        
        var response = await client.GetAsync($"https://api.mercadopago.com/v1/payments/{paymentId}");
        var content = await response.Content.ReadAsStringAsync();
        var paymentData = JsonConvert.DeserializeObject<dynamic>(content);
        
        var status = paymentData.status; // "approved", "pending", "rejected", etc
        var externalReference = paymentData.external_reference; // Seu PedidoId
        
        if (status == "approved")
        {
            // Marcar pedido como PAGO
            var pedido = await _unitOfWork.Pedidos.ObterPorIdAsync(Guid.Parse(externalReference));
            if (pedido != null)
            {
                pedido.StatusPagamento = StatusPagamento.Pago;
                await _unitOfWork.SalvarMudancasAsync();
                
                _logger.LogInformation("✅ Pedido {PedidoId} marcado como PAGO via webhook", externalReference);
            }
        }
        
        return Ok();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao processar webhook");
        return StatusCode(500);
    }
}
```

---

## 6️⃣ TESTES IMPORTANTES

### ✅ Teste Completo 1: Cartão Aprovado
1. Use cartão de teste com flag `approved`
2. Clique em pagar
3. Verifique em Portainer se webhook foi acionado
4. Verifique no banco se pedido está `Pago`

**Comando para ver logs:**
```bash
docker-compose logs backend -f
```

### ✅ Teste Completo 2: PIX Real
1. Crie pedido com 10 reais
2. Escolha PIX
3. Escaneie QR Code com seu celular
4. Transfira de verdade
5. Aguarde webhook (30 segundos)
6. Verifique se pedido marcou como `Pago`

### ✅ Teste 3: Cancelamento
1. Se tiver botão de cancelamento, teste
2. Verifique se webhook de `payment.rejected` chega
3. Verifique se pedido marca como `Cancelado`

### ✅ Teste 4: Múltiplos Pagamentos
1. Crie 3-5 pedidos
2. Pague cada um com PIX
3. Observe se todos atualizam via webhook
4. Verifique inconsistências

---

## 7️⃣ POSSÍVEIS PROBLEMAS

### ❌ "Webhook não está chegando"

**Causas:**
1. URL webhook incorreta
2. Firewall bloqueando Mercado Pago (AWS IP)
3. Aplicação retornando erro 500
4. Webhook não registrado em Mercado Pago

**Solução:**
```bash
# Veja os logs
docker-compose logs backend -f

# Teste manualmente o webhook
curl -X POST http://localhost:5000/api/pagamentos/webhook \
  -H "Content-Type: application/json" \
  -d '{"data":{"id":"123"}}'

# Verifique se Mercado Pago consegue alcançar seu domínio
# Use ferramentas como: https://webhook.site para testar
```

### ❌ "Está cobrando mas não marca como pago"

**Causa:** Webhook não está atualizando o banco

**Verifique:**
1. Logs do backend (`docker-compose logs backend`)
2. Se `StatusPagamento` está sendo atualizado
3. Se `PedidoId` corresponde ao `external_reference`

### ❌ "Cartão foi rejeitado"

**Normal!** Mercado Pago pode rejeitar por:
- Limite insuficiente
- CVV/CPF errado
- Banco rejeitando (fraude)
- Testando segurança

Tente novamente ou use outro cartão.

### ❌ "PIX não está funcionando"

**Verificações:**
1. Sua conta tem PIX habilitado? (Verificar em Mercado Pago)
2. CPF/CNPJ está correto?
3. Chave PIX registrada em sua conta?

---

## 8️⃣ MONITORAMENTO EM PRODUÇÃO

### Dashboard Mercado Pago
1. Acesse: https://www.mercadopago.com.br/activities
2. Veja todos os pagamentos recebidos
3. Status, valores, datas

### Seu Backend
```bash
# Ver pagamentos no seu banco
SELECT * FROM Pagamentos WHERE DataPagamento > DATEADD(day,-1,GETDATE())
ORDER BY DataPagamento DESC

# Ver webhooks processados
SELECT * FROM [LogPagamentos] WHERE DataLog > DATEADD(hour,-1,GETDATE())
```

### Alertas Recomendados
- [ ] Email quando pagamento é aprovado
- [ ] Notificação se webhook falha 3x
- [ ] Alert se PIX não processa em 60 segundos
- [ ] Dashboard com vendas do dia

---

## 📱 RESUMO RÁPIDO

```
┌─────────────────────────────────────────┐
│ 1. Upgrade conta Mercado Pago           │
│ 2. Gerar credenciais de PRODUÇÃO        │
│ 3. Registrar WEBHOOK URL                │
│ 4. Atualizar env vars (ACCESS_TOKEN)    │
│ 5. Deploy docker-compose up -d --build  │
│ 6. Testar cartão (real)                 │
│ 7. Testar PIX (real)                    │
│ 8. Monitorar logs & dashboard           │
└─────────────────────────────────────────┘
```

---

## 🎯 PRÓXIMOS PASSOS

- [ ] Completar upgrade conta Mercado Pago
- [ ] Gerar credenciais produção
- [ ] Registrar webhook
- [ ] Atualizar variáveis em Portainer
- [ ] Fazer primeiro teste com cartão
- [ ] Fazer primeiro teste com PIX
- [ ] Configurar alertas por email
- [ ] Documentar fluxo para suporte

---

**Precisa de ajuda em alguma etapa? Me avisa!** 🚀
