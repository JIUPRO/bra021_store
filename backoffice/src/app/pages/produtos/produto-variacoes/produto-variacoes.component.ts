import { Component, Input, OnInit, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProdutoTamanhoService, ProdutoTamanhoDTO } from '../../../services/produto-variacao.service';
import { AlertService } from '../../../services/alert.service';
import { PaginationComponent } from '../../../components/pagination/pagination.component';

@Component({
  selector: 'app-produto-variacoes',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  template: `
    <div class="variacoes-section">
      <h5 class="mb-3">
        <i class="bi bi-diagram-3 me-2"></i>Tamanhos
      </h5>

      <div class="mb-3">
        <div class="row g-2">
          <div class="col-md-12">
            <label class="form-label">Tamanho</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="novaVariacao.tamanho"
              placeholder="Ex: P, M, G, GG ou A1, A2, A3, A4"
            >
          </div>
        </div>
      </div>

      <button 
        type="button" 
        class="btn btn-sm btn-primary mb-3"
        (click)="adicionarVariacao()"
        [disabled]="!novaVariacao.tamanho"
      >
        <i class="bi bi-plus-lg me-2"></i>Adicionar Tamanho
      </button>

      <div class="table-responsive" *ngIf="variacoes.length > 0">
        <table class="table table-sm table-hover mb-0">
          <thead class="bg-light">
            <tr>
              <th>Tamanho</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let v of variacoesPaginadas">
              <td>
                <strong>{{ v.tamanho }}</strong>
              </td>
              <td>
                <span class="badge" [class]="v.ativo ? 'bg-success' : 'bg-secondary'">
                  {{ v.ativo ? 'Ativo' : 'Inativo' }}
                </span>
              </td>
              <td>
                <div class="btn-group btn-group-sm">
                  <button 
                    type="button"
                    class="btn btn-outline-primary"
                    (click)="iniciarEdicao(v)"
                    [hidden]="editandoId === v.id"
                  >
                    <i class="bi bi-pencil"></i>
                  </button>
                  <button 
                    type="button"
                    class="btn btn-outline-success"
                    (click)="salvarVariacao(v)"
                    [hidden]="editandoId !== v.id"
                  >
                    <i class="bi bi-check-lg"></i>
                  </button>
                  <button 
                    type="button"
                    class="btn btn-outline-secondary"
                    (click)="cancelarEdicao()"
                    [hidden]="editandoId !== v.id"
                  >
                    <i class="bi bi-x-lg"></i>
                  </button>
                  <button 
                    type="button"
                    class="btn btn-outline-danger"
                    (click)="removerVariacao(v.id)"
                    [hidden]="editandoId === v.id"
                  >
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div class="card-footer bg-white">
          <app-pagination
            [totalItens]="variacoes.length"
            [paginaAtual]="paginaAtual"
            [itensPorPagina]="itensPorPagina"
            (paginar)="onPaginar($event)">
          </app-pagination>
        </div>
      </div>

      <div class="alert alert-info mt-3" *ngIf="variacoes.length === 0">
        <i class="bi bi-info-circle me-2"></i>
        Nenhum tamanho adicionado. Configure os tamanhos disponíveis para este produto.
      </div>
    </div>
  `,
  styles: [`
    .variacoes-section {
      background: #f8f9fa;
      border: 1px solid #dee2e6;
      border-radius: 8px;
      padding: 15px;
      margin-top: 20px;
    }

    .table {
      background: white;
      border-radius: 6px;
    }

    .btn-group-sm .btn {
      padding: 4px 8px;
      font-size: 0.875rem;
    }
  `]
})
export class ProdutoVariacoesComponent implements OnInit {
  @Input() produtoId: string | null = null;
  @Output() variacoesChange = new EventEmitter<ProdutoTamanhoDTO[]>();

  private variacao = inject(ProdutoTamanhoService);
  private alertService = inject(AlertService);

  variacoes: ProdutoTamanhoDTO[] = [];
  variacoesPaginadas: ProdutoTamanhoDTO[] = [];
  paginaAtual = 1;
  itensPorPagina = 10;
  editandoId: string | null = null;
  variacaoBackup: ProdutoTamanhoDTO | null = null;

  novaVariacao: ProdutoTamanhoDTO = {
    produtoId: '',
    tamanho: '',
    ativo: true
  };

  ngOnInit(): void {
    if (this.produtoId) {
      this.carregarVariacoes();
    }
  }

  carregarVariacoes(): void {
    if (!this.produtoId) return;

    this.variacao.getByProdutoId(this.produtoId).subscribe({
      next: (list) => {
        this.variacoes = list;
        this.atualizarPaginacao();
        this.variacoesChange.emit(this.variacoes);
      },
      error: (err) => {
        console.error('Erro carregando variações', err);
        this.variacoes = [];
        this.variacoesChange.emit(this.variacoes);
      }
    });
  }

  adicionarVariacao(): void {
    if (!this.novaVariacao.tamanho) {
      this.alertService.warning('Atenção', 'Digite o tamanho');
      return;
    }

    // Se o produto já existe, cria via API
    if (this.produtoId) {
      this.novaVariacao.produtoId = this.produtoId;
      this.variacao.create(this.novaVariacao).subscribe({
        next: (v) => {
          this.variacoes.push(v);
          this.atualizarPaginacao();
          this.novaVariacao = {
            produtoId: '',
            tamanho: '',
            ativo: true
          };
          this.variacoesChange.emit(this.variacoes);
        },
        error: (err) => {
          console.error('Erro adicionando variação', err);
          this.alertService.error('Erro ao adicionar variação', 'Tente novamente');
        }
      });
      return;
    }

    // Se ainda estamos criando o produto (produtoId nulo), armazena localmente
    const nova = { ...this.novaVariacao, id: `local-${Date.now()}` } as ProdutoTamanhoDTO;
    this.variacoes.push(nova);
    this.atualizarPaginacao();
    this.novaVariacao = {
      produtoId: '',
      tamanho: '',
      ativo: true
    };
    this.variacoesChange.emit(this.variacoes);
  }

  iniciarEdicao(variacao: ProdutoTamanhoDTO): void {
    this.editandoId = variacao.id || null;
    this.variacaoBackup = { ...variacao };
  }

  salvarVariacao(variacao: ProdutoTamanhoDTO): void {
    if (!variacao.id) return;

    // variação local (sem id persistido no servidor)
    if (variacao.id.toString().startsWith('local-')) {
      this.editandoId = null;
      this.variacaoBackup = null;
      this.variacoesChange.emit(this.variacoes);
      return;
    }

    this.variacao.update(variacao.id, variacao).subscribe({
      next: () => {
        this.editandoId = null;
        this.variacaoBackup = null;
        this.variacoesChange.emit(this.variacoes);
      },
      error: (err) => {
        console.error('Erro salvando variação', err);
        this.alertService.error('Erro ao salvar variação', 'Tente novamente');
        if (this.variacaoBackup) {
          const index = this.variacoes.findIndex(v => v.id === variacao.id);
          if (index >= 0) {
            this.variacoes[index] = this.variacaoBackup;
          }
        }
      }
    });
  }

  cancelarEdicao(): void {
    if (this.variacaoBackup) {
      const index = this.variacoes.findIndex(v => v.id === this.editandoId);
      if (index >= 0) {
        this.variacoes[index] = this.variacaoBackup;
      }
    }
    this.editandoId = null;
    this.variacaoBackup = null;
  }

  removerVariacao(id: string | undefined): void {
    if (!id) return;

    this.alertService.confirm('Confirmar Exclusão', 'Deseja remover esta variação de tamanho?').then(confirmado => {
      if (confirmado) {
        // se for variação local (id começa com local-), apenas remover da lista
        if (id.toString().startsWith('local-')) {
          this.variacoes = this.variacoes.filter(v => v.id !== id);
          this.atualizarPaginacao();
          this.variacoesChange.emit(this.variacoes);
          this.alertService.success('Variação removida com sucesso!');
          return;
        }

        this.variacao.delete(id).subscribe({
          next: () => {
            this.variacoes = this.variacoes.filter(v => v.id !== id);
            this.atualizarPaginacao();
            this.variacoesChange.emit(this.variacoes);
            this.alertService.success('Variação removida com sucesso!');
          },
          error: (err) => {
            console.error('Erro removendo variação', err);
            this.alertService.error('Erro ao remover variação', 'Tente novamente.');
          }
        });
      }
    });
  }

  onPaginar(event: { pagina: number; itensPorPagina: number }): void {
    this.paginaAtual = event.pagina;
    this.itensPorPagina = event.itensPorPagina;
    this.atualizarPaginacao();
  }

  private atualizarPaginacao(): void {
    const inicio = (this.paginaAtual - 1) * this.itensPorPagina;
    const fim = inicio + this.itensPorPagina;
    this.variacoesPaginadas = this.variacoes.slice(inicio, fim);
  }
}
