import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { PedidoService } from '../../services/pedido.service';
import { ClienteService } from '../../services/cliente.service';
import { AlertService } from '../../services/alert.service';
import { ResumoPedido, StatusPedido } from '../../models/pedido.model';

@Component({
  selector: 'app-meus-pedidos',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container py-5">
      <h1 class="fw-bold mb-4">
        <i class="bi bi-bag me-2"></i>Meus Pedidos
      </h1>

      <div *ngIf="!clienteLogado" class="text-center py-5">
        <i class="bi bi-person-x fs-1 text-muted"></i>
        <h4 class="mt-3">Você não está logado</h4>
        <p class="text-muted">Faça login para ver seus pedidos.</p>
        <a routerLink="/login" class="btn btn-primary btn-lg">
          <i class="bi bi-box-arrow-in-right me-2"></i>Entrar
        </a>
      </div>

      <div *ngIf="clienteLogado">
        <div *ngIf="carregando" class="text-center py-5">
          <div class="spinner-loja mx-auto"></div>
          <p class="mt-3 text-muted">Carregando pedidos...</p>
        </div>

        <div *ngIf="!carregando && pedidos.length === 0" class="text-center py-5">
          <i class="bi bi-inbox fs-1 text-muted"></i>
          <h4 class="mt-3">Nenhum pedido encontrado</h4>
          <p class="text-muted">Você ainda não realizou nenhum pedido.</p>
          <a routerLink="/produtos" class="btn btn-primary btn-lg">
            <i class="bi bi-grid me-2"></i>Ver Produtos
          </a>
        </div>

        <div *ngIf="!carregando && pedidos.length > 0" class="row">
          <div class="col-12 mb-3" *ngFor="let pedido of pedidos">
            <div class="card pedido-card">
              <div class="card-body">
                <div class="row align-items-center">
                  <div class="col-md-3 mb-3 mb-md-0">
                    <small class="text-muted d-block">Número do Pedido</small>
                    <span class="fw-bold">{{ pedido.numeroPedido }}</span>
                  </div>
                  <div class="col-md-2 mb-3 mb-md-0">
                    <small class="text-muted d-block">Data</small>
                    <span>{{ pedido.dataPedido | date:'dd/MM/yyyy' }}</span>
                  </div>
                  <div class="col-md-2 mb-3 mb-md-0">
                    <small class="text-muted d-block">Status</small>
                    <span class="badge" [class]="getStatusClass(pedido.status)">
                      {{ pedido.statusDescricao }}
                    </span>
                  </div>
                  <div class="col-md-2 mb-3 mb-md-0">
                    <small class="text-muted d-block">Itens</small>
                    <span>{{ pedido.quantidadeItens }}</span>
                  </div>
                  <div class="col-md-2 mb-3 mb-md-0">
                    <small class="text-muted d-block">Total</small>
                    <span class="fw-bold text-primary">R$ {{ pedido.valorTotal | number:'1.2-2' }}</span>
                  </div>
                  <div class="col-md-1 text-end">
                    <button class="btn btn-outline-primary btn-sm" (click)="alternarExpansao(pedido.id)">
                      <i [class]="pedidosExpandidos.has(pedido.id) ? 'bi bi-chevron-up' : 'bi bi-chevron-down'"></i>
                    </button>
                  </div>
                </div>
                
                <!-- Produtos Expandidos -->
                <div *ngIf="pedidosExpandidos.has(pedido.id)" class="row mt-3 pt-3 border-top">
                  <div class="col-12">
                    <small class="text-muted d-block mb-2"><i class="bi bi-box me-2"></i>Produtos:</small>
                    <div class="ms-3">
                      <div *ngFor="let produto of pedido.produtos" class="mb-2">
                        <span class="fw-bold">{{ produto.nome }}</span>
                        <span *ngIf="produto.tamanho" class="text-muted ms-2">
                          <i class="bi bi-rulers"></i> {{ produto.tamanho }}
                        </span>
                        <span class="text-muted ms-2">Qtd: {{ produto.quantidade }}</span>
                      </div>
                    </div>
                    <div class="mt-3">
                      <small class="text-muted">Prazo de entrega: {{ pedido.prazoEntregaDias }} dias</small>
                    </div>
                  </div>
                </div>
                
                <div class="mt-3 text-end">
                  <button class="btn btn-primary btn-sm" (click)="verDetalhes(pedido.id)">
                    <i class="bi bi-eye me-1"></i>Ver Detalhes
                  </button>
                  <!-- Botão de refazer pagamento para pedidos em AguardandoPagamento -->
                  <button 
                    *ngIf="pedido.status === 1"
                    class="btn btn-warning btn-sm ms-2" 
                    (click)="retentarPagamento(pedido.id, pedido.valorTotal, pedido.numeroPedido)"
                  >
                    <i class="bi bi-arrow-clockwise me-1"></i>Refazer Pagamento
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
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
    
    .pedido-card {
      transition: box-shadow 0.3s ease;
      border: none;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }
    
    .pedido-card:hover {
      box-shadow: 0 5px 20px rgba(0, 0, 0, 0.15);
    }
    
    .badge {
      padding: 0.5em 0.75em;
    }
  `]
})
export class MeusPedidosComponent implements OnInit {
  pedidos: ResumoPedido[] = [];
  clienteLogado = false;
  carregando = true;
  pedidosExpandidos = new Set<string>();

  constructor(
    private pedidoService: PedidoService,
    private clienteService: ClienteService,
    private alertService: AlertService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.clienteService.clienteLogado$.subscribe(cliente => {
      this.clienteLogado = !!cliente;
      if (this.clienteLogado) {
        this.carregarPedidos();
      } else {
        this.carregando = false;
      }
    });
  }

  carregarPedidos(): void {
    this.carregando = true;
    const cliente = this.clienteService.obterClienteLogado();
    if (cliente) {
      this.pedidoService.obterPorCliente(cliente.id).subscribe({
        next: (pedidos) => {
          this.pedidos = pedidos;
          this.carregando = false;
        },
        error: (err) => {
          console.error('Erro ao carregar pedidos', err);
          this.alertService.error('Erro', 'Não foi possível carregar seus pedidos');
          this.carregando = false;
        }
      });
    }
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

  verDetalhes(pedidoId: string): void {
    this.router.navigate(['/pedido-detalhe', pedidoId]);
  }

  alternarExpansao(pedidoId: string): void {
    if (this.pedidosExpandidos.has(pedidoId)) {
      this.pedidosExpandidos.delete(pedidoId);
    } else {
      this.pedidosExpandidos.add(pedidoId);
    }
  }

  // ← NOVO: Retentar pagamento
  retentarPagamento(pedidoId: string, valorTotal: number, numeroPedido: string): void {
    this.router.navigate(['/pagamento'], {
      queryParams: {
        pedidoId: pedidoId,
        numero: numeroPedido,
        total: valorTotal,
        status: StatusPedido.AguardandoPagamento
      }
    });
  }
}
