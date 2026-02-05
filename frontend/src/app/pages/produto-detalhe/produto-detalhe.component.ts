import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProdutoService } from '../../services/produto.service';
import { CarrinhoService } from '../../services/carrinho.service';
import { AlertService } from '../../services/alert.service';
import { ProdutoVariacaoService } from '../../services/produto-variacao.service';
import { Produto, ProdutoVariacao } from '../../models/produto.model';

@Component({
  selector: 'app-produto-detalhe',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <nav aria-label="breadcrumb">
          <ol class="breadcrumb breadcrumb-estilizado mb-0">
            <li class="breadcrumb-item"><a routerLink="/">Início</a></li>
            <li class="breadcrumb-item"><a routerLink="/produtos">Produtos</a></li>
            <li class="breadcrumb-item active" aria-current="page">{{ produto?.nome }}</li>
          </ol>
        </nav>
        <button (click)="voltar()" class="btn btn-outline-secondary">
          <i class="bi bi-arrow-left me-2"></i>Voltar
        </button>
      </div>

      <div *ngIf="carregando" class="text-center py-5">
        <div class="spinner-loja mx-auto"></div>
        <p class="mt-3 text-muted">Carregando produto...</p>
      </div>

      <div *ngIf="!carregando && produto" class="row">
        <!-- Imagem do Produto -->
        <div class="col-lg-6 mb-4">
          <div class="card border-0 shadow-sm">
            <img 
              [src]="produto.imagemUrl || 'assets/images/produto-sem-imagem.jpg'" 
              [alt]="produto.nome"
              class="img-fluid rounded"
              style="max-height: 500px; object-fit: contain; width: 100%; background: #f8f9fa;"
            >
          </div>
        </div>

        <!-- Informações do Produto -->
        <div class="col-lg-6">
          <span class="badge bg-secondary mb-2">{{ produto.nomeCategoria }}</span>
          <h1 class="fw-bold mb-3">{{ produto.nome }}</h1>
          
          <div class="mb-4">
            <span *ngIf="temDesconto" class="preco-original me-3 fs-4">
              R$ {{ produto.preco | number:'1.2-2' }}
            </span>
            <span [class]="temDesconto ? 'preco-promocional' : 'preco-normal'" class="fs-1">
              R$ {{ precoAtual | number:'1.2-2' }}
            </span>
            <span *ngIf="temDesconto" class="badge bg-danger ms-3 fs-6">
              -{{ percentualDesconto }}% OFF
            </span>
          </div>

          <p class="text-muted mb-4">{{ produto.descricao }}</p>

          <div class="row mb-4">
            <div class="col-6">
              <small class="text-muted d-block">Estoque</small>
              <span [class]="estoqueDisponivelBaixo ? 'text-danger' : 'text-success'">
                <i class="bi" [class.bi-check-circle-fill]="!estoqueDisponivelBaixo" [class.bi-exclamation-circle-fill]="estoqueDisponivelBaixo"></i>
                {{ estoqueDisponivel }} unidades
              </span>
            </div>
          </div>

          <div class="row mb-4">
            <div class="col-6">
              <small class="text-muted d-block">Peso</small>
              <span>{{ produto.peso }} kg</span>
            </div>
            <div class="col-6" *ngIf="produto.altura && produto.largura && produto.profundidade">
              <small class="text-muted d-block">Dimensões</small>
              <span>{{ produto.altura }} x {{ produto.largura }} x {{ produto.profundidade }} cm</span>
            </div>
          </div>

          <div class="row mb-4">
            <div class="col-12">
              <small class="text-muted d-block">Prazo de entrega</small>
              <span>{{ produto.prazoEntregaDias }} dias</span>
            </div>
          </div>

          <hr class="my-4">

          <!-- Variações de Tamanho (se houver) -->
          <div *ngIf="variacoes.length > 0" class="mb-4">
            <label class="form-label fw-bold">Tamanho</label>
            <div class="btn-group d-flex gap-2 mb-3" role="group">
              <button 
                *ngFor="let v of variacoes"
                type="button" 
                class="btn flex-grow-1"
                [class.btn-primary]="tamanhoSelecionado === v.id"
                [class.btn-outline-secondary]="tamanhoSelecionado !== v.id"
                [disabled]="v.quantidadeEstoque === 0"
                (click)="selecionarTamanho(v)"
              >
                {{ v.tamanho }}
                <small class="d-block" *ngIf="v.quantidadeEstoque === 0">(Fora de estoque)</small>
              </button>
            </div>
          </div>

          <!-- Quantidade e Botão Comprar -->
          <div class="row align-items-center">
            <div class="col-4 col-md-3">
              <label class="form-label">Quantidade</label>
              <div class="input-group">
                <button class="btn btn-outline-secondary" (click)="diminuirQuantidade()" [disabled]="quantidade <= 1">
                  <i class="bi bi-dash"></i>
                </button>
                <input 
                  type="number" 
                  class="form-control text-center" 
                  [(ngModel)]="quantidade" 
                  (change)="validarQuantidade()"
                  min="1" 
                  [max]="estoqueDisponivel"
                >
                <button class="btn btn-outline-secondary" (click)="aumentarQuantidade()" [disabled]="quantidade >= estoqueDisponivel">
                  <i class="bi bi-plus"></i>
                </button>
              </div>
              <small class="text-muted d-block mt-1">Máx: {{ estoqueDisponivel }} unidades</small>
            </div>
            <div class="col-8 col-md-9">
              <button 
                class="btn btn-primario btn-lg w-100" 
                (click)="adicionarAoCarrinho()"
                [disabled]="!podeComprar"
              >
                <i class="bi bi-cart-plus me-2"></i>
                {{ obterMensagemBotao() }}
              </button>
            </div>
          </div>

          <div class="mt-3">
            <a routerLink="/carrinho" class="btn btn-outline-primary w-100">
              <i class="bi bi-cart me-2"></i>Ver Carrinho
            </a>
          </div>
        </div>
      </div>

      <div *ngIf="!carregando && !produto" class="text-center py-5">
        <i class="bi bi-exclamation-circle fs-1 text-muted"></i>
        <h4 class="mt-3">Produto não encontrado</h4>
        <p class="text-muted">O produto que você procura não existe ou foi removido.</p>
        <a routerLink="/produtos" class="btn btn-primary">
          <i class="bi bi-arrow-left me-2"></i>Voltar para Produtos
        </a>
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

    .breadcrumb-estilizado {
      background: linear-gradient(135deg, #f8f9fa, #e9ecef);
      padding: 12px 16px;
      border-radius: 8px;
      border: 1px solid #dee2e6;
    }

    .breadcrumb-estilizado a {
      color: var(--cor-primaria);
      text-decoration: none;
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .breadcrumb-estilizado a:hover {
      color: var(--cor-primaria-claro);
      text-decoration: underline;
    }

    .breadcrumb-estilizado .breadcrumb-item.active {
      color: #6c757d;
      font-weight: 600;
    }

    .breadcrumb-estilizado .breadcrumb-item + .breadcrumb-item::before {
      content: '›';
      color: #6c757d;
      padding: 0 8px;
      font-weight: bold;
    }
    
    .preco-original {
      text-decoration: line-through;
      color: var(--cor-clara-2);
    }
    
    .preco-promocional {
      color: #dc3545;
      font-weight: bold;
    }
    
    .preco-normal {
      color: var(--cor-primaria);
      font-weight: bold;
    }
    
    .btn-primario {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      border: none;
      border-radius: 8px;
      padding: 12px 24px;
      font-weight: 600;
      transition: all 0.3s ease;
      color: var(--cor-escura);
    }
    
    .btn-primario:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(47, 106, 73, 0.18);
      color: var(--cor-escura);
    }
    
    .btn-primario:disabled {
      background: var(--cor-secundaria);
      cursor: not-allowed;
    }
  `]
})
export class ProdutoDetalheComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private produtoService = inject(ProdutoService);
  private carrinhoService = inject(CarrinhoService);
  private variacao = inject(ProdutoVariacaoService);
  private alertService = inject(AlertService);

  produto: Produto | null = null;
  variacoes: ProdutoVariacao[] = [];
  carregando = true;
  quantidade = 1;
  tamanhoSelecionado: string | null = null;
  variacaoSelecionada: ProdutoVariacao | null = null;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.carregarProduto(id);
        this.carregarVariacoes(id);
      }
    });
  }

  carregarProduto(id: string): void {
    this.carregando = true;
    this.produtoService.obterPorId(id).subscribe({
      next: (produto) => {
        this.produto = produto;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
      }
    });
  }

  carregarVariacoes(produtoId: string): void {
    this.variacao.getByProdutoId(produtoId).subscribe({
      next: (variacoes) => {
        this.variacoes = variacoes.filter(v => v.ativo);
      },
      error: (err) => {
        console.error('Erro carregando variações', err);
        this.variacoes = [];
      }
    });
  }

  selecionarTamanho(variacao: ProdutoVariacao): void {
    this.tamanhoSelecionado = variacao.id;
    this.variacaoSelecionada = variacao;
    this.quantidade = 1;
  }

  get estoqueDisponivel(): number {
    if (this.variacaoSelecionada) {
      return this.variacaoSelecionada.quantidadeEstoque;
    }
    return this.produto?.quantidadeEstoque || 0;
  }

  get podeComprar(): boolean {
    // Se tem variações, precisa selecionar uma
    if (this.variacoes.length > 0) {
      return !!this.variacaoSelecionada && this.variacaoSelecionada.quantidadeEstoque > 0;
    }
    // Senão, apenas verifica estoque do produto
    return (this.produto?.quantidadeEstoque || 0) > 0;
  }

  obterMensagemBotao(): string {
    if (this.variacoes.length > 0 && !this.variacaoSelecionada) {
      return 'Selecione um tamanho';
    }
    if (this.estoqueDisponivel === 0) {
      return 'Indisponível';
    }
    return 'Adicionar ao Carrinho';
  }

  get temDesconto(): boolean {
    return !!this.produto?.precoPromocional && this.produto.precoPromocional < this.produto.preco;
  }

  get precoAtual(): number {
    return this.produto?.precoPromocional || this.produto?.preco || 0;
  }

  get percentualDesconto(): number {
    if (!this.temDesconto || !this.produto) return 0;
    return Math.round(((this.produto.preco - this.produto.precoPromocional!) / this.produto.preco) * 100);
  }

  get estoqueBaixo(): boolean {
    return (this.produto?.quantidadeEstoque || 0) <= (this.produto?.quantidadeMinimaEstoque || 0);
  }

  get estoqueDisponivelBaixo(): boolean {
    return this.estoqueDisponivel <= (this.produto?.quantidadeMinimaEstoque || 0);
  }

  aumentarQuantidade(): void {
    if (this.quantidade < this.estoqueDisponivel) {
      this.quantidade++;
    }
  }

  diminuirQuantidade(): void {
    if (this.quantidade > 1) {
      this.quantidade--;
    }
  }

  validarQuantidade(): void {
    if (this.quantidade < 1) {
      this.quantidade = 1;
    }
    if (this.quantidade > this.estoqueDisponivel) {
      this.quantidade = this.estoqueDisponivel;
    }
  }

  async adicionarAoCarrinho(): Promise<void> {
    if (this.produto) {
      this.carrinhoService.adicionarProduto(
        this.produto, 
        this.quantidade,
        undefined,
        this.variacaoSelecionada?.id,
        this.variacaoSelecionada?.tamanho
      );
      await this.alertService.success('Produto adicionado', 'Produto adicionado ao carrinho!');
      this.quantidade = 1;
      this.tamanhoSelecionado = null;
      this.variacaoSelecionada = null;
    }
  }

  voltar(): void {
    this.router.navigate(['/produtos']);
  }
}
