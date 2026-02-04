import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CarrinhoService } from '../../services/carrinho.service';
import { AlertService } from '../../services/alert.service';
import { ItemCarrinho } from '../../models/produto.model';

@Component({
  selector: 'app-carrinho',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <h1 class="fw-bold mb-4">
        <i class="bi bi-cart3 me-2"></i>Meu Carrinho
      </h1>

      <div *ngIf="itens.length === 0" class="text-center py-5">
        <i class="bi bi-cart-x fs-1 text-muted"></i>
        <h4 class="mt-3">Seu carrinho está vazio</h4>
        <p class="text-muted">Adicione produtos para continuar comprando.</p>
        <a routerLink="/produtos" class="btn btn-primary btn-lg">
          <i class="bi bi-grid me-2"></i>Ver Produtos
        </a>
      </div>

      <div *ngIf="itens.length > 0" class="row">
        <!-- Lista de Itens -->
        <div class="col-lg-8">
          <div class="card mb-3" *ngFor="let item of itens">
            <div class="card-body">
              <div class="row align-items-center">
                <div class="col-3 col-md-2">
                  <img 
                    [src]="item.produto.imagemUrl || 'assets/images/produto-sem-imagem.jpg'" 
                    [alt]="item.produto.nome"
                    class="img-fluid rounded"
                    style="max-height: 100px; object-fit: contain; background: #f8f9fa;"
                  >
                </div>
                <div class="col-9 col-md-4">
                  <h5 class="mb-1">{{ item.produto.nome }}</h5>
                  <small class="text-muted">{{ item.produto.nomeCategoria }}</small>
                  <div *ngIf="item.tamanhoVariacao" class="text-muted small">
                    Tamanho: {{ item.tamanhoVariacao }}
                  </div>
                  <p class="mb-0 mt-1">
                    <span class="fw-bold">R$ {{ precoUnitario(item) | number:'1.2-2' }}</span>
                    <span *ngIf="item.produto.precoPromocional" class="text-muted text-decoration-line-through ms-2 small">
                      R$ {{ item.produto.preco | number:'1.2-2' }}
                    </span>
                  </p>
                </div>
                <div class="col-6 col-md-3 mt-3 mt-md-0">
                  <div class="input-group">
                    <button class="btn btn-outline-secondary" (click)="diminuirQuantidade(item)">
                      <i class="bi bi-dash"></i>
                    </button>
                    <input type="number" class="form-control text-center" [(ngModel)]="item.quantidade" (change)="atualizarQuantidade(item)" min="1">
                    <button class="btn btn-outline-secondary" (click)="aumentarQuantidade(item)">
                      <i class="bi bi-plus"></i>
                    </button>
                  </div>
                </div>
                <div class="col-6 col-md-2 mt-3 mt-md-0 text-end">
                  <p class="fw-bold mb-0">R$ {{ calcularSubtotalItem(item) | number:'1.2-2' }}</p>
                </div>
                <div class="col-12 col-md-1 mt-3 mt-md-0 text-end">
                  <button class="btn btn-outline-danger btn-sm" (click)="removerItem(item)">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div class="d-flex justify-content-between align-items-center mt-4">
            <button class="btn btn-outline-secondary" (click)="limparCarrinho()">
              <i class="bi bi-trash me-2"></i>Limpar Carrinho
            </button>
            <a routerLink="/produtos" class="btn btn-outline-primary">
              <i class="bi bi-arrow-left me-2"></i>Continuar Comprando
            </a>
          </div>
        </div>

        <!-- Resumo do Pedido -->
        <div class="col-lg-4 mt-4 mt-lg-0">
          <div class="card">
            <div class="card-header bg-white">
              <h5 class="mb-0"><i class="bi bi-receipt me-2"></i>Resumo do Pedido</h5>
            </div>
            <div class="card-body">
              <div class="d-flex justify-content-between mb-2">
                <span>Subtotal ({{ quantidadeItens }} itens)</span>
                <span>R$ {{ subtotal | number:'1.2-2' }}</span>
              </div>
              <div class="d-flex justify-content-between mb-2">
                <span>Frete</span>
                <span class="text-muted">R$ {{ calcularFrete() | number:'1.2-2' }}</span>
              </div>
              <div class="d-flex justify-content-between mb-2">
                <span>Prazo de entrega</span>
                <span class="text-muted">{{ calcularPrazoEntrega() }} dias</span>
              </div>
              <div class="d-flex justify-content-between mb-2">
                <span>Desconto</span>
                <span class="text-success">- R$ 0,00</span>
              </div>
              <hr>
              <div class="d-flex justify-content-between mb-4">
                <span class="fw-bold fs-5">Total</span>
                <span class="fw-bold fs-5 text-primary">R$ {{ (subtotal + calcularFrete()) | number:'1.2-2' }}</span>
              </div>
              <button class="btn btn-primario w-100 btn-lg" (click)="finalizarCompra()">
                <i class="bi bi-check-circle me-2"></i>Finalizar Compra
              </button>
            </div>
          </div>

          <div class="card mt-3">
            <div class="card-body">
              <h6 class="fw-bold mb-3"><i class="bi bi-truck me-2"></i>Frete e Entrega</h6>
              <p class="text-muted small mb-0">
                O frete e o prazo exibidos consideram o maior valor entre os itens do carrinho.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .btn-primario {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      border: none;
      border-radius: 8px;
      padding: 12px 24px;
      font-weight: 600;
      transition: all 0.3s ease;
      color: var(--cor-escura);
    }
    
    .btn-primario:hover {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(47,106,73,0.18);
      color: var(--cor-escura);
    }
  `]
})
export class CarrinhoComponent implements OnInit {
  itens: ItemCarrinho[] = [];

  constructor(
    private carrinhoService: CarrinhoService,
    private router: Router,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.carrinhoService.carrinho$.subscribe(itens => {
      this.itens = itens;
    });
  }

  get quantidadeItens(): number {
    return this.itens.reduce((total, item) => total + item.quantidade, 0);
  }

  get subtotal(): number {
    return this.carrinhoService.calcularSubtotal();
  }

  calcularFrete(): number {
    return this.itens.reduce((maiorFrete, item) => {
      const freteItem = item.produto.valorFrete || 0;
      return freteItem > maiorFrete ? freteItem : maiorFrete;
    }, 0);
  }

  calcularPrazoEntrega(): number {
    return this.itens.reduce((maiorPrazo, item) => {
      const prazoItem = item.produto.prazoEntregaDias || 0;
      return prazoItem > maiorPrazo ? prazoItem : maiorPrazo;
    }, 0);
  }

  precoUnitario(item: ItemCarrinho): number {
    return item.produto.precoPromocional || item.produto.preco;
  }

  calcularSubtotalItem(item: ItemCarrinho): number {
    return this.precoUnitario(item) * item.quantidade;
  }

  aumentarQuantidade(item: ItemCarrinho): void {
    this.carrinhoService.atualizarQuantidade(item.produto.id, item.quantidade + 1);
  }

  diminuirQuantidade(item: ItemCarrinho): void {
    if (item.quantidade > 1) {
      this.carrinhoService.atualizarQuantidade(item.produto.id, item.quantidade - 1);
    }
  }

  atualizarQuantidade(item: ItemCarrinho): void {
    if (item.quantidade < 1) {
      item.quantidade = 1;
    }
    this.carrinhoService.atualizarQuantidade(item.produto.id, item.quantidade);
  }

  async removerItem(item: ItemCarrinho): Promise<void> {
    const confirmado = await this.alertService.confirm('Remover item', 'Deseja remover este item do carrinho?');
    if (confirmado) {
      this.carrinhoService.removerProduto(item.produto.id);
    }
  }

  async limparCarrinho(): Promise<void> {
    const confirmado = await this.alertService.confirm('Limpar carrinho', 'Deseja limpar todo o carrinho?');
    if (confirmado) {
      this.carrinhoService.limparCarrinho();
    }
  }

  finalizarCompra(): void {
    this.router.navigate(['/checkout']);
  }
}
