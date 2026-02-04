import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  DashboardService,
  DashboardAlerta,
  DashboardEstoqueBaixo,
  DashboardPedidoRecente
} from '../../services/dashboard.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  template: `
    <div class="dashboard">
      <!-- Cards de Estatísticas -->
      <div *ngIf="carregando" class="text-center py-4">
        <div class="spinner-admin mx-auto"></div>
        <p class="mt-2 text-muted">Carregando dashboard...</p>
      </div>

      <div *ngIf="erroCarregar" class="alert alert-warning d-flex align-items-center">
        <i class="bi bi-exclamation-triangle me-2"></i>
        Não foi possível carregar o dashboard.
      </div>

      <div *ngIf="!carregando" class="row g-4 mb-4">
        <div class="col-md-6 col-lg-3">
          <div class="card card-dashboard card-stats">
            <div class="card-stats-icon bg-primary bg-opacity-10 text-primary">
              <i class="bi bi-bag"></i>
            </div>
            <div class="card-stats-info">
              <h3>{{ estatisticas.totalPedidos }}</h3>
              <p>Total de Pedidos</p>
            </div>
          </div>
        </div>
        <div class="col-md-6 col-lg-3">
          <div class="card card-dashboard card-stats">
            <div class="card-stats-icon bg-success bg-opacity-10 text-success">
              <i class="bi bi-currency-dollar"></i>
            </div>
            <div class="card-stats-info">
              <h3>R$ {{ estatisticas.vendasMes | number:'1.2-2' }}</h3>
              <p>Vendas do Mês</p>
            </div>
          </div>
        </div>
        <div class="col-md-6 col-lg-3">
          <div class="card card-dashboard card-stats">
            <div class="card-stats-icon bg-warning bg-opacity-10 text-warning">
              <i class="bi bi-box-seam"></i>
            </div>
            <div class="card-stats-info">
              <h3>{{ estatisticas.totalProdutos }}</h3>
              <p>Produtos</p>
            </div>
          </div>
        </div>
        <div class="col-md-6 col-lg-3">
          <div class="card card-dashboard card-stats">
            <div class="card-stats-icon bg-info bg-opacity-10 text-info">
              <i class="bi bi-people"></i>
            </div>
            <div class="card-stats-info">
              <h3>{{ estatisticas.totalClientes }}</h3>
              <p>Clientes</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Pedidos Recentes e Alertas -->
      <div *ngIf="!carregando" class="row g-4">
        <div class="col-lg-8">
          <div class="card card-dashboard">
            <div class="card-header bg-white d-flex justify-content-between align-items-center">
              <h5 class="mb-0"><i class="bi bi-clock-history me-2"></i>Pedidos Recentes</h5>
              <a routerLink="/pedidos" class="btn btn-sm btn-primary">Ver Todos</a>
            </div>
            <div class="card-body p-0">
              <div class="table-responsive">
                <table class="table table-hover mb-0">
                  <thead>
                    <tr>
                      <th>Pedido</th>
                      <th>Cliente</th>
                      <th>Data</th>
                      <th>Status</th>
                      <th>Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let pedido of pedidosRecentesPaginados">
                      <td><strong>{{ pedido.numeroPedido }}</strong></td>
                      <td>{{ pedido.nomeCliente }}</td>
                      <td>{{ pedido.dataPedido | date:'dd/MM/yyyy' }}</td>
                      <td>
                        <span class="badge" [class]="getStatusColor(pedido.status)">
                          {{ pedido.statusDescricao }}
                        </span>
                      </td>
                      <td>R$ {{ pedido.valorTotal | number:'1.2-2' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
            <div class="card-footer bg-white">
              <app-pagination
                [totalItens]="pedidosRecentes.length"
                [paginaAtual]="paginaAtualPedidosRecentes"
                [itensPorPagina]="itensPorPaginaPedidosRecentes"
                (paginar)="onPaginarPedidosRecentes($event)">
              </app-pagination>
            </div>
          </div>
        </div>

        <div class="col-lg-4">
          <div class="card card-dashboard">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-exclamation-triangle me-2"></i>Alertas</h5>
            </div>
            <div class="card-body">
              <div *ngIf="alertas.length === 0" class="text-center py-4">
                <i class="bi bi-check-circle fs-1 text-success"></i>
                <p class="mt-2 mb-0 text-muted">Nenhum alerta no momento</p>
              </div>
              <div *ngFor="let alerta of alertas" class="alert alert-warning d-flex align-items-center mb-2">
                <i class="bi bi-exclamation-triangle-fill me-2"></i>
                <div>
                  <strong>{{ alerta.titulo }}</strong>
                  <p class="mb-0 small">{{ alerta.mensagem }}</p>
                </div>
              </div>
            </div>
          </div>

          <div class="card card-dashboard mt-4">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-boxes me-2"></i>Estoque Baixo</h5>
            </div>
            <div class="card-body p-0">
              <div class="list-group list-group-flush">
                <a *ngFor="let item of estoqueBaixo" 
                   routerLink="/estoque" 
                   class="list-group-item list-group-item-action d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="mb-0">{{ item.produto }}</h6>
                  </div>
                  <span class="badge bg-danger">{{ item.quantidade }} un</span>
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .spinner-admin {
      width: 40px;
      height: 40px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #10b981;
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
    
    .card-stats {
      padding: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-direction: column;
      text-align: center;
    }
    
    .card-stats-icon {
      width: 60px;
      height: 60px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      margin-bottom: 12px;
    }
    
    .card-stats-info h3 {
      margin: 0;
      font-size: 1.75rem;
      font-weight: 700;
    }
    
    .card-stats-info p {
      margin: 0;
      color: #6c757d;
      font-size: 0.9rem;
    }

    .card-stats-info h3,
    .card-stats-info p {
      line-height: 1.1;
    }

    .card-stats-info {
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
    }
    
    .table th {
      font-weight: 600;
      color: #212529;
      border: none;
      background: #f8f9fa;
    }
    
    .table td {
      vertical-align: middle;
    }
  `]
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  carregando = true;
  erroCarregar = false;
  estatisticas = {
    totalPedidos: 0,
    vendasMes: 0,
    totalProdutos: 0,
    totalClientes: 0
  };

  pedidosRecentes: DashboardPedidoRecente[] = [];
  pedidosRecentesPaginados: DashboardPedidoRecente[] = [];
  alertas: DashboardAlerta[] = [];
  estoqueBaixo: DashboardEstoqueBaixo[] = [];
  paginaAtualPedidosRecentes = 1;
  itensPorPaginaPedidosRecentes = 10;

  ngOnInit(): void {
    this.carregarDashboard();
  }

  carregarDashboard(): void {
    this.carregando = true;
    this.erroCarregar = false;
    this.dashboardService.getDashboard().subscribe({
      next: (dados) => {
        this.estatisticas = dados.estatisticas;
        this.pedidosRecentes = dados.pedidosRecentes;
        this.atualizarPaginacaoPedidosRecentes();
        this.alertas = dados.alertas;
        this.estoqueBaixo = dados.estoqueBaixo;
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar dashboard', err);
        this.erroCarregar = true;
        this.carregando = false;
      }
    });
  }

  getStatusColor(status: number): string {
    switch (status) {
      case 0:
        return 'bg-warning text-dark';
      case 1:
        return 'bg-info text-dark';
      case 2:
        return 'bg-success';
      case 3:
        return 'bg-primary';
      case 4:
        return 'bg-info';
      case 5:
        return 'bg-success';
      case 6:
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  onPaginarPedidosRecentes(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtualPedidosRecentes = event.pagina;
    this.itensPorPaginaPedidosRecentes = event.itensPorPagina;
    this.atualizarPaginacaoPedidosRecentes();
  }

  private atualizarPaginacaoPedidosRecentes(): void {
    const inicio = (this.paginaAtualPedidosRecentes - 1) * this.itensPorPaginaPedidosRecentes;
    const fim = inicio + this.itensPorPaginaPedidosRecentes;
    this.pedidosRecentesPaginados = this.pedidosRecentes.slice(inicio, fim);
  }
}
