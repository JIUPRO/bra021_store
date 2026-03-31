import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ClienteDTO, ClienteService } from '../../services/cliente.service';
import { PedidoService } from '../../services/pedido.service';
import { ResumoPedido, StatusPedido } from '../../models/pedido.model';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-cliente-detalhe',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  template: `
    <div class="cliente-detalhe">
      <div *ngIf="carregando" class="text-center py-5">
        <div class="spinner-admin mx-auto"></div>
        <p class="mt-3 text-muted">Carregando cliente...</p>
      </div>

      <div *ngIf="!carregando && cliente">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h2 class="fw-bold mb-0">
            <i class="bi bi-person-vcard me-2"></i>{{ formatarNome(cliente.nome) }}
          </h2>
          <a routerLink="/clientes" class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-2"></i>Voltar
          </a>
        </div>

        <div class="row g-3 mb-4">
          <div class="col-md-4">
            <div class="card card-dashboard h-100">
              <div class="card-body">
                <div class="small text-muted mb-1">Total de pedidos</div>
                <div class="fs-4 fw-bold">{{ pedidos.length }}</div>
              </div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="card card-dashboard h-100">
              <div class="card-body">
                <div class="small text-muted mb-1">Total gasto</div>
                <div class="fs-4 fw-bold">R$ {{ getTotalGasto() | number:'1.2-2' }}</div>
              </div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="card card-dashboard h-100">
              <div class="card-body">
                <div class="small text-muted mb-1">Ticket médio</div>
                <div class="fs-4 fw-bold">R$ {{ getTicketMedio() | number:'1.2-2' }}</div>
              </div>
            </div>
          </div>
        </div>

        <div class="row g-4">
          <div class="col-lg-4">
            <div class="card card-dashboard">
              <div class="card-header bg-white">
                <h5 class="mb-0"><i class="bi bi-person me-2"></i>Dados do Cliente</h5>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <label class="small text-muted d-block">Email</label>
                  <div>{{ cliente.email }}</div>
                </div>
                <div class="mb-3">
                  <label class="small text-muted d-block">Telefone</label>
                  <div>{{ cliente.telefone || 'N/A' }}</div>
                </div>
                <div class="mb-3">
                  <label class="small text-muted d-block">CPF</label>
                  <div>{{ cliente.cpf || 'N/A' }}</div>
                </div>
                <div class="mb-3">
                  <label class="small text-muted d-block">Email confirmado</label>
                  <span class="badge" [class.bg-success]="cliente.emailConfirmado" [class.bg-warning]="!cliente.emailConfirmado">
                    {{ cliente.emailConfirmado ? 'Sim' : 'Não' }}
                  </span>
                </div>
                <div class="mb-3">
                  <label class="small text-muted d-block">Último pedido</label>
                  <div>{{ getDataUltimoPedido() || 'Sem pedidos' }}</div>
                </div>
                <div class="mb-0">
                  <label class="small text-muted d-block">Endereço</label>
                  <div *ngIf="temEndereco(); else semEndereco">
                    <div>{{ cliente.logradouro }}, {{ cliente.numero }}</div>
                    <div *ngIf="cliente.complemento">{{ cliente.complemento }}</div>
                    <div>{{ cliente.bairro }} - {{ cliente.cidade }}/{{ cliente.estado }}</div>
                    <div>CEP: {{ cliente.cep }}</div>
                  </div>
                  <ng-template #semEndereco>
                    <div class="text-muted">Endereço não informado</div>
                  </ng-template>
                </div>
              </div>
            </div>
          </div>

          <div class="col-lg-8">
            <div class="card card-dashboard">
              <div class="card-header bg-white d-flex justify-content-between align-items-center">
                <h5 class="mb-0"><i class="bi bi-bag me-2"></i>Pedidos do Cliente</h5>
                <span class="badge bg-light text-dark">{{ pedidos.length }} pedido(s)</span>
              </div>
              <div class="card-body p-0">
                <div class="table-responsive">
                  <table class="table table-hover mb-0">
                    <thead>
                      <tr>
                        <th>Número</th>
                        <th>Data</th>
                        <th>Status</th>
                        <th>Itens</th>
                        <th>Total</th>
                        <th>Ações</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngIf="!pedidosPaginados.length">
                        <td colspan="6" class="text-center text-muted py-4">Nenhum pedido encontrado para este cliente.</td>
                      </tr>
                      <tr *ngFor="let pedido of pedidosPaginados">
                        <td><strong>{{ pedido.numeroPedido }}</strong></td>
                        <td>{{ pedido.dataPedido | date:'dd/MM/yyyy HH:mm' }}</td>
                        <td>
                          <span class="badge" [class]="getStatusColor(pedido.status)">
                            {{ pedido.statusDescricao }}
                          </span>
                        </td>
                        <td>{{ pedido.quantidadeItens }}</td>
                        <td class="fw-semibold">R$ {{ pedido.valorTotal | number:'1.2-2' }}</td>
                        <td>
                          <a class="btn btn-sm btn-outline-primary" [routerLink]="['/pedidos', pedido.id]">
                            <i class="bi bi-eye"></i>
                          </a>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              <div class="card-footer bg-white">
                <app-pagination
                  [totalItens]="pedidos.length"
                  [paginaAtual]="paginaAtual"
                  [itensPorPagina]="itensPorPagina"
                  (paginar)="onPaginar($event)">
                </app-pagination>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="!carregando && !cliente" class="alert alert-warning" role="alert">
        <i class="bi bi-exclamation-triangle me-2"></i>Cliente não encontrado
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
  `]
})
export class ClienteDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private clienteService = inject(ClienteService);
  private pedidoService = inject(PedidoService);
  private alertService = inject(AlertService);

  clienteId: string | null = null;
  cliente: ClienteDTO | null = null;
  pedidos: ResumoPedido[] = [];
  pedidosPaginados: ResumoPedido[] = [];
  carregando = true;
  paginaAtual = 1;
  itensPorPagina = 10;

  ngOnInit(): void {
    this.clienteId = this.route.snapshot.paramMap.get('id');
    if (!this.clienteId) {
      this.carregando = false;
      return;
    }

    this.carregarCliente();
    this.carregarPedidos();
  }

  carregarCliente(): void {
    if (!this.clienteId) return;

    this.clienteService.getById(this.clienteId).subscribe({
      next: (cliente) => {
        this.cliente = cliente;
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar cliente', err);
        this.alertService.error('Erro', 'Não foi possível carregar o cliente');
        this.carregando = false;
      }
    });
  }

  carregarPedidos(): void {
    if (!this.clienteId) return;

    this.pedidoService.getByCliente(this.clienteId).subscribe({
      next: (pedidos) => {
        this.pedidos = pedidos;
        this.atualizarPaginacao();
      },
      error: (err) => {
        console.error('Erro ao carregar pedidos do cliente', err);
        this.alertService.error('Erro', 'Não foi possível carregar os pedidos do cliente');
      }
    });
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.pedidosPaginados = this.pedidos.slice(inicio, fim);
  }

  temEndereco(): boolean {
    return !!(this.cliente?.logradouro && this.cliente?.cidade && this.cliente?.estado);
  }

  getStatusColor(status: StatusPedido): string {
    switch (status) {
      case StatusPedido.Pendente:
        return 'bg-secondary';
      case StatusPedido.AguardandoPagamento:
        return 'bg-warning text-dark';
      case StatusPedido.Pago:
        return 'bg-info text-dark';
      case StatusPedido.EmSeparacao:
        return 'bg-primary';
      case StatusPedido.Enviado:
        return 'bg-dark';
      case StatusPedido.Entregue:
        return 'bg-success';
      case StatusPedido.Cancelado:
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  getTotalGasto(): number {
    return this.pedidos.reduce((total, pedido) => total + (pedido.valorTotal || 0), 0);
  }

  getTicketMedio(): number {
    if (!this.pedidos.length) {
      return 0;
    }

    return this.getTotalGasto() / this.pedidos.length;
  }

  getDataUltimoPedido(): string | null {
    if (!this.pedidos.length) {
      return null;
    }

    const ultimoPedido = [...this.pedidos].sort((a, b) =>
      new Date(b.dataPedido).getTime() - new Date(a.dataPedido).getTime()
    )[0];

    return new Date(ultimoPedido.dataPedido).toLocaleString('pt-BR');
  }

  formatarNome(nome?: string): string {
    if (!nome) {
      return '';
    }

    return nome
      .toLowerCase()
      .split(' ')
      .filter(Boolean)
      .map(parte => parte.charAt(0).toUpperCase() + parte.slice(1))
      .join(' ');
  }
}
