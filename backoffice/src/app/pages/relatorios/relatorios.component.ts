import { Component, inject } from '@angular/core';
import { AlertService } from '../../services/alert.service';
import { RelatorioService } from '../../services/relatorio.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-relatorios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="relatorios-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-graph-up me-2"></i>Relatórios</h2>
      </div>

      <!-- Cards de Relatórios -->
      <div class="row g-4">
        <div class="col-md-6 col-lg-4">
          <div class="card card-dashboard h-100">
            <div class="card-body text-center">
              <i class="bi bi-bag-check fs-1 text-success mb-3"></i>
              <h5>Pedidos</h5>
              <p class="text-muted">Relatório completo de pedidos por período</p>
              <button class="btn btn-success" (click)="abrirModalPedidos()">
                <i class="bi bi-file-pdf me-2"></i>Gerar PDF
              </button>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card card-dashboard h-100">
            <div class="card-body text-center">
              <i class="bi bi-percent fs-1 text-danger mb-3"></i>
              <h5>Comissão por Período</h5>
              <p class="text-muted">Comissão a receber por escola nos pedidos</p>
              <button class="btn btn-success" (click)="abrirModalComissao()">
                <i class="bi bi-file-pdf me-2"></i>Gerar PDF
              </button>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card card-dashboard h-100">
            <div class="card-body text-center">
              <i class="bi bi-box2 fs-1 text-info mb-3"></i>
              <h5>Produtos Mais Vendidos</h5>
              <p class="text-muted">Ranking dos 20 produtos mais vendidos</p>
              <button class="btn btn-success" (click)="abrirModalProdutosMaisVendidos()">
                <i class="bi bi-file-pdf me-2"></i>Gerar PDF
              </button>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card card-dashboard h-100">
            <div class="card-body text-center">
              <i class="bi bi-people fs-1 text-primary mb-3"></i>
              <h5>Clientes</h5>
              <p class="text-muted">Análise de clientes e gastos</p>
              <button class="btn btn-success" (click)="abrirModalClientes()">
                <i class="bi bi-file-pdf me-2"></i>Gerar PDF
              </button>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card card-dashboard h-100">
            <div class="card-body text-center">
              <i class="bi bi-basket3 fs-1 text-warning mb-3"></i>
              <h5>Estoque</h5>
              <p class="text-muted">Movimentação de estoque por produto</p>
              <button class="btn btn-success" (click)="abrirModalEstoque()">
                <i class="bi bi-file-pdf me-2"></i>Gerar PDF
              </button>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card card-dashboard h-100">
            <div class="card-body text-center">
              <i class="bi bi-x-circle fs-1 text-secondary mb-3"></i>
              <h5>Produtos Sem Saída</h5>
              <p class="text-muted">Produtos sem vendas no período</p>
              <button class="btn btn-success" (click)="abrirModalProdutosSemSaida()">
                <i class="bi bi-file-pdf me-2"></i>Gerar PDF
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Relatório de Pedidos -->
    <div class="modal" [class.show]="mostrarModalPedidos" [style.display]="mostrarModalPedidos ? 'block' : 'none'">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="bi bi-file-pdf me-2"></i>Gerar Relatório de Pedidos
            </h5>
            <button type="button" class="btn-close" (click)="fecharModalPedidos()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Data Inicial</label>
              <input type="date" class="form-control" [(ngModel)]="dataInicioPedidos">
            </div>
            <div class="mb-3">
              <label class="form-label">Data Final</label>
              <input type="date" class="form-control" [(ngModel)]="dataFimPedidos">
            </div>
            <div class="mb-3">
              <label class="form-label">Status (Opcional)</label>
              <select class="form-select" [(ngModel)]="statusPedidos">
                <option [ngValue]="null">Todos os status</option>
                <option [ngValue]="0">Pendente</option>
                <option [ngValue]="1">Aguardando Pagamento</option>
                <option [ngValue]="2">Pago</option>
                <option [ngValue]="3">Em Separação</option>
                <option [ngValue]="4">Enviado</option>
                <option [ngValue]="5">Entregue</option>
                <option [ngValue]="6">Cancelado</option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="fecharModalPedidos()">Cancelar</button>
            <button type="button" class="btn btn-success" (click)="gerarRelatorioPedidos()" [disabled]="carregandoPedidos">
              <span *ngIf="!carregandoPedidos">
                <i class="bi bi-download me-1"></i>Gerar PDF
              </span>
              <span *ngIf="carregandoPedidos">
                <i class="bi bi-hourglass-split me-1"></i>Gerando...
              </span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-backdrop fade" [class.show]="mostrarModalPedidos" *ngIf="mostrarModalPedidos"></div>

    <!-- Modal Relatório de Comissão -->
    <div class="modal" [class.show]="mostrarModalComissao" [style.display]="mostrarModalComissao ? 'block' : 'none'">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="bi bi-file-pdf me-2"></i>Gerar Relatório de Comissão
            </h5>
            <button type="button" class="btn-close" (click)="fecharModalComissao()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Data Inicial</label>
              <input type="date" class="form-control" [(ngModel)]="dataInicioComissao">
            </div>
            <div class="mb-3">
              <label class="form-label">Data Final</label>
              <input type="date" class="form-control" [(ngModel)]="dataFimComissao">
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="fecharModalComissao()">Cancelar</button>
            <button type="button" class="btn btn-success" (click)="gerarRelatorioComissao()" [disabled]="carregandoComissao">
              <span *ngIf="!carregandoComissao">
                <i class="bi bi-download me-1"></i>Gerar PDF
              </span>
              <span *ngIf="carregandoComissao">
                <i class="bi bi-hourglass-split me-1"></i>Gerando...
              </span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-backdrop fade" [class.show]="mostrarModalComissao" *ngIf="mostrarModalComissao"></div>

    <!-- Modal Relatório Genérico -->
    <div class="modal" [class.show]="mostrarModalRelatorio" [style.display]="mostrarModalRelatorio ? 'block' : 'none'">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="bi bi-file-pdf me-2"></i>Gerar Relatório
            </h5>
            <button type="button" class="btn-close" (click)="fecharModalRelatorio()"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Data Inicial</label>
              <input type="date" class="form-control" [(ngModel)]="dataInicioRelatorio">
            </div>
            <div class="mb-3">
              <label class="form-label">Data Final</label>
              <input type="date" class="form-control" [(ngModel)]="dataFimRelatorio">
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="fecharModalRelatorio()">Cancelar</button>
            <button type="button" class="btn btn-success" (click)="gerarRelatorioGenerico()" [disabled]="carregandoRelatorio">
              <span *ngIf="!carregandoRelatorio">
                <i class="bi bi-download me-1"></i>Gerar PDF
              </span>
              <span *ngIf="carregandoRelatorio">
                <i class="bi bi-hourglass-split me-1"></i>Gerando...
              </span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-backdrop fade" [class.show]="mostrarModalRelatorio" *ngIf="mostrarModalRelatorio"></div>
  `,
  styles: [`
    .card-dashboard {
      border: none;
      border-radius: 12px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
      transition: all 0.3s ease;
    }
    
    .card-dashboard:hover {
      box-shadow: 0 5px 20px rgba(0, 0, 0, 0.12);
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
export class RelatoriosComponent {
  private alertService = inject(AlertService);
  private relatorioService = inject(RelatorioService);
  
  filtros = {
    dataInicio: '',
    dataFim: ''
  };

  // Relatório de Pedidos
  mostrarModalPedidos = false;
  dataInicioPedidos: string = this.obterDataInicio();
  dataFimPedidos: string = this.obterDataFim();
  statusPedidos: number | null = null;
  carregandoPedidos = false;

  // Relatório de Comissão
  mostrarModalComissao = false;
  dataInicioComissao: string = this.obterDataInicio();
  dataFimComissao: string = this.obterDataFim();
  carregandoComissao = false;

  // Relatório Genérico (Produtos, Clientes, Estoque, Sem Saída)
  mostrarModalRelatorio = false;
  tipoRelatorioAtual: string = '';
  dataInicioRelatorio: string = this.obterDataInicio();
  dataFimRelatorio: string = this.obterDataFim();
  carregandoRelatorio = false;

  abrirModalPedidos(): void {
    this.mostrarModalPedidos = true;
  }

  fecharModalPedidos(): void {
    this.mostrarModalPedidos = false;
  }

  gerarRelatorioPedidos(): void {
    if (!this.dataInicioPedidos || !this.dataFimPedidos) {
      this.alertService.warning('Aviso', 'Selecione as datas de início e fim');
      return;
    }

    // Criar Date objects a partir das strings no formato yyyy-MM-dd
    const [anoInicio, mesInicio, diaInicio] = this.dataInicioPedidos.split('-').map(Number);
    const [anoFim, mesFim, diaFim] = this.dataFimPedidos.split('-').map(Number);
    
    const dataInicio = new Date(anoInicio, mesInicio - 1, diaInicio);
    const dataFim = new Date(anoFim, mesFim - 1, diaFim);

    if (dataInicio > dataFim) {
      this.alertService.warning('Aviso', 'Data inicial não pode ser maior que data final');
      return;
    }

    this.carregandoPedidos = true;

    this.relatorioService.gerarRelatorioPedidos(dataInicio, dataFim, this.statusPedidos || undefined).subscribe({
      next: (blob: Blob) => {
        const nomeArquivo = `relatorio_pedidos_${this.formatarDataNome(dataInicio)}_${this.formatarDataNome(dataFim)}.pdf`;
        this.relatorioService.downloadPDF(blob, nomeArquivo);
        this.alertService.success('Sucesso', 'Relatório gerado com sucesso');
        this.fecharModalPedidos();
        this.carregandoPedidos = false;
      },
      error: (err) => {
        console.error('Erro ao gerar relatório', err);
        this.alertService.error('Erro', 'Não foi possível gerar o relatório');
        this.carregandoPedidos = false;
      }
    });
  }

  private obterDataInicio(): string {
    const data = new Date();
    data.setDate(data.getDate() - 30); // 30 dias atrás
    return data.toISOString().split('T')[0];
  }

  private obterDataFim(): string {
    const data = new Date();
    return data.toISOString().split('T')[0];
  }

  private formatarDataNome(data: Date): string {
    return data.toISOString().split('T')[0];
  }

  gerarRelatorio(tipo: string): void {
    this.alertService.info('Relatório', `Gerando relatório de ${tipo}...`);
  }

  abrirModalProdutosMaisVendidos(): void {
    this.tipoRelatorioAtual = 'produtos-vendidos';
    this.dataInicioRelatorio = this.obterDataInicio();
    this.dataFimRelatorio = this.obterDataFim();
    this.mostrarModalRelatorio = true;
  }

  abrirModalClientes(): void {
    this.tipoRelatorioAtual = 'clientes';
    this.dataInicioRelatorio = this.obterDataInicio();
    this.dataFimRelatorio = this.obterDataFim();
    this.mostrarModalRelatorio = true;
  }

  abrirModalEstoque(): void {
    this.tipoRelatorioAtual = 'estoque';
    this.dataInicioRelatorio = this.obterDataInicio();
    this.dataFimRelatorio = this.obterDataFim();
    this.mostrarModalRelatorio = true;
  }

  abrirModalProdutosSemSaida(): void {
    this.tipoRelatorioAtual = 'produtos-sem-saida';
    this.dataInicioRelatorio = this.obterDataInicio();
    this.dataFimRelatorio = this.obterDataFim();
    this.mostrarModalRelatorio = true;
  }

  fecharModalRelatorio(): void {
    this.mostrarModalRelatorio = false;
    this.tipoRelatorioAtual = '';
  }

  gerarRelatorioGenerico(): void {
    if (!this.dataInicioRelatorio || !this.dataFimRelatorio) {
      this.alertService.warning('Aviso', 'Selecione as datas de início e fim');
      return;
    }

    // Criar Date objects a partir das strings no formato yyyy-MM-dd
    const [anoInicio, mesInicio, diaInicio] = this.dataInicioRelatorio.split('-').map(Number);
    const [anoFim, mesFim, diaFim] = this.dataFimRelatorio.split('-').map(Number);
    
    const dataInicio = new Date(anoInicio, mesInicio - 1, diaInicio);
    const dataFim = new Date(anoFim, mesFim - 1, diaFim);

    if (dataInicio > dataFim) {
      this.alertService.warning('Aviso', 'Data inicial não pode ser maior que data final');
      return;
    }

    this.carregandoRelatorio = true;

    let observable;
    let nomeBase = '';

    switch (this.tipoRelatorioAtual) {
      case 'produtos-vendidos':
        observable = this.relatorioService.gerarRelatorioProdutosMaisVendidos(dataInicio, dataFim);
        nomeBase = 'relatorio_produtos_vendidos';
        break;
      case 'clientes':
        observable = this.relatorioService.gerarRelatorioClientes(dataInicio, dataFim);
        nomeBase = 'relatorio_clientes';
        break;
      case 'estoque':
        observable = this.relatorioService.gerarRelatorioEstoque(dataInicio, dataFim);
        nomeBase = 'relatorio_estoque';
        break;
      case 'produtos-sem-saida':
        observable = this.relatorioService.gerarRelatorioProdutosSemSaida(dataInicio, dataFim);
        nomeBase = 'relatorio_produtos_sem_saida';
        break;
      default:
        this.carregandoRelatorio = false;
        return;
    }

    observable.subscribe({
      next: (blob: Blob) => {
        const nomeArquivo = `${nomeBase}_${this.formatarDataNome(dataInicio)}_${this.formatarDataNome(dataFim)}.pdf`;
        this.relatorioService.downloadPDF(blob, nomeArquivo);
        this.alertService.success('Sucesso', 'Relatório gerado com sucesso');
        this.fecharModalRelatorio();
        this.carregandoRelatorio = false;
      },
      error: (err) => {
        console.error('Erro ao gerar relatório', err);
        this.alertService.error('Erro', 'Não foi possível gerar o relatório');
        this.carregandoRelatorio = false;
      }
    });
  }

  abrirModalComissao(): void {
    this.mostrarModalComissao = true;
  }

  fecharModalComissao(): void {
    this.mostrarModalComissao = false;
  }

  gerarRelatorioComissao(): void {
    if (!this.dataInicioComissao || !this.dataFimComissao) {
      this.alertService.warning('Aviso', 'Selecione as datas de início e fim');
      return;
    }

    // Criar Date objects a partir das strings no formato yyyy-MM-dd
    const [anoInicio, mesInicio, diaInicio] = this.dataInicioComissao.split('-').map(Number);
    const [anoFim, mesFim, diaFim] = this.dataFimComissao.split('-').map(Number);
    
    const dataInicio = new Date(anoInicio, mesInicio - 1, diaInicio);
    const dataFim = new Date(anoFim, mesFim - 1, diaFim);

    if (dataInicio > dataFim) {
      this.alertService.warning('Aviso', 'Data inicial não pode ser maior que data final');
      return;
    }

    this.carregandoComissao = true;

    this.relatorioService.gerarRelatorioComissao(dataInicio, dataFim).subscribe({
      next: (blob: Blob) => {
        const nomeArquivo = `relatorio_comissao_${this.formatarDataNome(dataInicio)}_${this.formatarDataNome(dataFim)}.pdf`;
        this.relatorioService.downloadPDF(blob, nomeArquivo);
        this.alertService.success('Sucesso', 'Relatório gerado com sucesso');
        this.fecharModalComissao();
        this.carregandoComissao = false;
      },
      error: (err) => {
        console.error('Erro ao gerar relatório', err);
        this.alertService.error('Erro', 'Não foi possível gerar o relatório');
        this.carregandoComissao = false;
      }
    });
  }

  aplicarFiltros(): void {
    this.alertService.success('Filtros', 'Filtros aplicados!');
  }
}
