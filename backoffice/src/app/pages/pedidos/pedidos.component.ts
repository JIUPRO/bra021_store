import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PedidoService } from '../../services/pedido.service';
import { AlertService } from '../../services/alert.service';
import { ResumoPedido, StatusPedido } from '../../models/pedido.model';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-pedidos',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, PaginationComponent],
  template: `
    <div class="pedidos-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-bag me-2"></i>Pedidos</h2>
        <button class="btn btn-primary" (click)="exportar()">
          <i class="bi bi-download me-2"></i>Exportar
        </button>
      </div>

      <!-- Filtros -->
      <div class="card card-dashboard mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <input type="text" class="form-control" placeholder="Nº do Pedido" [(ngModel)]="filtros.numero">
            </div>
            <div class="col-md-3">
              <select class="form-select" [(ngModel)]="filtros.status">
                <option value="">Todos os Status</option>
                <option value="pendente">Pendente</option>
                <option value="pago">Pago</option>
                <option value="enviado">Enviado</option>
                <option value="entregue">Entregue</option>
              </select>
            </div>
            <div class="col-md-3">
              <input type="date" class="form-control" [(ngModel)]="filtros.dataInicio" placeholder="Data Início">
            </div>
            <div class="col-md-3">
              <button class="btn btn-primary w-100" (click)="filtrar()">
                <i class="bi bi-search me-2"></i>Filtrar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Tabela -->
      <div class="card card-dashboard">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Pedido</th>
                  <th>Cliente</th>
                  <th>Data</th>
                  <th>Status</th>
                  <th>Itens</th>
                  <th>Total</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let pedido of pedidosPaginados">
                  <td><strong>{{ pedido.numeroPedido }}</strong></td>
                  <td>{{ pedido.nomeCliente }}</td>
                  <td>{{ pedido.dataPedido | date:'dd/MM/yyyy' }}</td>
                  <td>
                    <span class="badge" [ngClass]="getStatusColor(pedido.status)">
                      {{ pedido.statusDescricao }}
                    </span>
                  </td>
                  <td>{{ pedido.quantidadeItens }}</td>
                  <td class="fw-bold">R$ {{ pedido.valorTotal | number:'1.2-2' }}</td>
                  <td>
                    <div class="btn-group">
                      <a [routerLink]="['/pedidos', pedido.id]" class="btn btn-sm btn-outline-primary">
                        <i class="bi bi-eye"></i>
                      </a>
                      <button class="btn btn-sm btn-outline-primary" (click)="abrirModalStatus(pedido)">
                        <i class="bi bi-arrow-clockwise"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="card-footer bg-white">
          <app-pagination
            [totalItens]="pedidosFiltrados.length"
            [paginaAtual]="paginaAtual"
            [itensPorPagina]="itensPorPagina"
            (paginar)="onPaginar($event)">
          </app-pagination>
        </div>
      </div>
    </div>

    <!-- Modal Status do Pedido -->
    <div class="modal" [class.show]="mostrarModalStatus" [style.display]="mostrarModalStatus ? 'block' : 'none'">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              Alterar Status - {{ pedidoSelecionado?.numeroPedido }}
            </h5>
            <button type="button" class="btn-close" (click)="fecharModalStatus()"></button>
          </div>
          <div class="modal-body">
            <label class="form-label">Status</label>
            <select class="form-select" [(ngModel)]="statusSelecionado">
              <option *ngFor="let s of getStatusOptions()" [ngValue]="s.valor">{{ s.texto }}</option>
            </select>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="fecharModalStatus()">Cancelar</button>
            <button type="button"
                    class="btn btn-danger"
                    (click)="confirmarCancelamento()"
                    [disabled]="cancelando || pedidoSelecionado?.status === statusCancelado">
              <i class="bi bi-x-circle me-1"></i>Cancelar e Estornar
            </button>
            <button type="button" class="btn btn-green" (click)="confirmarStatus()">
              <i class="bi bi-check2-circle me-1"></i>Atualizar
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-backdrop fade" [class.show]="mostrarModalStatus" *ngIf="mostrarModalStatus"></div>
  `,
  styles: [`
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

    .modal.show {
      z-index: 1050;
    }

    .modal-backdrop.show {
      opacity: 0.5;
      z-index: 1040;
    }
  `]
})
export class PedidosComponent implements OnInit {
  private pedidoService = inject(PedidoService);
  private alertService = inject(AlertService);

  filtros = {
    numero: '',
    status: '',
    dataInicio: ''
  };

  pedidos: ResumoPedido[] = [];
  pedidosFiltrados: ResumoPedido[] = [];
  pedidosPaginados: ResumoPedido[] = [];
  paginaAtual = 1;
  itensPorPagina = 10;
  carregando = true;
  mostrarModalStatus = false;
  pedidoSelecionado: ResumoPedido | null = null;
  statusSelecionado: StatusPedido | null = null;
  cancelando = false;
  
  statusOptions = [
    { valor: StatusPedido.Pendente, texto: 'Pendente' },
    { valor: StatusPedido.AguardandoPagamento, texto: 'Aguardando Pagamento' },
    { valor: StatusPedido.Pago, texto: 'Pago' },
    { valor: StatusPedido.EmSeparacao, texto: 'Em Separação' },
    { valor: StatusPedido.Enviado, texto: 'Enviado' },
    { valor: StatusPedido.Entregue, texto: 'Entregue' },
    { valor: StatusPedido.Cancelado, texto: 'Cancelado' }
  ];

  statusCancelado = StatusPedido.Cancelado;

  ngOnInit(): void {
    this.carregarPedidos();
  }

  carregarPedidos(): void {
    this.carregando = true;
    this.pedidoService.getAll().subscribe({
      next: (list) => {
        this.pedidos = list;
        this.pedidosFiltrados = list;
        this.atualizarPaginacao();
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro carregando pedidos', err);
        this.alertService.error('Erro', 'Não foi possível carregar os pedidos');
        this.pedidos = [];
        this.pedidosFiltrados = [];
        this.carregando = false;
      }
    });
  }

  filtrar(): void {
    this.pedidosFiltrados = this.pedidos.filter(pedido => {
      const matchNumero = !this.filtros.numero || 
        pedido.numeroPedido.toLowerCase().includes(this.filtros.numero.toLowerCase());
      
      const matchStatus = !this.filtros.status || 
        pedido.statusDescricao.toLowerCase().includes(this.filtros.status.toLowerCase());
      
      return matchNumero && matchStatus;
    });

    this.paginaAtual = 1;
    this.atualizarPaginacao();
  }

  exportar(): void {
    if (this.pedidosFiltrados.length === 0) {
      this.alertService.warning('Aviso', 'Nenhum pedido para exportar');
      return;
    }
    
    const csv = this.gerarCSV();
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', `pedidos_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    this.alertService.success('Sucesso', 'Pedidos exportados com sucesso');
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.pedidosPaginados = this.pedidosFiltrados.slice(inicio, fim);
  }

  gerarCSV(): string {
    let csv = 'Pedido,Cliente,Data,Status,Itens,Total\n';
    
    this.pedidosFiltrados.forEach(pedido => {
      csv += `"${pedido.numeroPedido}","${pedido.nomeCliente}","${new Date(pedido.dataPedido).toLocaleDateString('pt-BR')}","${pedido.statusDescricao}",${pedido.quantidadeItens},"${pedido.valorTotal.toFixed(2)}"\n`;
    });
    
    return csv;
  }

  abrirModalStatus(pedido: ResumoPedido): void {
    this.pedidoSelecionado = pedido;
    this.statusSelecionado = pedido.status;
    this.mostrarModalStatus = true;
  }

  fecharModalStatus(): void {
    this.mostrarModalStatus = false;
    this.pedidoSelecionado = null;
    this.statusSelecionado = null;
  }

  confirmarStatus(): void {
    if (!this.pedidoSelecionado || this.statusSelecionado === null || this.statusSelecionado === undefined) {
      this.alertService.warning('Aviso', 'Selecione um status válido');
      return;
    }

    this.pedidoService.updateStatus(this.pedidoSelecionado.id, this.statusSelecionado).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Status atualizado com sucesso');
        this.fecharModalStatus();
        this.carregarPedidos();
      },
      error: (err) => {
        console.error('Erro ao atualizar status', err);
        this.alertService.error('Erro', 'Não foi possível atualizar o status');
      }
    });
  }

  getStatusOptions(): { valor: StatusPedido; texto: string }[] {
    if (this.pedidoSelecionado?.status === StatusPedido.Pago) {
      return this.statusOptions.filter(s =>
        s.valor !== StatusPedido.Pendente && s.valor !== StatusPedido.AguardandoPagamento
      );
    }
    return this.statusOptions;
  }

  confirmarCancelamento(): void {
    if (!this.pedidoSelecionado) {
      return;
    }

    this.alertService.confirm(
      'Cancelar pedido',
      'Deseja cancelar este pedido e solicitar estorno/cancelamento do pagamento?'
    ).then((confirmado) => {
      if (!confirmado) {
        return;
      }

      this.cancelando = true;
      this.pedidoService.cancelarPagamento(this.pedidoSelecionado!.id).subscribe({
        next: () => {
          this.cancelando = false;
          this.alertService.success('Sucesso', 'Pedido cancelado e estorno solicitado');
          this.fecharModalStatus();
          this.carregarPedidos();
        },
        error: (err) => {
          console.error('Erro ao cancelar pedido', err);
          this.cancelando = false;
          this.alertService.error('Erro', 'Não foi possível cancelar o pedido');
        }
      });
    });
  }

  getStatusColor(status: number): string {
    switch (status) {
      case 0: return 'bg-warning'; // Pendente
      case 1: return 'bg-info'; // Aguardando Pagamento
      case 2: return 'bg-success'; // Pago
      case 3: return 'bg-primary'; // Em Separação
      case 4: return 'bg-info'; // Enviado
      case 5: return 'bg-success'; // Entregue
      case 6: return 'bg-danger'; // Cancelado
      default: return 'bg-secondary';
    }
  }
}
