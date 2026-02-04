import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProdutoService, ProdutoDTO } from '../../../services/produto.service';
import { ProdutoTamanhoService, ProdutoTamanhoDTO } from '../../../services/produto-variacao.service';
import { AlertService } from '../../../services/alert.service';
import { firstValueFrom } from 'rxjs';
import { CategoriaService, CategoriaDTO } from '../../../services/categoria.service';
import { ProdutoVariacoesComponent } from '../produto-variacoes/produto-variacoes.component';

@Component({
  selector: 'app-produto-form',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ProdutoVariacoesComponent],
  template: `
    <div class="produto-form">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold mb-0">
          <i class="bi bi-box-seam me-2"></i>
          {{ editando ? 'Editar Produto' : 'Novo Produto' }}
        </h2>
        <a routerLink="/produtos" class="btn btn-outline-secondary">
          <i class="bi bi-arrow-left me-2"></i>Voltar
        </a>
      </div>

      <div class="card card-dashboard">
        <div class="card-body">
          <form (ngSubmit)="salvar()">
            <div class="row">
              <!-- Informações Básicas -->
              <div class="col-lg-8">
                <h5 class="mb-3">Informações Básicas</h5>
                
                <div class="mb-3">
                  <label class="form-label">Nome do Produto *</label>
                  <input type="text" class="form-control" [(ngModel)]="produto.nome" name="nome" required>
                </div>

                <div class="row">
                  <div class="col-md-12 mb-3">
                    <label class="form-label">Categoria *</label>
                    <select class="form-select" [(ngModel)]="produto.categoriaId" name="categoriaId" required>
                      <option value="">Selecione...</option>
                      <option *ngFor="let cat of categorias" [value]="cat.id">{{ cat.nome }}</option>
                    </select>
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label">Descrição Curta</label>
                  <input type="text" class="form-control" [(ngModel)]="produto.descricaoCurta" name="descricaoCurta" maxlength="200">
                </div>

                <div class="mb-3">
                  <label class="form-label">Descrição Completa</label>
                  <textarea class="form-control" [(ngModel)]="produto.descricao" name="descricao" rows="4"></textarea>
                </div>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Preço *</label>
                    <div class="input-group">
                      <span class="input-group-text">R$</span>
                      <input type="number" class="form-control" [(ngModel)]="produto.preco" name="preco" step="0.01" required>
                    </div>
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Preço Promocional</label>
                    <div class="input-group">
                      <span class="input-group-text">R$</span>
                      <input type="number" class="form-control" [(ngModel)]="produto.precoPromocional" name="precoPromocional" step="0.01">
                    </div>
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Valor do Frete</label>
                    <div class="input-group">
                      <span class="input-group-text">R$</span>
                      <input type="number" class="form-control" [(ngModel)]="produto.valorFrete" name="valorFrete" step="0.01">
                    </div>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Prazo de Entrega (dias)</label>
                    <input type="number" class="form-control" [(ngModel)]="produto.prazoEntregaDias" name="prazoEntregaDias" min="0" step="1">
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Peso (kg)</label>
                    <input type="number" class="form-control" [(ngModel)]="produto.peso" name="peso" step="0.001">
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Altura (cm)</label>
                    <input type="number" class="form-control" [(ngModel)]="produto.altura" name="altura" step="0.1">
                  </div>
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Largura (cm)</label>
                    <input type="number" class="form-control" [(ngModel)]="produto.largura" name="largura" step="0.1">
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label class="form-label">Profundidade (cm)</label>
                    <input type="number" class="form-control" [(ngModel)]="produto.profundidade" name="profundidade" step="0.1">
                  </div>
                </div>
              </div>

              <!-- Estoque e Configurações -->
              <div class="col-lg-4">
                <h5 class="mb-3">Estoque</h5>
                
                <div class="row">
                  <div class="col-md-12 mb-3">
                    <label class="form-label">Estoque Mínimo *</label>
                    <input type="number" class="form-control" [(ngModel)]="produto.estoqueMinimo" name="estoqueMinimo" required>
                  </div>
                </div>

                <h5 class="mb-3 mt-4">Configurações</h5>
                
                <div class="mb-3">
                  <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" [(ngModel)]="produto.ativo" name="ativo" id="ativo">
                    <label class="form-check-label" for="ativo">Produto Ativo</label>
                  </div>
                </div>

                <div class="mb-3">
                  <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" [(ngModel)]="produto.destaque" name="destaque" id="destaque">
                    <label class="form-check-label" for="destaque">Produto em Destaque</label>
                  </div>
                </div>

                <h5 class="mb-3 mt-4">Imagem</h5>
                
                <div class="mb-3">
                  <div class="border rounded p-3">
                    <div class="text-center mb-2">
                      <img
                        [src]="produto.imagemUrl || 'assets/produto-sem-imagem.jpg'"
                        class="img-fluid"
                        style="max-height: 150px;"
                      >
                    </div>
                    <label class="form-label">URL da Imagem</label>
                    <input type="url" class="form-control" [(ngModel)]="produto.imagemUrl" name="imagemUrl" placeholder="https://exemplo.com/imagem.jpg">
                    <small class="form-text text-muted">Por enquanto cole a URL retornada pela API de upload. Futuramente teremos upload direto.</small>
                  </div>
                </div>
              </div>

              <!-- Variações de Tamanho -->
              <div class="col-lg-12">
                <app-produto-variacoes [produtoId]="produtoId" (variacoesChange)="onVariacoesChange($event)"></app-produto-variacoes>
              </div>
            </div>

            <hr class="my-4">

            <div class="d-flex justify-content-end gap-2">
              <a routerLink="/produtos" class="btn btn-outline-secondary">Cancelar</a>
              <button type="submit" class="btn btn-primary">
                <i class="bi bi-check-lg me-2"></i>Salvar Produto
              </button>
            </div>
          </form>
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
    
    .form-label {
      font-weight: 500;
    }
    
    .form-control:focus,
    .form-select:focus {
      border-color: #0d6efd;
      box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
    }
  `]
})
export class ProdutoFormComponent implements OnInit {
  private produtoService = inject(ProdutoService);
  private categoriaService = inject(CategoriaService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private produtoTamanhoService = inject(ProdutoTamanhoService);
  private alertService = inject(AlertService);

  editando = false;
  produtoId: string | null = null;

  // variações locais (usadas durante criação ou para manter sincronia)
  variacoes: ProdutoTamanhoDTO[] = [];

  produto = {
    nome: '',
    categoriaId: '',
    descricao: '',
    descricaoCurta: '',
    preco: 0,
    precoPromocional: null as number | null,
    valorFrete: 0,
    prazoEntregaDias: 0,
    peso: 0,
    altura: null as number | null,
    largura: null as number | null,
    profundidade: null as number | null,
    estoqueMinimo: 5,
    ativo: true,
    destaque: false,
    imagemUrl: ''
  };

  categorias: CategoriaDTO[] = [];

  ngOnInit(): void {
    this.carregarCategorias();
    this.produtoId = this.route.snapshot.paramMap.get('id');
    this.editando = !!this.produtoId;
    
    if (this.editando) {
      this.carregarProduto();
    }
  }

  onVariacoesChange(list: ProdutoTamanhoDTO[]): void {
    this.variacoes = list || [];
  }

  carregarCategorias(): void {
    this.categoriaService.getAll().subscribe({
      next: (list) => this.categorias = list,
      error: (err) => {
        console.error('Erro carregando categorias', err);
        this.categorias = [];
      }
    });
  }

  carregarProduto(): void {
    if (!this.produtoId) return;
    
    this.produtoService.getById(this.produtoId).subscribe({
      next: (p) => {
        this.produto = {
          nome: p.nome || '',
          categoriaId: p.categoriaId || '',
          descricao: p.descricao || '',
          descricaoCurta: p.descricaoCurta || '',
          preco: p.preco || 0,
          precoPromocional: p.precoPromocional ?? null,
          valorFrete: p.valorFrete || 0,
          prazoEntregaDias: p.prazoEntregaDias || 0,
          peso: p.peso || 0,
          altura: p.altura ?? null,
          largura: p.largura ?? null,
          profundidade: p.profundidade ?? null,
          estoqueMinimo: p.quantidadeMinimaEstoque || 5,
          ativo: p.ativo ?? true,
          destaque: p.destaque ?? false,
          imagemUrl: p.imagemUrl || ''
        };
      },
      error: (err) => {
        console.error('Erro carregando produto', err);
        this.alertService.error('Erro ao carregar produto', 'Tente novamente');
      }
    });
  }

  salvar(): void {
    const produtoDTO: any = {
      // para updates a API exige que o Id também venha no corpo
      id: this.editando && this.produtoId ? this.produtoId : undefined,
      nome: this.produto.nome,
      categoriaId: this.produto.categoriaId,
      descricao: this.produto.descricao,
      descricaoCurta: this.produto.descricaoCurta,
      preco: this.produto.preco,
      precoPromocional: this.produto.precoPromocional,
      valorFrete: this.produto.valorFrete,
      prazoEntregaDias: this.produto.prazoEntregaDias,
      peso: this.produto.peso,
      altura: this.produto.altura,
      largura: this.produto.largura,
      profundidade: this.produto.profundidade,
      quantidadeMinimaEstoque: this.produto.estoqueMinimo,
      ativo: this.produto.ativo,
      destaque: this.produto.destaque,
      imagemUrl: this.produto.imagemUrl
    };
    // atualiza produto existente
    if (this.editando && this.produtoId) {
      this.produtoService.update(this.produtoId, produtoDTO).subscribe({
        next: () => {
            this.alertService.success('Produto atualizado', 'Produto atualizado com sucesso!').then(() => {
              this.router.navigate(['/produtos']);
            });
          },
          error: (err) => {
            console.error('Erro atualizando produto', err);
            this.alertService.error('Erro ao salvar produto', 'Tente novamente');
          }
      });
      return;
    }

    // criando novo produto: primeiro cria produto, depois cria variações (se houver)
    this.produtoService.create(produtoDTO).subscribe({
      next: (created: ProdutoDTO) => {
        // se houver variações locais, persistir cada uma com o produtoId criado
        if (this.variacoes.length > 0) {
          // criar variações sequencialmente via Promises
          const criarPromises = this.variacoes.map(v => {
            v.produtoId = created.id;
            return firstValueFrom(this.produtoTamanhoService.create(v));
          });

          Promise.all(criarPromises).then(() => {
            this.alertService.success('Produto criado', 'Produto criado com sucesso!').then(() => {
              this.router.navigate(['/produtos']);
            });
          }).catch((err) => {
            console.error('Erro criando variações após criar produto', err);
            this.alertService.warning('Atenção', 'Produto criado, mas houve erro ao salvar variações').then(() => {
              this.router.navigate(['/produtos']);
            });
          });
        } else {
          this.alertService.success('Produto criado', 'Produto criado com sucesso!').then(() => {
            this.router.navigate(['/produtos']);
          });
        }
      },
      error: (err) => {
        console.error('Erro criando produto', err);
        this.alertService.error('Erro ao salvar produto', 'Tente novamente');
      }
    });
  }
}
