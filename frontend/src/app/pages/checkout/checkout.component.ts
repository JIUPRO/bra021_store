import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CarrinhoService } from '../../services/carrinho.service';
import { ClienteService } from '../../services/cliente.service';
import { PedidoService } from '../../services/pedido.service';
import { AlertService } from '../../services/alert.service';
import { EscolaService } from '../../services/escola.service';
import { ParametroSistemaService } from '../../services/parametro-sistema.service';
import { PagamentoService } from '../../services/pagamento.service';
import { ItemCarrinho } from '../../models/produto.model';
import { Cliente } from '../../models/cliente.model';
import { CriarPedido, StatusPedido } from '../../models/pedido.model';
import { Escola } from '../../models/escola.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <h1 class="fw-bold mb-4">
        <i class="bi bi-credit-card me-2"></i>Finalizar Compra
      </h1>

      <div *ngIf="itens.length === 0" class="text-center py-5">
        <i class="bi bi-cart-x fs-1 text-muted"></i>
        <h4 class="mt-3">Seu carrinho está vazio</h4>
        <p class="text-muted">Adicione produtos para continuar.</p>
        <a routerLink="/produtos" class="btn btn-primary btn-lg">
          <i class="bi bi-grid me-2"></i>Ver Produtos
        </a>
      </div>

      <div *ngIf="itens.length > 0" class="row">
        <!-- Formulário de Dados -->
        <div class="col-lg-8">
          <!-- Dados Pessoais e Cadastro -->
          <div class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-person me-2"></i>Dados Pessoais e Cadastro</h5>
            </div>
            <div class="card-body">
              <ng-container *ngIf="!cliente; else clienteInfo">
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Nome Completo *</label>
                    <input type="text" class="form-control" [(ngModel)]="dadosEntrega.nome" placeholder="Digite seu nome completo">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Telefone *</label>
                    <input type="tel" class="form-control" [(ngModel)]="dadosEntrega.telefone" placeholder="(11) 99999-9999">
                  </div>
                </div>
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Email (Login) *</label>
                    <input type="email" class="form-control" [(ngModel)]="dadosEntrega.email" placeholder="seu@email.com">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">CPF</label>
                    <input type="text" class="form-control" [(ngModel)]="dadosEntrega.cpf" placeholder="000.000.000-00" (blur)="verificarCpf()">
                  </div>
                </div>
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Senha *</label>
                    <input type="password" class="form-control" [(ngModel)]="dadosEntrega.senha" placeholder="Digite uma senha segura">
                    <small class="form-text text-muted">Mínimo 6 caracteres</small>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Confirmar Senha *</label>
                    <input type="password" class="form-control" [(ngModel)]="dadosEntrega.confirmaSenha" placeholder="Confirme sua senha">
                  </div>
                </div>
                <div class="alert alert-info mb-0">
                  <i class="bi bi-info-circle me-2"></i>
                  <small>Após concluir a compra, você já poderá fazer login com seu email e senha para acompanhar seus pedidos.</small>
                </div>
              </ng-container>

              <ng-template #clienteInfo>
                <p><strong>Nome:</strong> {{ cliente?.nome }}</p>
                <p><strong>Email:</strong> {{ cliente?.email }}</p>
                <p><strong>Telefone:</strong> {{ cliente?.telefone || '—' }}</p>
                <div class="mt-3">
                  <button class="btn btn-link p-0" (click)="irParaLogin()">Entrar com outra conta</button>
                </div>
              </ng-template>
            </div>
          </div>

          <!-- Seleção de Escola -->
          <div class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-building me-2"></i>Escola *</h5>
            </div>
            <div class="card-body">
              <p class="text-muted mb-3">Selecione a escola à qual você está vinculado</p>
              <select class="form-select" [(ngModel)]="escolaId" (change)="onEscolaChange()" required>
                <option value="">Selecione uma escola</option>
                <option *ngFor="let escola of escolas" [value]="escola.id">
                  {{ escola.nome }}{{ escola.professorResponsavel ? ' - Prof. ' + escola.professorResponsavel : '' }}
                </option>
              </select>
              <small *ngIf="!escolaId" class="form-text text-danger">Campo obrigatório</small>
            </div>
          </div>

          <!-- Endereço de Entrega -->
          <div class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-geo-alt me-2"></i>Endereço de Entrega</h5>
            </div>
            <div class="card-body">
              <div *ngIf="tipoEnderecoEntrega === 'Ambos'" class="mb-3">
                <label class="form-label">Entregar em *</label>
                <div class="d-flex gap-3">
                  <div class="form-check">
                    <input
                      class="form-check-input"
                      type="radio"
                      id="entregaCliente"
                      name="entregaTipo"
                      [value]="'Cliente'"
                      [(ngModel)]="enderecoEntregaSelecionado"
                      (change)="onEnderecoEntregaSelecionadoChange()"
                    >
                    <label class="form-check-label" for="entregaCliente">Meu endereço</label>
                  </div>
                  <div class="form-check">
                    <input
                      class="form-check-input"
                      type="radio"
                      id="entregaEscola"
                      name="entregaTipo"
                      [value]="'Escola'"
                      [(ngModel)]="enderecoEntregaSelecionado"
                      (change)="onEnderecoEntregaSelecionadoChange()"
                    >
                    <label class="form-check-label" for="entregaEscola">Endereço da escola</label>
                  </div>
                </div>
              </div>

              <ng-container *ngIf="usarEnderecoEscola; else enderecoCliente">
                <ng-container *ngIf="escolaSelecionada; else enderecoEscolaVazio">
                  <p><strong>CEP:</strong> {{ escolaSelecionada.cep || '—' }}</p>
                  <p><strong>Endereço:</strong> {{ escolaSelecionada.logradouro || '—' }}{{ escolaSelecionada.numero ? ', ' + escolaSelecionada.numero : '' }}{{ escolaSelecionada.complemento ? ' - ' + escolaSelecionada.complemento : '' }}</p>
                  <p><strong>Bairro:</strong> {{ escolaSelecionada.bairro || '—' }}</p>
                  <p><strong>Cidade/UF:</strong> {{ escolaSelecionada.cidade || '—' }} / {{ escolaSelecionada.estado || '—' }}</p>
                  <small class="text-muted d-block mt-2">A entrega será feita na escola selecionada.</small>
                </ng-container>
              </ng-container>

              <ng-template #enderecoEscolaVazio>
                <div class="alert alert-warning mb-0">
                  <i class="bi bi-exclamation-triangle me-2"></i>
                  Selecione uma escola com endereço completo para entrega.
                </div>
              </ng-template>

              <ng-template #enderecoCliente>
                <ng-container *ngIf="!cliente; else enderecoInfo">
                  <div class="row">
                    <div class="col-md-4 mb-3">
                      <label class="form-label">CEP *</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.cep" placeholder="00000-000" (blur)="buscarCep()">
                    </div>
                    <div class="col-md-8 mb-3">
                      <label class="form-label">Logradouro *</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.logradouro" placeholder="Rua, Avenida, etc.">
                    </div>
                  </div>
                  <div class="row">
                    <div class="col-md-4 mb-3">
                      <label class="form-label">Número *</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.numero" placeholder="123">
                    </div>
                    <div class="col-md-8 mb-3">
                      <label class="form-label">Complemento</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.complemento" placeholder="Apto, Bloco, etc.">
                    </div>
                  </div>
                  <div class="row">
                    <div class="col-md-4 mb-3">
                      <label class="form-label">Bairro *</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.bairro" placeholder="Bairro">
                    </div>
                    <div class="col-md-6 mb-3">
                      <label class="form-label">Cidade *</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.cidade" placeholder="Cidade">
                    </div>
                    <div class="col-md-2 mb-3">
                      <label class="form-label">UF *</label>
                      <input type="text" class="form-control" [(ngModel)]="dadosEntrega.estado" placeholder="SP" maxlength="2">
                    </div>
                  </div>
                </ng-container>

                <ng-template #enderecoInfo>
                  <p><strong>CEP:</strong> {{ dadosEntrega.cep || '—' }}</p>
                  <p><strong>Endereço:</strong> {{ dadosEntrega.logradouro }}{{ dadosEntrega.numero ? ', ' + dadosEntrega.numero : '' }}{{ dadosEntrega.complemento ? ' - ' + dadosEntrega.complemento : '' }}</p>
                  <p><strong>Bairro:</strong> {{ dadosEntrega.bairro || '—' }}</p>
                  <p><strong>Cidade/UF:</strong> {{ dadosEntrega.cidade || '—' }} / {{ dadosEntrega.estado || '—' }}</p>
                  <div class="mt-3"><button class="btn btn-link p-0" (click)="editarEndereco()">Editar endereço</button></div>
                </ng-template>
              </ng-template>
            </div>
          </div>

          <!-- Forma de Pagamento -->
          <div class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-credit-card-2-front me-2"></i>Forma de Pagamento *</h5>
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

              <!-- Dados do Cartão -->
              <div *ngIf="metodoPagamento === 'credit_card'" class="mt-3">
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
                        <small>R$ {{ (total / parcela) | number:'1.2-2' }}</small>
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
                  <label class="form-label">Número do Cartão *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="dadosCartao.cardNumber"
                    placeholder="0000 0000 0000 0000"
                    maxlength="19"
                  >
                </div>
                <div class="mb-3">
                  <label class="form-label">Nome no Cartão *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="dadosCartao.cardholderName"
                    placeholder="NOME COMO ESTÁ NO CARTÃO"
                  >
                </div>
                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Mês *</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      [(ngModel)]="dadosCartao.expirationMonth"
                      placeholder="MM"
                      maxlength="2"
                    >
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Ano *</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      [(ngModel)]="dadosCartao.expirationYear"
                      placeholder="AA"
                      maxlength="2"
                    >
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">CVV *</label>
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
                  <label class="form-label">CPF do Titular *</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    [(ngModel)]="dadosCartao.cpf"
                    placeholder="000.000.000-00"
                    maxlength="14"
                  >
                </div>
              </div>

              <div *ngIf="metodoPagamento === 'pix'" class="alert alert-info mt-3">
                <i class="bi bi-info-circle me-2"></i>
                <strong>PIX:</strong> Após confirmar o pedido, você receberá um QR Code para pagamento instantâneo.
              </div>
            </div>
          </div>

          <!-- Observações -->
          <div class="card mb-4">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-chat-left-text me-2"></i>Observações</h5>
            </div>
            <div class="card-body">
              <textarea class="form-control" [(ngModel)]="observacoes" rows="3" placeholder="Alguma observação sobre o pedido?"></textarea>
            </div>
          </div>
        </div>

        <!-- Resumo do Pedido -->
        <div class="col-lg-4">
          <div class="card sticky-top" style="top: 100px;">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-receipt me-2"></i>Resumo</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <small class="text-muted">Itens ({{ quantidadeItens }})</small>
                <div *ngFor="let item of itens" class="d-flex justify-content-between small mb-1">
                  <span>
                    {{ item.quantidade }}x {{ item.produto.nome }}
                    <span *ngIf="item.tamanhoVariacao">({{ item.tamanhoVariacao }})</span>
                  </span>
                  <span>R$ {{ calcularSubtotalItem(item) | number:'1.2-2' }}</span>
                </div>
              </div>
              
              <hr>
              
              <div class="d-flex justify-content-between mb-2">
                <span>Subtotal</span>
                <span>R$ {{ subtotal | number:'1.2-2' }}</span>
              </div>
              <div class="d-flex justify-content-between mb-2">
                <span>Frete</span>
                <span class="text-success">R$ {{ calcularFrete() | number:'1.2-2' }}</span>
              </div>
              <div class="d-flex justify-content-between mb-2">
                <span>Prazo de entrega</span>
                <span class="text-muted">{{ calcularPrazoEntrega() }} dias</span>
              </div>
              <div class="d-flex justify-content-between mb-3">
                <span>Desconto</span>
                <span class="text-success">- R$ 0,00</span>
              </div>
              
              <hr>
              
              <div class="d-flex justify-content-between mb-4">
                <span class="fw-bold fs-5">Total</span>
                <span class="fw-bold fs-5 text-primary">R$ {{ total | number:'1.2-2' }}</span>
              </div>
              
              <button 
                class="btn btn-primario w-100 btn-lg" 
                (click)="confirmarPedido()"
                [disabled]="!dadosValidos() || processando || existeCadastro"
              >
                <span *ngIf="!processando">
                  <i class="bi bi-check-circle me-2"></i>Confirmar Pedido
                </span>
                <span *ngIf="processando">
                  <span class="spinner-border spinner-border-sm me-2"></span>
                  Processando...
                </span>
              </button>
              
              <!-- Seção QR Code PIX -->
              <div *ngIf="qrCodePix" id="qrcode-section" class="card mt-4 border-success">
                <div class="card-header bg-success text-white">
                  <h5 class="mb-0">
                    <i class="bi bi-qr-code me-2"></i>Pague com PIX
                  </h5>
                </div>
                <div class="card-body text-center py-4">
                  <p class="mb-3">Escaneie o QR Code abaixo com o app do seu banco:</p>
                  <img 
                    [src]="'data:image/png;base64,' + qrCodePix" 
                    alt="QR Code PIX" 
                    class="img-fluid mb-3"
                    style="max-width: 300px; border: 3px solid #198754; border-radius: 10px; padding: 10px; background: white;"
                  >
                  <div class="alert alert-info mb-3">
                    <i class="bi bi-info-circle me-2"></i>
                    Após pagar, seu pedido será confirmado automaticamente.
                  </div>
                  <div class="d-flex align-items-center justify-content-center text-muted">
                    <span class="spinner-border spinner-border-sm me-2"></span>
                    <small>Aguardando pagamento...</small>
                  </div>
                </div>
              </div>
              
              <a routerLink="/carrinho" class="btn btn-outline-secondary w-100 mt-2">
                <i class="bi bi-arrow-left me-2"></i>Voltar ao Carrinho
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .btn-primario {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      border: none;
      border-radius: 8px;
      padding: 12px 24px;
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
    }
    
    .sticky-top {
      z-index: 100;
    }

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
  `]
})
export class CheckoutComponent implements OnInit, OnDestroy {
  itens: ItemCarrinho[] = [];
  cliente: Cliente | null = null;
  observacoes = '';
  processando = false;

  existeCadastro = false;
  cadastroEncontrado: Cliente | null = null;

  escolas: Escola[] = [];
  escolaId: string = '';
  escolaSelecionada: Escola | null = null;

  tipoEnderecoEntrega: 'Cliente' | 'Escola' | 'Ambos' = 'Escola';
  enderecoEntregaSelecionado: 'Cliente' | 'Escola' = 'Escola';

  // Pagamento
  metodoPagamento: string = '';
  qrCodePix: string = '';
  pedidoId: string = '';
  verificandoPagamento: any;
  tentativasVerificacao: number = 0;
  maxTentativas: number = 60; // 5 minutos (60 * 5s = 5min)
  parcelas: number = 1;
  valorParcelado: number = 0;
  maxParcelas: number = 3;
  opcoesParcelamento: number[] = [1, 2, 3];
  dadosCartao = {
    cardNumber: '',
    cardholderName: '',
    expirationMonth: '',
    expirationYear: '',
    securityCode: '',
    cpf: ''
  };

  dadosEntrega = {
    nome: '',
    telefone: '',
    email: '',
    cpf: '',
    senha: '',
    confirmaSenha: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    cidade: '',
    estado: ''
  };

  constructor(
    private carrinhoService: CarrinhoService,
    private clienteService: ClienteService,
    private pedidoService: PedidoService,
    private alertService: AlertService,
    private escolaService: EscolaService,
    private parametroSistemaService: ParametroSistemaService,
    private pagamentoService: PagamentoService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carrinhoService.carrinho$.subscribe(itens => {
      this.itens = itens;
      this.atualizarValorParcela();
    });

    this.clienteService.clienteLogado$.subscribe(cliente => {
      this.cliente = cliente;
      if (cliente) {
        this.preencherDadosCliente(cliente);
      }
      this.aplicarEnderecoEntrega();
    });

    this.carregarParametroTipoEnderecoEntrega();
    this.carregarEscolas();
    this.carregarMaximoParcelas();
  }

  carregarEscolas(): void {
    this.escolaService.obterAtivas().subscribe({
      next: (escolas) => {
        this.escolas = escolas;
        this.atualizarEscolaSelecionada();
        this.aplicarEnderecoEntrega();
      },
      error: (err) => {
        console.error('Erro ao carregar escolas', err);
      }
    });
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

  carregarParametroTipoEnderecoEntrega(): void {
    this.parametroSistemaService.obterPorChave('TipoEnderecoEntrega').subscribe({
      next: (parametro) => {
        this.tipoEnderecoEntrega = this.normalizarTipoEndereco(parametro?.valor);
        if (this.tipoEnderecoEntrega === 'Ambos') {
          if (!this.enderecoEntregaSelecionado) {
            this.enderecoEntregaSelecionado = 'Cliente';
          }
        } else {
          this.enderecoEntregaSelecionado = this.tipoEnderecoEntrega;
        }
        this.aplicarEnderecoEntrega();
      },
      error: () => {
        this.tipoEnderecoEntrega = 'Escola';
        this.enderecoEntregaSelecionado = 'Escola';
        this.aplicarEnderecoEntrega();
      }
    });
  }

  private normalizarTipoEndereco(valor?: string | null): 'Cliente' | 'Escola' | 'Ambos' {
    const normalizado = (valor || '').trim().toLowerCase();
    if (normalizado === 'cliente') return 'Cliente';
    if (normalizado === 'ambos') return 'Ambos';
    return 'Escola';
  }

  get usarEnderecoEscola(): boolean {
    return this.tipoEnderecoEntrega === 'Escola' ||
      (this.tipoEnderecoEntrega === 'Ambos' && this.enderecoEntregaSelecionado === 'Escola');
  }

  onEscolaChange(): void {
    this.atualizarEscolaSelecionada();
    this.aplicarEnderecoEntrega();
  }

  onEnderecoEntregaSelecionadoChange(): void {
    this.aplicarEnderecoEntrega();
  }

  private atualizarEscolaSelecionada(): void {
    this.escolaSelecionada = this.escolas.find(e => e.id === this.escolaId) || null;
  }

  private aplicarEnderecoEntrega(): void {
    if (this.usarEnderecoEscola) {
      this.preencherEnderecoDaEscola();
      return;
    }

    this.preencherEnderecoDoCliente();
  }

  private preencherEnderecoDaEscola(): void {
    if (!this.escolaSelecionada) {
      this.limparEnderecoEntrega();
      return;
    }

    this.dadosEntrega.cep = this.escolaSelecionada.cep || '';
    this.dadosEntrega.logradouro = this.escolaSelecionada.logradouro || '';
    this.dadosEntrega.numero = this.escolaSelecionada.numero || '';
    this.dadosEntrega.complemento = this.escolaSelecionada.complemento || '';
    this.dadosEntrega.bairro = this.escolaSelecionada.bairro || '';
    this.dadosEntrega.cidade = this.escolaSelecionada.cidade || '';
    this.dadosEntrega.estado = this.escolaSelecionada.estado || '';
  }

  private preencherEnderecoDoCliente(): void {
    if (!this.cliente) {
      return;
    }

    this.dadosEntrega.cep = this.cliente.cep || '';
    this.dadosEntrega.logradouro = this.cliente.logradouro || '';
    this.dadosEntrega.numero = this.cliente.numero || '';
    this.dadosEntrega.complemento = this.cliente.complemento || '';
    this.dadosEntrega.bairro = this.cliente.bairro || '';
    this.dadosEntrega.cidade = this.cliente.cidade || '';
    this.dadosEntrega.estado = this.cliente.estado || '';
  }

  private limparEnderecoEntrega(): void {
    this.dadosEntrega.cep = '';
    this.dadosEntrega.logradouro = '';
    this.dadosEntrega.numero = '';
    this.dadosEntrega.complemento = '';
    this.dadosEntrega.bairro = '';
    this.dadosEntrega.cidade = '';
    this.dadosEntrega.estado = '';
  }

  preencherDadosCliente(cliente: Cliente): void {
    this.dadosEntrega.nome = cliente.nome;
    this.dadosEntrega.email = cliente.email;
    this.dadosEntrega.telefone = cliente.telefone || '';
    this.dadosEntrega.cpf = cliente.cpf || '';
    this.preencherEnderecoDoCliente();
  }

  atualizarValorParcela(): void {
    this.valorParcelado = this.total / (this.parcelas || 1);
  }

  get quantidadeItens(): number {
    return this.itens.reduce((total, item) => total + item.quantidade, 0);
  }

  get subtotal(): number {
    return this.carrinhoService.calcularSubtotal();
  }

  get total(): number {
    return this.subtotal + this.calcularFrete();
  }

  calcularFrete(): number {
    return this.itens.reduce((maiorFrete, item) => {
      const freteItem = item.produto.valorFrete || 0;
      return freteItem > maiorFrete ? freteItem : maiorFrete;
    }, 0);
  }

  calcularPrazoEntrega(): number {
    return this.itens.reduce((maiorPrazo, item) => {
      const prazoItem = item.produto.prazoEntregaDias || 0;
      return prazoItem > maiorPrazo ? prazoItem : maiorPrazo;
    }, 0);
  }

  calcularSubtotalItem(item: ItemCarrinho): number {
    const preco = item.produto.precoPromocional || item.produto.preco;
    return preco * item.quantidade;
  }

  dadosValidos(): boolean {
    // Validar escola (obrigatório para todos)
    if (!this.escolaId) {
      return false;
    }

    // Validar forma de pagamento
    if (!this.metodoPagamento) {
      return false;
    }

    // Se cartão, validar dados do cartão
    if (this.metodoPagamento === 'credit_card') {
      if (!this.validarDadosCartao()) {
        return false;
      }
    }

    const enderecoValido = this.enderecoEntregaValido();

    // Se cliente logado, não exigimos senha/cadastro, apenas dados de entrega/contato
    if (this.cliente?.id) {
      return !!(
        this.dadosEntrega.nome &&
        this.dadosEntrega.telefone &&
        this.dadosEntrega.email &&
        enderecoValido
      );
    }

    // Se não estiver logado, validar também senha
    if (this.dadosEntrega.senha !== this.dadosEntrega.confirmaSenha) {
      return false;
    }

    if (this.dadosEntrega.senha.length < 6) {
      return false;
    }

    return !!(
      this.dadosEntrega.nome &&
      this.dadosEntrega.telefone &&
      this.dadosEntrega.email &&
      this.dadosEntrega.senha &&
      this.dadosEntrega.confirmaSenha &&
      enderecoValido
    );
  }

  private validarDadosCartao(): boolean {
    return !!(
      this.dadosCartao.cardNumber &&
      this.dadosCartao.cardholderName &&
      this.dadosCartao.expirationMonth &&
      this.dadosCartao.expirationYear &&
      this.dadosCartao.securityCode &&
      this.dadosCartao.cpf
    );
  }

  private enderecoEntregaValido(): boolean {
    if (this.usarEnderecoEscola) {
      return !!(
        this.escolaSelecionada &&
        this.escolaSelecionada.cep &&
        this.escolaSelecionada.logradouro &&
        this.escolaSelecionada.numero &&
        this.escolaSelecionada.bairro &&
        this.escolaSelecionada.cidade &&
        this.escolaSelecionada.estado
      );
    }

    return !!(
      this.dadosEntrega.cep &&
      this.dadosEntrega.logradouro &&
      this.dadosEntrega.numero &&
      this.dadosEntrega.bairro &&
      this.dadosEntrega.cidade &&
      this.dadosEntrega.estado
    );
  }

  buscarCep(): void {
    // Aqui você pode integrar com uma API de CEP
    console.log('Buscar CEP:', this.dadosEntrega.cep);
  }

  verificarCpf(): void {
    const cpf = (this.dadosEntrega.cpf || '').trim();
    if (!cpf) return;

    this.clienteService.obterPorCpf(cpf).subscribe({
      next: (cliente) => {
        this.existeCadastro = true;
        this.cadastroEncontrado = cliente;
        this.alertService.warning('Já existe um cadastro', 'Já existe um cliente cadastrado com este CPF. Faça login para usar a conta.');
      },
      error: (err: any) => {
        // 404 significa não existe cadastro — seguinte fluxo de criação segue normalmente
        if (err?.status === 404) {
          this.existeCadastro = false;
          this.cadastroEncontrado = null;
        } else {
          console.error('Erro ao verificar CPF', err);
          this.alertService.error('Erro', 'Não foi possível verificar o CPF no momento.');
        }
      }
    });
  }

  irParaLogin(): void {
    // Redireciona para a página de login (pode abrir modal conforme sua app)
    this.router.navigate(['/login']);
  }

  editarEndereco(): void {
    // Permite editar o endereço do cliente no checkout
    this.cliente = null;
  }

  confirmarPedido(): void {
    if (!this.dadosValidos()) {
      if (!this.cliente?.id) {
        if (this.dadosEntrega.senha !== this.dadosEntrega.confirmaSenha) {
          this.alertService.error('Erro', 'As senhas não correspondem.');
        } else if (this.dadosEntrega.senha.length < 6) {
          this.alertService.error('Erro', 'A senha deve ter no mínimo 6 caracteres.');
        } else {
          this.alertService.error('Erro', 'Por favor, preencha todos os campos obrigatórios.');
        }
      } else {
        this.alertService.error('Erro', 'Por favor, verifique os dados de entrega e contato.');
      }
      return;
    }

    if (this.itens.length === 0) {
      this.alertService.warning('Atenção', 'Seu carrinho está vazio.');
      return;
    }

    this.processando = true;

    // Se não há cliente logado, criar um cliente com os dados do checkout
    if (!this.cliente?.id) {
      this.criarClienteComDadosCheckout();
      return;
    }

    this.enviarPedido(this.cliente.id);
  }

  private criarClienteComDadosCheckout(): void {
    // Se já existe cadastro para esse CPF, solicitar login em vez de criar um novo
    if (this.existeCadastro && this.cadastroEncontrado) {
      this.processando = false;
      this.alertService.warning('Cadastro existente', 'Já existe um cadastro para este CPF. Por favor, faça login para continuar com esse usuário.');
      return;
    }

    const novoCliente: any = {
      nome: this.dadosEntrega.nome,
      email: this.dadosEntrega.email,
      telefone: this.dadosEntrega.telefone,
      cpf: this.dadosEntrega.cpf || '',
      dataNascimento: new Date(),
      senha: this.dadosEntrega.senha, // usar a senha que o usuário digitou
      cep: this.dadosEntrega.cep,
      logradouro: this.dadosEntrega.logradouro,
      numero: this.dadosEntrega.numero,
      complemento: this.dadosEntrega.complemento,
      bairro: this.dadosEntrega.bairro,
      cidade: this.dadosEntrega.cidade,
      estado: this.dadosEntrega.estado
    };

    this.clienteService.cadastrar(novoCliente).subscribe({
      next: (cliente: any) => {
        this.cliente = cliente;
        // Login automático do cliente criado
        this.clienteService.login({
          email: cliente.email,
          senha: this.dadosEntrega.senha
        }).subscribe({
          next: () => {
            this.enviarPedido(cliente.id);
          },
          error: (erro: any) => {
            // Mesmo se falhar o login automático, prosseguir com o pedido
            this.enviarPedido(cliente.id);
          }
        });
      },
      error: (erro: any) => {
        this.processando = false;
        console.error('Erro ao criar conta', erro);
        this.alertService.error('Erro ao criar conta', erro.error?.mensagem || 'Tente novamente');
      }
    });
  }

  private async enviarPedido(clienteId: string): Promise<void> {
    const pedido: CriarPedido = {
      clienteId: clienteId,
      escolaId: this.escolaId || undefined,
      itens: this.itens.map(item => ({
        produtoId: item.produto.id,
        quantidade: item.quantidade,
        observacoes: item.observacoes,
        produtoTamanhoId: item.produtoVariacaoId
      })),
      valorFrete: this.calcularFrete(),
      valorDesconto: 0,
      prazoEntregaDias: this.calcularPrazoEntrega(),
      observacoes: this.observacoes,
      nomeEntrega: this.dadosEntrega.nome,
      telefoneEntrega: this.dadosEntrega.telefone,
      cepEntrega: this.dadosEntrega.cep,
      logradouroEntrega: this.dadosEntrega.logradouro,
      numeroEntrega: this.dadosEntrega.numero,
      complementoEntrega: this.dadosEntrega.complemento,
      bairroEntrega: this.dadosEntrega.bairro,
      cidadeEntrega: this.dadosEntrega.cidade,
      estadoEntrega: this.dadosEntrega.estado
    };

    try {
      // 1. Criar o pedido
      const resposta = await firstValueFrom(this.pedidoService.criar(pedido));
      
      // 2. Processar pagamento imediatamente
      try {
        let pagamentoRequest: any = {
          pedidoId: resposta.id,
          metodoPagamento: this.metodoPagamento,
          parcelas: this.metodoPagamento === 'credit_card' ? this.parcelas : 1
        };

        // Se for cartão, criar token primeiro
        if (this.metodoPagamento === 'credit_card') {
          console.log('Criando token do cartão...');
          const cardToken = await this.pagamentoService.criarTokenCartao(this.dadosCartao);
          console.log('Token criado:', cardToken);
          pagamentoRequest.dadosCartao = {
            ...this.dadosCartao,
            cardToken: cardToken
          };
        }

        console.log('Enviando requisição de pagamento:', pagamentoRequest);
        const pagamentoResponse = await firstValueFrom(this.pagamentoService.criarPagamento(pagamentoRequest));
        console.log('Resposta do pagamento:', pagamentoResponse);

        if (pagamentoResponse.sucesso) {
          // Se for PIX, mostrar QR Code
          if (this.metodoPagamento === 'pix' && pagamentoResponse.qrCodeBase64) {
            this.qrCodePix = pagamentoResponse.qrCodeBase64;
            this.processando = false;
            this.alertService.success('Pedido criado!', 'Escaneie o QR Code para pagar com PIX');
            // Scroll para a seção do QR Code
            setTimeout(() => {
              document.getElementById('qrcode-section')?.scrollIntoView({ behavior: 'smooth' });
            }, 100);
            // Iniciar verificação automática do pagamento
            this.iniciarVerificacaoPagamento(resposta.id);
          } 
          // Se for cartão aprovado, redirecionar para sucesso
          else if (this.metodoPagamento === 'credit_card' && pagamentoResponse.status === 'approved') {
            this.carrinhoService.limparCarrinho();
            this.processando = false;
            this.alertService.success('Pagamento aprovado!', 'Seu pedido foi confirmado');
            setTimeout(() => {
              this.router.navigate(['/pedido-sucesso'], { 
                queryParams: { 
                  numero: resposta.numeroPedido 
                } 
              });
            }, 1500);
          }
          // Cartão pendente ou outro status
          else {
            this.carrinhoService.limparCarrinho();
            this.processando = false;
            this.alertService.warning(
              'Pedido realizado', 
              'Porém houve um problema com o pagamento. Tente novamente acessando "Meus Pedidos"'
            ).then(() => {
              this.router.navigate(['/meus-pedidos']);
            });
          }
        } else {
          // Pagamento falhou
          this.carrinhoService.limparCarrinho();
          this.processando = false;
          this.alertService.warning(
            'Pedido realizado', 
            'Porém houve um problema com o pagamento. Tente novamente acessando "Meus Pedidos"'
          ).then(() => {
            this.router.navigate(['/meus-pedidos']);
          });
        }
      } catch (erroPagamento: any) {
        // Pedido foi criado mas pagamento deu erro/exceção
        this.carrinhoService.limparCarrinho();
        this.processando = false;
        console.error('Erro ao processar pagamento:', erroPagamento);
        console.error('Detalhes do erro:', {
          message: erroPagamento?.message,
          error: erroPagamento?.error,
          status: erroPagamento?.status,
          statusText: erroPagamento?.statusText
        });
        
        this.alertService.warning(
          'Pedido realizado', 
          'Porém houve um problema com o pagamento. Tente novamente acessando "Meus Pedidos"'
        ).then(() => {
          this.router.navigate(['/meus-pedidos']);
        });
      }
    } catch (erro: any) {
      this.processando = false;
      console.error('Erro ao criar pedido', erro);
      this.alertService.error('Erro ao criar pedido', erro.error?.mensagem || erro.message || 'Tente novamente');
    }
  }

  private iniciarVerificacaoPagamento(pedidoId: string): void {
    this.pedidoId = pedidoId;
    this.tentativasVerificacao = 0;
    
    // Verificar a cada 5 segundos
    this.verificandoPagamento = setInterval(async () => {
      this.tentativasVerificacao++;
      
      try {
        const pedido = await firstValueFrom(this.pedidoService.obterPorId(pedidoId));
        
        console.log(`Verificação ${this.tentativasVerificacao}: Status do pedido = ${pedido.status}`);
        
        // Se o pagamento foi aprovado
        if (pedido.status === StatusPedido.Pago) {
          this.pararVerificacaoPagamento();
          this.carrinhoService.limparCarrinho();
          this.alertService.success('Pagamento confirmado!', 'Seu pedido foi aprovado');
          setTimeout(() => {
            this.router.navigate(['/pedido-sucesso'], { 
              queryParams: { 
                numero: pedido.numeroPedido 
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
