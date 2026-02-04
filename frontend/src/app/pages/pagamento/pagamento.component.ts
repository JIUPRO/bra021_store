import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PagamentoService } from '../../services/pagamento.service';
import { AlertService } from '../../services/alert.service';
import { ParametroSistemaService } from '../../services/parametro-sistema.service';
import { PedidoService } from '../../services/pedido.service';
import { StatusPedido } from '../../models/pedido.model';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-pagamento',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-lg-8">
          <div class="text-center mb-4">
            <h1 class="fw-bold">
              <i class="bi bi-credit-card-2-front me-2"></i>Pagamento
            </h1>
            <p class="text-muted">Pedido #{{ numeroPedido }}</p>
          </div>

          <!-- Seleção de Forma de Pagamento -->
          <div class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0">Forma de Pagamento</h5>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <div 
                    class="payment-option" 
                    [class.selected]="metodoPagamento === 'pix'"
                    (click)="metodoPagamento = 'pix'"
                  >
                    <i class="bi bi-qr-code fs-1 text-primary"></i>
                    <h6 class="mt-2 mb-0">PIX</h6>
                    <small class="text-muted">Aprovação imediata</small>
                  </div>
                </div>
                <div class="col-md-6 mb-3">
                  <div 
                    class="payment-option" 
                    [class.selected]="metodoPagamento === 'credit_card'"
                    (click)="metodoPagamento = 'credit_card'"
                  >
                    <i class="bi bi-credit-card fs-1 text-success"></i>
                    <h6 class="mt-2 mb-0">Cartão de Crédito</h6>
                    <small class="text-muted">Em até {{ maxParcelas }}x sem juros</small>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Dados do Cartão (se selecionado) -->
          <div *ngIf="metodoPagamento === 'credit_card'" class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0">Dados do Cartão</h5>
            </div>
            <div class="card-body">
              <!-- Seletor de Parcelas -->
              <div class="mb-4 p-3 bg-light rounded">
                <label class="form-label fw-bold">
                  <i class="bi bi-credit-card me-2"></i>Parcelas (sem juros)
                </label>
                <div class="row g-2">
                    <div class="col-auto" *ngFor="let parcela of opcoesParcelamento">
                    <button
                      type="button"
                      class="btn w-100"
                      [class.btn-primary]="parcelas === parcela"
                      [class.btn-outline-primary]="parcelas !== parcela"
                      (click)="parcelas = parcela; atualizarValorParcela()"
                    >
                      <strong>{{ parcela }}x</strong><br>
                      <small>R$ {{ (valorTotal / parcela) | number:'1.2-2' }}</small>
                    </button>
                  </div>
                </div>
                <div class="alert alert-info mt-3 mb-0">
                  <small>
                    <i class="bi bi-info-circle me-1"></i>
                    Total: <strong>{{ parcelas }}x de R$ {{ valorParcelado | number:'1.2-2' }}</strong>
                  </small>
                </div>
              </div>
              <div class="mb-3">
                <label class="form-label">Número do Cartão</label>
                <input 
                  type="text" 
                  class="form-control" 
                  [(ngModel)]="dadosCartao.cardNumber"
                  placeholder="0000 0000 0000 0000"
                  maxlength="19"
                >
              </div>
              <div class="mb-3">
                <label class="form-label">Nome no Cartão</label>
                <input 
                  type="text" 
                  class="form-control" 
                  [(ngModel)]="dadosCartao.cardholderName"
                  placeholder="NOME COMO ESTÁ NO CARTÃO"
                >
              </div>
              <div class="row">
                <div class="col-md-4 mb-3">
                  <label class="form-label">Mês</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="dadosCartao.expirationMonth"
                    placeholder="MM"
                    maxlength="2"
                  >
                </div>
                <div class="col-md-4 mb-3">
                  <label class="form-label">Ano</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="dadosCartao.expirationYear"
                    placeholder="AA"
                    maxlength="2"
                  >
                </div>
                <div class="col-md-4 mb-3">
                  <label class="form-label">CVV</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="dadosCartao.securityCode"
                    placeholder="123"
                    maxlength="4"
                  >
                </div>
              </div>
              <div class="mb-3">
                <label class="form-label">CPF do Titular</label>
                <input 
                  type="text" 
                  class="form-control" 
                  [(ngModel)]="dadosCartao.cpf"
                  placeholder="000.000.000-00"
                  maxlength="14"
                >
              </div>
            </div>
          </div>

          <!-- Mensagem de Erro -->
          <div *ngIf="erroAtual" class="alert alert-danger alert-dismissible fade show mb-4" role="alert">
            <i class="bi bi-exclamation-triangle me-2"></i>
            <strong>Erro ao processar pagamento:</strong><br>
            {{ erroAtual.mensagem }}
            <small class="d-block mt-2" *ngIf="erroAtual.codigoErro">
              Código: {{ erroAtual.codigoErro }}
            </small>
            <button
              type="button"
              class="btn-close"
              (click)="erroAtual = null"
              aria-label="Fechar"
            ></button>
          </div>

          <!-- QR Code PIX (se selecionado) -->
          <div *ngIf="qrCodePix" class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0">Pague com PIX</h5>
            </div>
            <div class="card-body text-center">
              <img [src]="'data:image/png;base64,' + qrCodePix" alt="QR Code PIX" class="img-fluid mb-3" style="max-width: 300px;">
              <div class="alert alert-info">
                <i class="bi bi-info-circle me-2"></i>
                <strong>Instruções:</strong><br>
                1. Abra o app do seu banco<br>
                2. Escolha pagar com PIX<br>
                3. Escaneie o QR Code acima<br>
                4. Confirme o pagamento
              </div>
              <p class="text-muted small">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Verificando pagamento automaticamente...
              </p>
            </div>
          </div>

          <!-- Botão de Pagar -->
          <div class="text-center" *ngIf="!qrCodePix">
            <button 
              class="btn btn-primario btn-lg px-5" 
              (click)="processarPagamento()"
              [disabled]="processando || !metodoPagamento"
            >
              <span *ngIf="!processando">
                <i class="bi bi-check-circle me-2"></i>
                {{ metodoPagamento === 'pix' ? 'Gerar QR Code PIX' : 'Pagar com Cartão' }}
              </span>
              <span *ngIf="processando">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Processando...
              </span>
            </button>
            <br>
            <a routerLink="/produtos" class="btn btn-link mt-3">
              <i class="bi bi-arrow-left me-2"></i>Voltar para produtos
            </a>
          </div>

          <!-- Botão de Retentar Pagamento -->
          <div class="text-center" *ngIf="erroAtual && (statusPedido === 1 || statusPedido === 0)">
            <button 
              class="btn btn-warning btn-lg px-5" 
              (click)="abrirRetentativa()"
            >
              <i class="bi bi-arrow-clockwise me-2"></i>Tentar Novamente
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .payment-option {
      border: 2px solid #dee2e6;
      border-radius: 12px;
      padding: 24px;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .payment-option:hover {
      border-color: var(--cor-primaria);
      background-color: #f8f9fa;
      transform: translateY(-2px);
    }

    .payment-option.selected {
      border-color: var(--cor-primaria);
      background-color: rgba(251, 191, 36, 0.1);
      box-shadow: 0 4px 12px rgba(47, 106, 73, 0.15);
    }

    .btn-primario {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      border: none;
      border-radius: 8px;
      padding: 12px 32px;
      font-weight: 600;
      transition: all 0.3s ease;
      color: var(--cor-escura);
    }
    
    .btn-primario:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(47,106,73,0.18);
      color: var(--cor-escura);
    }
    
    .btn-primario:disabled {
      background: var(--cor-secundaria);
      cursor: not-allowed;
      opacity: 0.6;
    }
    
    .spinning {
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class PagamentoComponent implements OnInit, OnDestroy {
  pedidoId: string = '';
  numeroPedido: string = '';
  metodoPagamento: string = '';
  processando: boolean = false;
  qrCodePix: string = '';
  valorTotal: number = 0;
  parcelas: number = 1;
  valorParcelado: number = 0;
  statusPedido: number = 0;
  erroAtual: { mensagem: string; codigoErro?: string } | null = null;
  maxParcelas: number = 3;
  opcoesParcelamento: number[] = [1, 2, 3];
  
  // Verificação automática do pagamento PIX
  private verificandoPagamento: any = null;
  private tentativasVerificacao: number = 0;
  private maxTentativas: number = 60; // 5 minutos (60 * 5s)

  dadosCartao = {
    cardNumber: '',
    cardholderName: '',
    expirationMonth: '',
    expirationYear: '',
    securityCode: '',
    cpf: ''
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pagamentoService: PagamentoService,
    private alertService: AlertService,
    private parametroSistemaService: ParametroSistemaService,
    private pedidoService: PedidoService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.pedidoId = params['pedidoId'] || '';
      this.numeroPedido = params['numero'] || '';
      this.valorTotal = parseFloat(params['total']) || 0;
      this.statusPedido = parseInt(params['status']) || 0;

      if (!this.pedidoId) {
        this.alertService.error('Erro', 'Pedido não encontrado');
        this.router.navigate(['/produtos']);
        return;
      }

      this.atualizarValorParcela();
    });

    this.carregarMaximoParcelas();
  }

  carregarMaximoParcelas(): void {
    this.parametroSistemaService.obterPorChave('MaximoParcelas').subscribe({
      next: (parametro) => {
        const max = parseInt(parametro?.valor || '3');
        this.maxParcelas = max;
        this.opcoesParcelamento = Array.from({ length: max }, (_, i) => i + 1);
      },
      error: () => {
        this.maxParcelas = 3;
        this.opcoesParcelamento = [1, 2, 3];
      }
    });
  }

  atualizarValorParcela(): void {
    this.valorParcelado = this.valorTotal / (this.parcelas || 1);
  }

  async processarPagamento(): Promise<void> {
    if (!this.metodoPagamento) {
      this.alertService.warning('Atenção', 'Selecione uma forma de pagamento');
      return;
    }

    if (this.metodoPagamento === 'credit_card' && !this.validarDadosCartao()) {
      this.alertService.warning('Atenção', 'Preencha todos os dados do cartão');
      return;
    }

    this.processando = true;
    this.erroAtual = null;
    this.qrCodePix = '';

    try {
      let cardToken = '';

      if (this.metodoPagamento === 'credit_card') {
        try {
          cardToken = await this.pagamentoService.criarTokenCartao(this.dadosCartao);
        } catch (error) {
          this.processando = false;
          this.alertService.error('Erro', 'Dados do cartão inválidos');
          return;
        }
      }

      const request = {
        pedidoId: this.pedidoId,
        metodoPagamento: this.metodoPagamento,
        parcelas: this.metodoPagamento === 'credit_card' ? this.parcelas : 1,
        dadosCartao: this.metodoPagamento === 'credit_card' ? {
          ...this.dadosCartao,
          cardToken
        } : undefined
      };

      this.pagamentoService.criarPagamento(request).subscribe({
        next: (response) => {
          this.processando = false;

          if (response.sucesso) {
            if (this.metodoPagamento === 'pix' && response.qrCodeBase64) {
              this.qrCodePix = response.qrCodeBase64;
              this.alertService.success('QR Code gerado!', 'Escaneie o código para pagar');
              // Iniciar verificação automática do pagamento
              this.iniciarVerificacaoPagamento();
            } else {
              this.alertService.success('Pagamento aprovado!', 'Seu pedido está sendo processado');
              setTimeout(() => {
                this.router.navigate(['/pedido-sucesso'], {
                  queryParams: { numero: this.numeroPedido }
                });
              }, 1500);
            }
          } else {
            this.erroAtual = {
              mensagem: response.mensagem || 'Pagamento recusado',
              codigoErro: response.codigoErro
            };
            this.alertService.error('Pagamento recusado', this.erroAtual.mensagem);
          }
        },
        error: (error) => {
          this.processando = false;
          this.erroAtual = {
            mensagem: 'Não foi possível processar o pagamento',
            codigoErro: 'ERRO_INTERNO'
          };
          console.error('Erro ao processar pagamento:', error);
          this.alertService.error('Erro', this.erroAtual.mensagem);
        }
      });
    } catch (error) {
      this.processando = false;
      this.erroAtual = {
        mensagem: 'Não foi possível processar o pagamento',
        codigoErro: 'ERRO_INTERNO'
      };
      console.error('Erro ao processar pagamento:', error);
      this.alertService.error('Erro', this.erroAtual.mensagem);
    }
  }

  abrirRetentativa(): void {
    this.metodoPagamento = '';
    this.parcelas = 1;
    this.atualizarValorParcela();
    this.dadosCartao = {
      cardNumber: '',
      cardholderName: '',
      expirationMonth: '',
      expirationYear: '',
      securityCode: '',
      cpf: ''
    };
    this.qrCodePix = '';
    this.erroAtual = null;
  }

  validarDadosCartao(): boolean {
    return !!(
      this.dadosCartao.cardNumber &&
      this.dadosCartao.cardholderName &&
      this.dadosCartao.expirationMonth &&
      this.dadosCartao.expirationYear &&
      this.dadosCartao.securityCode &&
      this.dadosCartao.cpf
    );
  }

  private iniciarVerificacaoPagamento(): void {
    this.tentativasVerificacao = 0;
    
    // Verificar a cada 5 segundos
    this.verificandoPagamento = setInterval(async () => {
      this.tentativasVerificacao++;
      
      try {
        const pedido = await firstValueFrom(this.pedidoService.obterPorId(this.pedidoId));
        
        console.log(`Verificação ${this.tentativasVerificacao}: Status do pedido = ${pedido.status}`);
        
        // Se o pagamento foi aprovado
        if (pedido.status === StatusPedido.Pago) {
          this.pararVerificacaoPagamento();
          this.alertService.success('Pagamento confirmado!', 'Seu pedido foi aprovado');
          setTimeout(() => {
            this.router.navigate(['/pedido-sucesso'], { 
              queryParams: { 
                numero: this.numeroPedido
              } 
            });
          }, 1500);
        }
        // Se foi cancelado ou expirou
        else if (pedido.status === StatusPedido.Cancelado) {
          this.pararVerificacaoPagamento();
          this.alertService.error('Pagamento não confirmado', 'O pagamento não foi realizado no prazo');
        }
        // Timeout após 5 minutos
        else if (this.tentativasVerificacao >= this.maxTentativas) {
          this.pararVerificacaoPagamento();
          this.alertService.info('Verificação encerrada', 'O pagamento ainda está pendente. Você pode acompanhar em "Meus Pedidos"');
        }
      } catch (erro) {
        console.error('Erro ao verificar status do pedido:', erro);
        // Se der erro muitas vezes, parar
        if (this.tentativasVerificacao >= this.maxTentativas) {
          this.pararVerificacaoPagamento();
        }
      }
    }, 5000); // 5 segundos
  }

  private pararVerificacaoPagamento(): void {
    if (this.verificandoPagamento) {
      clearInterval(this.verificandoPagamento);
      this.verificandoPagamento = null;
    }
  }

  ngOnDestroy(): void {
    this.pararVerificacaoPagamento();
  }
}
