import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EstoqueService, MovimentacaoEstoqueDTO, AlertaEstoqueDTO, ResumoMovimentacaoEstoqueDTO } from '../../services/estoque.service';
import { ProdutoService, ProdutoDTO } from '../../services/produto.service';
import { ProdutoTamanhoService, ProdutoTamanhoDTO } from '../../services/produto-variacao.service';
import { AlertService } from '../../services/alert.service';
import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-estoque',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  template: `
    <div class="estoque-page">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0"><i class="bi bi-boxes me-2"></i>Controle de Estoque</h2>
        <button class="btn btn-primary" (click)="novaMovimentacao()">
          <i class="bi bi-plus-lg me-2"></i>Nova Movimentação
        </button>
      </div>

      <!-- Modal Nova Movimentação -->
      <div class="card card-dashboard mb-4" *ngIf="mostrarFormMovimentacao">
        <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
          <h5 class="mb-0"><i class="bi bi-plus-lg me-2"></i>Nova Movimentação de Estoque</h5>
          <button class="btn btn-sm btn-light" (click)="cancelarMovimentacao()">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
        <div class="card-body">
          <form #movForm="ngForm">
            <div class="row">
              <div class="col-md-6 mb-3">
                <label class="form-label">Produto *</label>
                <select class="form-select" [(ngModel)]="novaMovimentacaoForm.produtoId" 
                        name="produtoId" required (change)="onProdutoChange()">
                  <option value="">Selecione um produto</option>
                  <option *ngFor="let produto of produtos" [value]="produto.id">
                    {{ produto.nome }}
                  </option>
                </select>
              </div>

              <div class="col-md-6 mb-3">
                <label class="form-label">Tamanho *</label>
                <select class="form-select" [(ngModel)]="novaMovimentacaoForm.produtoTamanhoId" 
                        name="tamanhoId" required [disabled]="!novaMovimentacaoForm.produtoId || tamanhos.length === 0">
                  <option value="">{{ carregandoTamanhos ? 'Carregando...' : 'Selecione um tamanho' }}</option>
                  <option *ngFor="let tamanho of tamanhos" [value]="tamanho.id">
                    {{ tamanho.tamanho }}
                  </option>
                </select>
                <small class="text-muted" *ngIf="!novaMovimentacaoForm.produtoId">
                  Selecione um produto primeiro
                </small>
              </div>

              <div class="col-md-4 mb-3">
                <label class="form-label">Tipo *</label>
                <select class="form-select" [(ngModel)]="novaMovimentacaoForm.tipo" name="tipo" required>
                  <option [ngValue]="1">Entrada</option>
                  <option [ngValue]="2">Saída</option>
                  <option [ngValue]="3">Ajuste</option>
                  <option [ngValue]="4">Devolução</option>
                </select>
              </div>

              <div class="col-md-4 mb-3">
                <label class="form-label">Quantidade *</label>
                <input type="number" class="form-control" [(ngModel)]="novaMovimentacaoForm.quantidade" 
                       name="quantidade" min="1" required>
              </div>

              <div class="col-md-4 mb-3">
                <label class="form-label">Referência</label>
                <input type="text" class="form-control" [(ngModel)]="novaMovimentacaoForm.referencia" 
                       name="referencia" placeholder="Ex: NF, pedido">
              </div>

              <div class="col-md-12 mb-3">
                <label class="form-label">Motivo</label>
                <textarea class="form-control" [(ngModel)]="novaMovimentacaoForm.motivo" 
                          name="motivo" rows="2" placeholder="Descreva o motivo da movimentação"></textarea>
              </div>
            </div>

            <div class="d-flex justify-content-end gap-2">
              <button type="button" class="btn btn-secondary" (click)="cancelarMovimentacao()">
                Cancelar
              </button>
              <button type="button" class="btn btn-primary" 
                      [disabled]="!movForm.valid || salvandoMovimentacao"
                      (click)="salvarMovimentacao()">
                <span *ngIf="salvandoMovimentacao" class="spinner-border spinner-border-sm me-2"></span>
                {{ salvandoMovimentacao ? 'Salvando...' : 'Salvar Movimentação' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Alertas -->
      <div class="alert alert-warning d-flex align-items-center mb-4" *ngIf="alertas.length > 0">
        <i class="bi bi-exclamation-triangle-fill me-2 fs-4"></i>
        <div>
          <strong>Atenção!</strong> Existem {{ alertas.length }} produtos com estoque baixo.
        </div>
      </div>

      <!-- Produtos com Estoque Baixo -->
      <div class="card card-dashboard mb-4" *ngIf="estoqueBaixo.length > 0">
        <div class="card-header bg-danger text-white">
          <h5 class="mb-0"><i class="bi bi-exclamation-circle me-2"></i>Estoque Baixo</h5>
        </div>
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Produto</th>
                  <th>Estoque Atual</th>
                  <th>Mínimo</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let item of estoqueBaixoPaginado" class="table-danger">
                  <td><strong>{{ item.nomeProduto }}</strong></td>
                  <td class="fw-bold text-danger">{{ item.quantidadeEstoque }}</td>
                  <td>{{ item.quantidadeMinima }}</td>
                  <td>
                    <button class="btn btn-sm btn-primary" (click)="reporEstoque(item)">
                      <i class="bi bi-plus-lg me-1"></i>Repor
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="card-footer bg-white">
          <app-pagination
            [totalItens]="estoqueBaixo.length"
            [paginaAtual]="paginaAtualEstoqueBaixo"
            [itensPorPagina]="itensPorPaginaEstoqueBaixo"
            (paginar)="onPaginarEstoqueBaixo($event)">
          </app-pagination>
        </div>
      </div>

      <!-- Movimentações -->
      <div class="card card-dashboard">
        <div class="card-header bg-white">
          <div class="mb-3">
            <h5 class="mb-0"><i class="bi bi-clock-history me-2"></i>Movimentações de Estoque</h5>
          </div>
          
          <div class="row g-3 align-items-end">
            <div class="col-md-3">
              <label class="form-label">Produto</label>
              <div class="input-group">
                <span class="input-group-text"><i class="bi bi-search"></i></span>
                <select class="form-select" [(ngModel)]="produtoFiltroId" (change)="onProdutoFiltroChange()">
                  <option value="">Todos os produtos</option>
                  <option *ngFor="let produto of produtos" [value]="produto.id">
                    {{ produto.nome }}
                  </option>
                </select>
              </div>
            </div>

            <div class="col-md-2">
              <label class="form-label">Tamanho</label>
              <select class="form-select" [(ngModel)]="produtoTamanhoFiltroId" [disabled]="!produtoFiltroId || tamanhosFiltro.length === 0">
                <option value="">{{ produtoFiltroId ? 'Todos os tamanhos' : 'Selecione o produto' }}</option>
                <option *ngFor="let tamanho of tamanhosFiltro" [value]="tamanho.id">
                  {{ tamanho.tamanho }}
                </option>
              </select>
            </div>

            <div class="col-md-2">
              <label class="form-label">Data inicial</label>
              <input type="date" class="form-control" [(ngModel)]="dataInicioFiltro" (ngModelChange)="onFiltroPeriodoChange()" [disabled]="!produtoFiltroId">
            </div>

            <div class="col-md-2">
              <label class="form-label">Data final</label>
              <input type="date" class="form-control" [(ngModel)]="dataFimFiltro" (ngModelChange)="onFiltroPeriodoChange()" [disabled]="!produtoFiltroId">
            </div>

            <div class="col-md-3">
              <div class="d-flex gap-2">
                <button class="btn btn-success flex-grow-1" (click)="aplicarFiltro()">
                  <i class="bi bi-funnel me-1"></i>Aplicar filtros
                </button>
                <button class="btn btn-outline-secondary" (click)="limparFiltro()">
                  <i class="bi bi-arrow-counterclockwise me-1"></i>Limpar
                </button>
              </div>
            </div>
          </div>

          <div class="mt-3" *ngIf="!produtoFiltroId">
            <small class="text-muted">
              Selecione um produto para consultar saldo e movimentações de estoque.
            </small>
          </div>

          <div class="row g-3 mt-1" *ngIf="mostrarResumoPeriodo">
            <div class="col-md-4">
              <div class="resumo-card resumo-inicial">
                <small class="text-muted d-block">Saldo na data inicial</small>
                <strong>{{ resumoMovimentacoes.saldoInicialPeriodo }}</strong>
              </div>
            </div>
            <div class="col-md-4">
              <div class="resumo-card resumo-movimentacoes">
                <small class="text-muted d-block">Movimentações no período</small>
                <strong>{{ resumoMovimentacoes.totalMovimentacoes }}</strong>
              </div>
            </div>
            <div class="col-md-4">
              <div class="resumo-card resumo-atual">
                <small class="text-muted d-block">Saldo atual</small>
                <strong>{{ resumoMovimentacoes.saldoAtual }}</strong>
              </div>
            </div>
          </div>
        </div>
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Produto</th>
                  <th>Tamanho</th>
                  <th>Tipo</th>
                  <th>Quantidade</th>
                  <th>Motivo</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngIf="!produtoFiltroId">
                  <td colspan="6" class="text-center text-muted py-4">
                    Selecione um produto para visualizar as movimentações de estoque.
                  </td>
                </tr>
                <tr *ngIf="produtoFiltroId && !filtroAplicado">
                  <td colspan="6" class="text-center text-muted py-4">
                    Clique em <strong>Aplicar filtros</strong> para consultar as movimentações deste produto.
                  </td>
                </tr>
                <tr *ngIf="produtoFiltroId && filtroAplicado && movimentacoesFiltradas.length === 0">
                  <td colspan="6" class="text-center text-muted py-4">
                    Nenhuma movimentação encontrada para os filtros informados.
                  </td>
                </tr>
                <tr *ngFor="let mov of (filtroAplicado ? movimentacoesPaginadas : [])">
                  <td>{{ mov.dataMovimentacao | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td>{{ mov.nomeProduto }}</td>
                  <td><span class="badge bg-secondary">{{ mov.tamanho }}</span></td>
                  <td>
                    <span class="badge" [ngClass]="getTipoCor(mov.tipo)">
                      {{ mov.tipoDescricao }}
                    </span>
                  </td>
                  <td [class]="mov.tipo === 1 || mov.tipo === 3 || mov.tipo === 4 ? 'text-success' : 'text-danger'">
                    {{ mov.tipo === 1 || mov.tipo === 3 || mov.tipo === 4 ? '+' : '-' }}{{ mov.quantidade }}
                  </td>
                  <td>{{ mov.motivo }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="card-footer bg-white" *ngIf="produtoFiltroId && filtroAplicado">
          <app-pagination
            [totalItens]="movimentacoesFiltradas.length"
            [paginaAtual]="paginaAtualMovimentacoes"
            [itensPorPagina]="itensPorPaginaMovimentacoes"
            (paginar)="onPaginarMovimentacoes($event)">
          </app-pagination>
        </div>
      </div>
    </div>
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

    .resumo-card {
      border-radius: 12px;
      padding: 14px 16px;
      border: 1px solid #e5e7eb;
      background: #f8fafc;
    }

    .resumo-card strong {
      font-size: 1.5rem;
      line-height: 1.2;
      color: #14532d;
    }

    .resumo-inicial {
      background: #f0fdf4;
      border-color: #bbf7d0;
    }

    .resumo-movimentacoes {
      background: #f7fee7;
      border-color: #d9f99d;
    }

    .resumo-atual {
      background: #ecfdf5;
      border-color: #a7f3d0;
    }
  `]
})
export class EstoqueComponent implements OnInit {
  private estoqueService = inject(EstoqueService);
  private produtoService = inject(ProdutoService);
  private tamanhoService = inject(ProdutoTamanhoService);
  private alertService = inject(AlertService);

  alertas: AlertaEstoqueDTO[] = [];
  estoqueBaixo: AlertaEstoqueDTO[] = [];
  estoqueBaixoPaginado: AlertaEstoqueDTO[] = [];
  movimentacoes: MovimentacaoEstoqueDTO[] = [];
  movimentacoesFiltradas: MovimentacaoEstoqueDTO[] = [];
  movimentacoesPaginadas: MovimentacaoEstoqueDTO[] = [];
  resumoMovimentacoes: ResumoMovimentacaoEstoqueDTO = {
    saldoInicialPeriodo: 0,
    saldoAtual: 0,
    totalMovimentacoes: 0,
    movimentacoes: []
  };
  mostrarResumoPeriodo = false;
  filtroAplicado = false;
  produtos: ProdutoDTO[] = [];
  tamanhos: ProdutoTamanhoDTO[] = [];
  tamanhosFiltro: ProdutoTamanhoDTO[] = [];
  produtoFiltroId: string = '';
  produtoTamanhoFiltroId: string = '';
  dataInicioFiltro: string = '';
  dataFimFiltro: string = '';
  paginaAtualEstoqueBaixo = 1;
  itensPorPaginaEstoqueBaixo = 10;
  paginaAtualMovimentacoes = 1;
  itensPorPaginaMovimentacoes = 10;
  
  mostrarFormMovimentacao = false;
  salvandoMovimentacao = false;
  carregandoTamanhos = false;
  
  novaMovimentacaoForm = {
    produtoId: '',
    produtoTamanhoId: '',
    tipo: 1,
    quantidade: 1,
    motivo: '',
    referencia: ''
  };

  ngOnInit(): void {
    this.carregarDados();
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.produtoService.getAll().subscribe({
      next: (list) => this.produtos = list.filter(p => p.ativo),
      error: (err) => {
        console.error('Erro carregando produtos', err);
        this.produtos = [];
      }
    });
  }

  carregarDados(): void {
    this.estoqueService.getAlertas().subscribe({
      next: (list) => {
        this.alertas = list;
        this.estoqueBaixo = list.filter(a => a.diferenca < 0);
        this.atualizarPaginacaoEstoqueBaixo();
      },
      error: (err) => {
        console.error('Erro carregando alertas', err);
        this.alertas = [];
        this.estoqueBaixo = [];
      }
    });

    this.estoqueService.getMovimentacoes().subscribe({
      next: (list) => {
        this.movimentacoes = list;
        this.movimentacoesFiltradas = [];
        this.resumoMovimentacoes = {
          saldoInicialPeriodo: 0,
          saldoAtual: this.calcularSaldoAtualLista(list),
          totalMovimentacoes: list.length,
          movimentacoes: list
        };
        this.atualizarPaginacaoMovimentacoes();
      },
      error: (err) => {
        console.error('Erro carregando movimentações', err);
        this.movimentacoes = [];
        this.movimentacoesFiltradas = [];
      }
    });
  }

  novaMovimentacao(): void {
    this.mostrarFormMovimentacao = true;
    this.tamanhos = [];
    this.novaMovimentacaoForm = {
      produtoId: '',
      produtoTamanhoId: '',
      tipo: 1,
      quantidade: 1,
      motivo: '',
      referencia: ''
    };
  }

  onProdutoChange(): void {
    this.novaMovimentacaoForm.produtoTamanhoId = '';
    this.tamanhos = [];
    
    if (this.novaMovimentacaoForm.produtoId) {
      this.carregandoTamanhos = true;
      this.tamanhoService.getByProdutoId(this.novaMovimentacaoForm.produtoId).subscribe({
        next: (list) => {
          this.tamanhos = list.filter(t => t.ativo);
          this.carregandoTamanhos = false;
          if (this.tamanhos.length === 0) {
            this.alertService.warning('Atenção', 'Este produto não possui tamanhos cadastrados.');
          }
        },
        error: (err) => {
          console.error('Erro ao carregar tamanhos', err);
          this.tamanhos = [];
          this.carregandoTamanhos = false;
          this.alertService.error('Erro', 'Não foi possível carregar os tamanhos do produto.');
        }
      });
    }
  }

  cancelarMovimentacao(): void {
    this.mostrarFormMovimentacao = false;
  }

  salvarMovimentacao(): void {
    if (!this.novaMovimentacaoForm.produtoTamanhoId) {
      this.alertService.warning('Atenção', 'Selecione um tamanho.');
      return;
    }

    this.salvandoMovimentacao = true;
    
    // Enviar apenas os campos necessários para o backend
    const payload = {
      produtoTamanhoId: this.novaMovimentacaoForm.produtoTamanhoId,
      quantidade: this.novaMovimentacaoForm.quantidade,
      tipo: this.novaMovimentacaoForm.tipo,
      motivo: this.novaMovimentacaoForm.motivo || undefined,
      referencia: this.novaMovimentacaoForm.referencia || undefined
    };
    
    this.estoqueService.criarMovimentacao(payload).subscribe({
      next: () => {
        this.alertService.success('Sucesso', 'Movimentação registrada com sucesso!');
        this.salvandoMovimentacao = false;
        this.mostrarFormMovimentacao = false;
        this.carregarDados();
      },
      error: (err) => {
        console.error('Erro ao criar movimentação', err);
        this.alertService.error('Erro', 'Não foi possível registrar a movimentação.');
        this.salvandoMovimentacao = false;
      }
    });
  }

  reporEstoque(item: any): void {
    this.alertService.question('Repor Estoque', 'Repor estoque de: ' + item.nomeProduto).then(confirm => {
      if (confirm) {
        // implementar ação de reposição quando houver endpoint
      }
    });
  }

  getTipoCor(tipo: number): string {
    switch (tipo) {
      case 1: return 'bg-success';   // Entrada
      case 2: return 'bg-danger';    // Saída
      case 3: return 'bg-warning';   // Ajuste
      case 4: return 'bg-info';      // Devolução
      default: return 'bg-secondary';
    }
  }

  onProdutoFiltroChange(): void {
    this.produtoTamanhoFiltroId = '';
    this.tamanhosFiltro = [];
    this.filtroAplicado = false;
    this.mostrarResumoPeriodo = false;

    if (!this.produtoFiltroId) {
      return;
    }

    this.tamanhoService.getByProdutoId(this.produtoFiltroId).subscribe({
      next: (list) => {
        this.tamanhosFiltro = list.filter(t => t.ativo);
      },
      error: (err) => {
        console.error('Erro ao carregar tamanhos do filtro', err);
        this.tamanhosFiltro = [];
      }
    });
  }

  aplicarFiltro(): void {
    if (!this.produtoFiltroId) {
      this.alertService.warning('Atenção', 'Selecione um produto para consultar o estoque.');
      return;
    }

    if (this.dataInicioFiltro && this.dataFimFiltro && this.dataInicioFiltro > this.dataFimFiltro) {
      this.alertService.warning('Atenção', 'A data inicial não pode ser maior que a data final.');
      return;
    }

    this.estoqueService.consultarMovimentacoes({
      produtoId: this.produtoFiltroId || undefined,
      produtoTamanhoId: this.produtoTamanhoFiltroId || undefined,
      dataInicio: this.dataInicioFiltro || undefined,
      dataFim: this.dataFimFiltro || undefined
    }).subscribe({
      next: (resumo) => {
        this.resumoMovimentacoes = resumo;
        this.mostrarResumoPeriodo = true;
        this.filtroAplicado = true;
        this.movimentacoesFiltradas = resumo.movimentacoes;
        this.paginaAtualMovimentacoes = 1;
        this.atualizarPaginacaoMovimentacoes();
      },
      error: (err) => {
        console.error('Erro ao consultar movimentações', err);
        this.alertService.error('Erro', 'Não foi possível consultar as movimentações de estoque.');
      }
    });
  }

  limparFiltro(): void {
    this.produtoFiltroId = '';
    this.produtoTamanhoFiltroId = '';
    this.dataInicioFiltro = '';
    this.dataFimFiltro = '';
    this.mostrarResumoPeriodo = false;
    this.filtroAplicado = false;
    this.tamanhosFiltro = [];
    this.movimentacoesFiltradas = [];
    this.resumoMovimentacoes = {
      saldoInicialPeriodo: 0,
      saldoAtual: this.calcularSaldoAtualLista(this.movimentacoes),
      totalMovimentacoes: this.movimentacoes.length,
      movimentacoes: this.movimentacoes
    };
    this.paginaAtualMovimentacoes = 1;
    this.atualizarPaginacaoMovimentacoes();
  }

  onPaginarEstoqueBaixo(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtualEstoqueBaixo = event.pagina;
    this.itensPorPaginaEstoqueBaixo = event.itensPorPagina;
    this.atualizarPaginacaoEstoqueBaixo();
  }

  onPaginarMovimentacoes(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtualMovimentacoes = event.pagina;
    this.itensPorPaginaMovimentacoes = event.itensPorPagina;
    this.atualizarPaginacaoMovimentacoes();
  }

  private atualizarPaginacaoEstoqueBaixo(): void {
    const inicio = (this.paginaAtualEstoqueBaixo - 1) * this.itensPorPaginaEstoqueBaixo;
    const fim = inicio + this.itensPorPaginaEstoqueBaixo;
    this.estoqueBaixoPaginado = this.estoqueBaixo.slice(inicio, fim);
  }

  private atualizarPaginacaoMovimentacoes(): void {
    const inicio = (this.paginaAtualMovimentacoes - 1) * this.itensPorPaginaMovimentacoes;
    const fim = inicio + this.itensPorPaginaMovimentacoes;
    this.movimentacoesPaginadas = this.movimentacoesFiltradas.slice(inicio, fim);
  }

  onFiltroPeriodoChange(): void {
    this.mostrarResumoPeriodo = false;
    this.filtroAplicado = false;
  }

  private calcularSaldoAtualLista(movimentacoes: MovimentacaoEstoqueDTO[]): number {
    const mapa = new Map<string, MovimentacaoEstoqueDTO>();

    for (const movimentacao of movimentacoes) {
      const atual = mapa.get(movimentacao.produtoTamanhoId);
      if (!atual || new Date(movimentacao.dataMovimentacao) > new Date(atual.dataMovimentacao)) {
        mapa.set(movimentacao.produtoTamanhoId, movimentacao);
      }
    }

    return Array.from(mapa.values()).reduce((total, item) => total + item.estoqueAtual, 0);
  }
}

