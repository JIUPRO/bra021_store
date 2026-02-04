import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PedidoService } from '../../services/pedido.service';
import { AlertService } from '../../services/alert.service';
import { Pedido, StatusPedido } from '../../models/pedido.model';

@Component({
  selector: 'app-pedido-detalhe',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container py-5">
      <div *ngIf="carregando" class="text-center py-5">
        <div class="spinner-loja mx-auto"></div>
        <p class="mt-3 text-muted">Carregando pedido...</p>
      </div>

      <div *ngIf="!carregando && pedido">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h1 class="fw-bold mb-0">
            <i class="bi bi-bag me-2"></i>Pedido {{ pedido.numeroPedido }}
          </h1>
          <button (click)="voltar()" class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-2"></i>Voltar
          </button>
        </div>

        <div class="row g-4">
          <div class="col-lg-8">
            <!-- Status -->
            <div class="card mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0"><i class="bi bi-info-circle me-2"></i>Status</h5>
              </div>
              <div class="card-body">
                <div class="d-flex align-items-center">
                  <span class="badge fs-6" [class]="getStatusClass(pedido.status)">
                    {{ pedido.statusDescricao }}
                  </span>
                  <span class="ms-3 text-muted">{{ pedido.dataPedido | date:'dd/MM/yyyy HH:mm' }}</span>
                </div>
              </div>
            </div>

            <!-- Itens -->
            <div class="card mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0"><i class="bi bi-box me-2"></i>Produtos Pedidos</h5>
              </div>
              <div class="card-body p-0">
                <div class="table-responsive">
                  <table class="table mb-0">
                    <thead class="table-light">
                      <tr>
                        <th>Produto</th>
                        <th class="text-center">Quantidade</th>
                        <th class="text-end">Preço Unit.</th>
                        <th class="text-end">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let item of pedido.itens">
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
            </div>

            <!-- Endereço de Entrega -->
            <div class="card mb-4">
              <div class="card-header bg-white">
                <h5 class="mb-0"><i class="bi bi-geo-alt me-2"></i>Endereço de Entrega</h5>
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
                <p class="mb-0 text-muted">CEP: {{ pedido.cepEntrega }}</p>
                <p class="mb-0 text-muted">Tel: {{ pedido.telefoneEntrega }}</p>
              </div>
            </div>

            <!-- Nota Fiscal -->
            <div *ngIf="pedido.notaFiscalUrl" class="card mb-4 border-success">
              <div class="card-header bg-light border-success">
                <h5 class="mb-0 text-success">
                  <i class="bi bi-file-earmark-pdf me-2"></i>Nota Fiscal Disponível
                </h5>
              </div>
              <div class="card-body">
                <p class="text-muted mb-3">
                  <i class="bi bi-check-circle-fill text-success me-2"></i>
                  Sua nota fiscal está pronta para download.
                </p>
                <a [href]="pedido.notaFiscalUrl" target="_blank" class="btn btn-success btn-sm">
                  <i class="bi bi-download me-1"></i>Baixar Nota Fiscal
                </a>
              </div>
            </div>

            <!-- Observações -->
            <div *ngIf="pedido.observacoes" class="card">
              <div class="card-header bg-white">
                <h5 class="mb-0"><i class="bi bi-chat-left-text me-2"></i>Observações</h5>
              </div>
              <div class="card-body">
                <p class="mb-0">{{ pedido.observacoes }}</p>
              </div>
            </div>
          </div>

          <!-- Resumo -->
          <div class="col-lg-4">
            <div class="card sticky-top" style="top: 20px;">
              <div class="card-header bg-white">
                <h5 class="mb-0"><i class="bi bi-receipt me-2"></i>Resumo</h5>
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
                <div class="d-flex justify-content-between mb-3 align-items-center">
                  <span>Pagamento</span>
                  <span>
                    <span *ngIf="pedido.metodoPagamento === 'pix'" class="badge bg-info">
                      <i class="bi bi-qr-code me-1"></i>PIX
                    </span>
                    <span *ngIf="pedido.metodoPagamento === 'credit_card'" class="badge bg-warning text-dark">
                      <i class="bi bi-credit-card me-1"></i>Cartão
                    </span>
                    <span *ngIf="!pedido.metodoPagamento" class="text-muted">N/A</span>
                  </span>
                </div>
                <hr>
                <div class="d-flex justify-content-between">
                  <span class="fw-bold fs-5">Total</span>
                  <span class="fw-bold fs-5 text-primary">R$ {{ pedido.valorTotal | number:'1.2-2' }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="!carregando && !pedido" class="text-center py-5">
        <i class="bi bi-inbox fs-1 text-muted"></i>
        <h4 class="mt-3">Pedido não encontrado</h4>
        <p class="text-muted">O pedido que você está procurando não existe.</p>
        <button (click)="voltar()" class="btn btn-primary btn-lg">
          <i class="bi bi-arrow-left me-2"></i>Voltar
        </button>
      </div>
    </div>
  `,
  styles: [`
    .spinner-loja {
      width: 50px;
      height: 50px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid var(--cor-primaria);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .card {
      border: none;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }

    .table th {
      font-weight: 600;
      background: #f8f9fa;
      border: none;
      color: #212529;
    }
  `]
})
export class PedidoDetalheComponent implements OnInit {
  pedido: Pedido | null = null;
  carregando = true;

  constructor(
    private pedidoService: PedidoService,
    private alertService: AlertService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.carregarPedido(id);
    }
  }

  carregarPedido(id: string): void {
    this.carregando = true;
    this.pedidoService.obterPorId(id).subscribe({
      next: (pedido) => {
        this.pedido = pedido;
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar pedido', err);
        this.alertService.error('Erro', 'Não foi possível carregar o pedido');
        this.carregando = false;
      }
    });
  }

  getStatusClass(status: StatusPedido): string {
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

  voltar(): void {
    this.router.navigate(['/meus-pedidos']);
  }
}
