import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AlertService } from '../../../services/alert.service';
import { PedidoService } from '../../../services/pedido.service';
import { Pedido, StatusPedido } from '../../../models/pedido.model';
import { PaginationComponent } from '../../../components/pagination/pagination.component';

@Component({
  selector: 'app-pedido-detalhe',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PaginationComponent],
  template: `
    <div class="pedido-detalhe">
      <div *ngIf="carregando" class="text-center py-5">
        <div class="spinner-admin mx-auto"></div>
        <p class="mt-3 text-muted">Carregando pedido...</p>
      </div>

      <div *ngIf="!carregando && pedido">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h2 class="fw-bold mb-0">
            <i class="bi bi-bag me-2"></i>Pedido {{ pedido.numeroPedido }}
          </h2>
          <a routerLink="/pedidos" class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-2"></i>Voltar
          </a>
        </div>

        <div class="row g-4">
          <div class="col-lg-8">
            <!-- Status do Pedido -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-info-circle me-2"></i>Status do Pedido
                </h5>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <span class="badge fs-6" [class]="getStatusColor(pedido.status)">
                    {{ pedido.statusDescricao }}
                  </span>
                  <small class="text-muted">{{ pedido.dataPedido | date:'dd/MM/yyyy HH:mm' }}</small>
                </div>
                <div class="d-flex flex-wrap gap-2 align-items-center">
                  <select class="form-select form-select-sm status-select" [(ngModel)]="statusSelecionado">
                    <option *ngFor="let s of getStatusOptionsDisponiveis()" [ngValue]="s.valor">{{ s.texto }}</option>
                  </select>
                  <button
                    type="button"
                    class="btn btn-green btn-sm"
                    (click)="salvarStatus()">
                    <i class="bi bi-check2-circle me-1"></i>Atualizar Status
                  </button>
                </div>
              </div>
            </div>

            <!-- Itens do Pedido -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-box me-2"></i>Itens do Pedido
                </h5>
              </div>
              <div class="card-body p-0">
                <div class="table-responsive">
                  <table class="table mb-0">
                    <thead>
                      <tr>
                        <th>Produto</th>
                        <th class="text-center">Qtd</th>
                        <th class="text-end">Preço Unit.</th>
                        <th class="text-end">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let item of itensPaginados">
                        <td>
                          <strong>{{ item.nomeProduto }}</strong>
                          <div *ngIf="item.tamanhoVariacao" class="text-muted small">
                            Tamanho: {{ item.tamanhoVariacao }}
                          </div>
                          <div *ngIf="item.observacoes" class="text-muted small">
                            {{ item.observacoes }}
                          </div>
                        </td>
                        <td class="text-center">{{ item.quantidade }}</td>
                        <td class="text-end">R$ {{ item.precoUnitario | number:'1.2-2' }}</td>
                        <td class="text-end fw-bold">R$ {{ item.valorTotal | number:'1.2-2' }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              <div class="card-footer bg-white">
                <app-pagination
                  [totalItens]="pedido.itens.length"
                  [paginaAtual]="paginaAtualItens"
                  [itensPorPagina]="itensPorPaginaItens"
                  (paginar)="onPaginarItens($event)">
                </app-pagination>
              </div>
            </div>

            <!-- Endereço de Entrega -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-geo-alt me-2"></i>Endereço de Entrega
                </h5>
              </div>
              <div class="card-body">
                <p><strong>{{ pedido.nomeEntrega }}</strong></p>
                <p class="mb-2">
                  {{ pedido.logradouroEntrega }}, {{ pedido.numeroEntrega }}
                  <span *ngIf="pedido.complementoEntrega" class="ms-2">- {{ pedido.complementoEntrega }}</span>
                </p>
                <p class="mb-2">
                  {{ pedido.bairroEntrega }} - {{ pedido.cidadeEntrega }}/{{ pedido.estadoEntrega }}
                </p>
                <p class="mb-1 text-muted">CEP: {{ pedido.cepEntrega }}</p>
                <p class="mb-0 text-muted">Tel: {{ pedido.telefoneEntrega }}</p>
              </div>
            </div>

            <!-- Observações -->
            <div *ngIf="pedido.observacoes" class="card card-dashboard">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-chat-left-text me-2"></i>Observações
                </h5>
              </div>
              <div class="card-body">
                <p class="mb-0">{{ pedido.observacoes }}</p>
              </div>
            </div>
          </div>

          <div class="col-lg-4">
            <!-- Resumo -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-receipt me-2"></i>Resumo
                </h5>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span>Subtotal</span>
                  <span>R$ {{ pedido.valorSubtotal | number:'1.2-2' }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Frete</span>
                  <span>R$ {{ pedido.valorFrete | number:'1.2-2' }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Prazo de entrega</span>
                  <span class="text-muted">{{ pedido.prazoEntregaDias }} dias</span>
                </div>
                <div class="d-flex justify-content-between mb-3">
                  <span>Desconto</span>
                  <span class="text-success">- R$ {{ pedido.valorDesconto | number:'1.2-2' }}</span>
                </div>
                <hr>
                <div class="d-flex justify-content-between">
                  <span class="fw-bold">Total</span>
                  <span class="fw-bold fs-5 text-primary">R$ {{ pedido.valorTotal | number:'1.2-2' }}</span>
                </div>
              </div>
            </div>

            <!-- Informações de Pagamento -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-credit-card me-2"></i>Pagamento
                </h5>
              </div>
              <div class="card-body">
                <div class="row g-3">
                  <div class="col-6">
                    <label class="small text-muted d-block">ID Pagamento</label>
                    <p class="mb-0"><code>{{ pedido.pagamentoId || 'N/A' }}</code></p>
                  </div>
                  <div class="col-6">
                    <label class="small text-muted d-block">Método</label>
                    <p class="mb-0">
                      <span *ngIf="pedido.metodoPagamento === 'pix'" class="badge bg-info">
                        <i class="bi bi-qr-code me-1"></i>PIX
                      </span>
                      <span *ngIf="pedido.metodoPagamento === 'credit_card'" class="badge bg-warning">
                        <i class="bi bi-credit-card me-1"></i>Cartão
                      </span>
                      <span *ngIf="!pedido.metodoPagamento" class="text-muted">N/A</span>
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Nota Fiscal -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-file-earmark-pdf me-2"></i>Nota Fiscal
                </h5>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <label for="notaFiscalUrl" class="form-label">URL da Nota Fiscal</label>
                  <input 
                    type="url" 
                    class="form-control"
                    id="notaFiscalUrl"
                    [(ngModel)]="notaFiscalUrlInput"
                    placeholder="https://..."
                    [disabled]="salvandoNotaFiscal">
                  <small class="text-muted d-block mt-2">
                    <i class="bi bi-info-circle me-1"></i>
                    Insira a URL da nota fiscal gerada pelo sistema de faturamento
                  </small>
                </div>
                <div *ngIf="pedido.notaFiscalUrl" class="alert alert-success mb-3">
                  <i class="bi bi-check-circle me-2"></i>
                  <strong>Nota Fiscal Registrada:</strong><br>
                  <a [href]="pedido.notaFiscalUrl" target="_blank" class="text-decoration-none">
                    <i class="bi bi-file-earmark-pdf me-1"></i>Visualizar
                  </a>
                </div>
                <button 
                  type="button"
                  class="btn btn-green btn-sm"
                  (click)="salvarNotaFiscal()"
                  [disabled]="salvandoNotaFiscal || !notaFiscalUrlInput.trim()">
                  <i *ngIf="!salvandoNotaFiscal" class="bi bi-check2-circle me-1"></i>
                  <span *ngIf="!salvandoNotaFiscal">Registrar Nota Fiscal</span>
                  <span *ngIf="salvandoNotaFiscal">
                    <span class="spinner-border spinner-border-sm me-1"></span>Salvando...
                  </span>
                </button>
              </div>
            </div>

            <!-- Dados do Cliente -->
            <div class="card card-dashboard mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0">
                  <i class="bi bi-person me-2"></i>Dados do Cliente
                </h5>
              </div>
              <div class="card-body">
                <p class="mb-1"><strong>{{ pedido.nomeCliente }}</strong></p>
                <p class="mb-1 text-muted">{{ pedido.emailCliente }}</p>
                <p class="mb-0 text-muted">{{ pedido.telefoneCliente }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="!carregando && !pedido" class="alert alert-warning" role="alert">
        <i class="bi bi-exclamation-triangle me-2"></i>Pedido não encontrado
      </div>
    </div>
  `,
  styles: [`
    .spinner-admin {
      width: 50px;
      height: 50px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #0d6efd;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .card-dashboard {
      border: none;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }
    
    .table th {
      font-weight: 600;
      color: #212529;
      border: none;
      background: #f8f9fa;
      padding: 16px;
    }
    
    .table td {
      vertical-align: middle;
      padding: 16px;
    }

    .btn-green {
      background: #10b981;
      border-color: #10b981;
      color: #fff;
    }

    .btn-green:hover {
      background: #059669;
      border-color: #059669;
    }

    .status-select {
      min-width: 220px;
    }
  `]
})
export class PedidoDetalheComponent implements OnInit {
  private alertService = inject(AlertService);
  private pedidoService = inject(PedidoService);
  private route = inject(ActivatedRoute);

  pedidoId: string | null = null;
  pedido: Pedido | null = null;
  itensPaginados: Pedido['itens'] = [];
  carregando = true;
  statusSelecionado: StatusPedido | null = null;
  paginaAtualItens = 1;
  itensPorPaginaItens = 10;
  notaFiscalUrlInput = '';
  salvandoNotaFiscal = false;
  statusOptions = [
    { valor: StatusPedido.Pendente, texto: 'Pendente' },
    { valor: StatusPedido.AguardandoPagamento, texto: 'Aguardando Pagamento' },
    { valor: StatusPedido.Pago, texto: 'Pago' },
    { valor: StatusPedido.EmSeparacao, texto: 'Em Separação' },
    { valor: StatusPedido.Enviado, texto: 'Enviado' },
    { valor: StatusPedido.Entregue, texto: 'Entregue' },
    { valor: StatusPedido.Cancelado, texto: 'Cancelado' }
  ];

  ngOnInit(): void {
    this.pedidoId = this.route.snapshot.paramMap.get('id');
    if (this.pedidoId) {
      this.carregarPedido();
    }
  }

  carregarPedido(): void {
    if (!this.pedidoId) return;
    
    this.carregando = true;
    this.pedidoService.getById(this.pedidoId).subscribe({
      next: (pedido: any) => {
        this.pedido = pedido;
        this.statusSelecionado = pedido.status;
        this.notaFiscalUrlInput = pedido.notaFiscalUrl || '';
        this.atualizarPaginacaoItens();
        this.carregando = false;
      },
      error: (err: any) => {
        console.error('Erro ao carregar pedido', err);
        this.alertService.error('Erro', 'Não foi possível carregar o pedido');
        this.carregando = false;
      }
    });
  }

  getStatusOptionsDisponiveis(): { valor: StatusPedido; texto: string }[] {
    if (this.pedido && this.pedido.status >= StatusPedido.Pago) {
      return this.statusOptions.filter(s =>
        s.valor !== StatusPedido.Pendente && s.valor !== StatusPedido.AguardandoPagamento
      );
    }
    return this.statusOptions;
  }

  salvarStatus(): void {
    if (this.statusSelecionado === null || this.statusSelecionado === undefined) {
      this.alertService.warning('Aviso', 'Selecione um status válido');
      return;
    }

    this.atualizarStatus(this.statusSelecionado);
  }

  atualizarStatus(novoStatus: StatusPedido): void {
    if (!this.pedidoId) return;

    this.pedidoService.updateStatus(this.pedidoId, novoStatus).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Status atualizado com sucesso');
        this.carregarPedido();
      },
      error: (err) => {
        console.error('Erro ao atualizar status', err);
        this.alertService.error('Erro', 'Não foi possível atualizar o status');
      }
    });
  }

  salvarNotaFiscal(): void {
    if (!this.pedidoId || !this.notaFiscalUrlInput.trim()) {
      this.alertService.warning('Aviso', 'Insira uma URL válida');
      return;
    }

    this.salvandoNotaFiscal = true;
    this.pedidoService.atualizarNotaFiscal(this.pedidoId, this.notaFiscalUrlInput).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Nota fiscal registrada com sucesso! Email enviado ao cliente.');
        this.salvandoNotaFiscal = false;
        this.carregarPedido();
      },
      error: (err) => {
        console.error('Erro ao salvar nota fiscal', err);
        this.alertService.error('Erro', 'Não foi possível registrar a nota fiscal');
        this.salvandoNotaFiscal = false;
      }
    });
  }

  getStatusColor(status: StatusPedido): string {
    switch (status) {
      case StatusPedido.Pendente:
        return 'bg-warning text-dark';
      case StatusPedido.AguardandoPagamento:
        return 'bg-info text-dark';
      case StatusPedido.Pago:
        return 'bg-success';
      case StatusPedido.EmSeparacao:
        return 'bg-primary';
      case StatusPedido.Enviado:
        return 'bg-info';
      case StatusPedido.Entregue:
        return 'bg-success';
      case StatusPedido.Cancelado:
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  onPaginarItens(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtualItens = event.pagina;
    this.itensPorPaginaItens = event.itensPorPagina;
    this.atualizarPaginacaoItens();
  }

  private atualizarPaginacaoItens(): void {
    if (!this.pedido?.itens) {
      this.itensPaginados = [];
      return;
    }

    const inicio = (this.paginaAtualItens - 1) * this.itensPorPaginaItens;
    const fim = inicio + this.itensPorPaginaItens;
    this.itensPaginados = this.pedido.itens.slice(inicio, fim);
  }
}
