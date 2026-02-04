import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProdutoCardComponent } from '../../components/produto-card/produto-card.component';
import { ProdutoService } from '../../services/produto.service';
import { ParametroSistemaService } from '../../services/parametro-sistema.service';
import { Produto, Categoria } from '../../models/produto.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ProdutoCardComponent],
  template: `
    <!-- Banner Principal -->
    <section class="hero-section">
      <div class="container">
        <div class="row align-items-center min-vh-50">
          <div class="col-lg-6">
            <h1 class="display-4 fw-bold mb-4 fade-in">
              Bem-Vindo a Loja virtual da <span class="text-warning">Brazil-021</span><br>
              <span class="text-primary">School of Jiu-Jitsu</span>
            </h1>
            <p class="lead mb-4 fade-in">
              Descubra produtos incríveis com os melhores preços. 
              Qualidade garantida em cada compra!
            </p>
            <div class="d-flex gap-3 fade-in">
              <a routerLink="/produtos" class="btn btn-primario btn-lg">
                <i class="bi bi-grid me-2"></i>Ver Produtos
              </a>
              <a routerLink="/carrinho" class="btn btn-outline-primary btn-lg">
                <i class="bi bi-cart me-2"></i>Meu Carrinho
              </a>
            </div>
          </div>
          <div class="col-lg-6 d-none d-lg-block">
            <!-- Carrossel de Imagens -->
            <div class="carousel-container" *ngIf="imagensCarrossel.length > 0">
              <div class="carousel-wrapper">
                <div class="carousel-slides" [style.transform]="'translateX(-' + (slideAtual * 100) + '%)'">
                  <div class="carousel-slide" *ngFor="let imagem of imagensCarrossel">
                    <img 
                      [src]="imagem" 
                      alt="Banner" 
                      class="img-fluid rounded-4 shadow-lg"
                      onerror="this.style.display='none'"
                    >
                  </div>
                </div>
                
                <!-- Controles -->
                <button class="carousel-control prev" (click)="anteriorSlide()" *ngIf="imagensCarrossel.length > 1">
                  <i class="bi bi-chevron-left"></i>
                </button>
                <button class="carousel-control next" (click)="proximoSlide()" *ngIf="imagensCarrossel.length > 1">
                  <i class="bi bi-chevron-right"></i>
                </button>
                
                <!-- Indicadores -->
                <div class="carousel-indicators" *ngIf="imagensCarrossel.length > 1">
                  <button 
                    *ngFor="let imagem of imagensCarrossel; let i = index"
                    [class.active]="i === slideAtual"
                    (click)="irParaSlide(i)"
                  ></button>
                </div>
              </div>
            </div>
            
            <!-- Loading enquanto busca imagens -->
            <div *ngIf="carregandoCarrossel" class="carousel-placeholder rounded-4 shadow-lg d-flex align-items-center justify-content-center">
              <div class="text-center">
                <div class="spinner-carrossel mx-auto mb-3"></div>
                <p class="text-muted">Carregando imagens...</p>
              </div>
            </div>
            
            <!-- Fallback se não houver imagens após carregar -->
            <div *ngIf="!carregandoCarrossel && imagensCarrossel.length === 0" class="carousel-placeholder rounded-4 shadow-lg d-flex align-items-center justify-content-center">
              <div class="text-center text-muted">
                <i class="bi bi-image fs-1 mb-3 d-block"></i>
                <p>Configure as imagens do carrossel<br>em Parâmetros do Sistema</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Categorias -->
    <section class="py-5 bg-light">
      <div class="container">
        <h2 class="text-center fw-bold mb-5">
          <i class="bi bi-grid-3x3-gap me-2"></i>Categorias
        </h2>
        <div class="row g-4">
          <div class="col-6 col-md-4 col-lg-3" *ngFor="let categoria of categorias">
            <a [routerLink]="['/categoria', categoria.id]" class="text-decoration-none">
              <div class="card categoria-card h-100 text-center">
                <div class="card-body">
                  <!-- Imagem da categoria se existir -->
                  <div class="categoria-imagem mb-3" *ngIf="categoria.imagemUrl">
                    <img [src]="categoria.imagemUrl" 
                         [alt]="categoria.nome"
                         class="img-fluid"
                         onerror="this.style.display='none'; this.nextElementSibling.style.display='block'">
                    <i [class]="'bi ' + getIconeCategoria(categoria.nome) + ' fs-1 text-primary'" 
                       style="display: none;"></i>
                  </div>
                  <!-- Ícone padrão se não houver imagem -->
                  <i *ngIf="!categoria.imagemUrl" 
                     [class]="'bi ' + getIconeCategoria(categoria.nome) + ' fs-1 text-primary mb-3'"></i>
                  
                  <h5 class="card-title">{{ categoria.nome }}</h5>
                  <p class="card-text text-muted small">
                    {{ categoria.quantidadeProdutos }} produtos
                  </p>
                </div>
              </div>
            </a>
          </div>
        </div>
      </div>
    </section>

    <!-- Produtos em Destaque -->
    <section class="py-5">
      <div class="container">
        <div class="d-flex justify-content-between align-items-center mb-5">
          <h2 class="fw-bold mb-0">
            <i class="bi bi-star-fill text-warning me-2"></i>Produtos em Destaque
          </h2>
          <a routerLink="/produtos" class="btn btn-outline-primary">
            Ver Todos <i class="bi bi-arrow-right ms-2"></i>
          </a>
        </div>
        
        <div *ngIf="carregando" class="text-center py-5">
          <div class="spinner-loja mx-auto"></div>
          <p class="mt-3 text-muted">Carregando produtos...</p>
        </div>
        
        <div *ngIf="!carregando && produtosDestaque.length === 0" class="text-center py-5">
          <i class="bi bi-inbox fs-1 text-muted"></i>
          <p class="mt-3 text-muted">Nenhum produto em destaque no momento.</p>
        </div>
        
        <div class="row g-4">
          <div class="col-6 col-md-4 col-lg-3" *ngFor="let produto of produtosDestaque">
            <app-produto-card [produto]="produto"></app-produto-card>
          </div>
        </div>
      </div>
    </section>

    <!-- Benefícios -->
    <section class="py-5 bg-beneficios">
      <div class="container">
        <div class="row g-4 text-center">
          <div class="col-md-4">
            <i class="bi bi-truck fs-1 mb-3 d-block"></i>
            <h5>Entrega Rápida</h5>
            <p class="mb-0">Receba seus produtos com total segurança</p>
          </div>
          <div class="col-md-4">
            <i class="bi bi-shield-check fs-1 mb-3 d-block"></i>
            <h5>Compra Segura</h5>
            <p class="mb-0">Ambiente 100% seguro</p>
          </div>
          <div class="col-md-4">
            <i class="bi bi-headset fs-1 mb-3 d-block"></i>
            <h5>Suporte 24/7</h5>
            <p class="mb-0">Atendimento sempre disponível</p>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .hero-section {
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
      padding: 4rem 0;
      min-height: 70vh;
      display: flex;
      align-items: center;
    }
    
    .min-vh-50 {
      min-height: 50vh;
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
    
    .btn-primario:hover {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(47,106,73,0.18);
      color: var(--cor-escura);
    }
    
    .categoria-card {
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      border: none;
      border-radius: 12px;
      cursor: pointer;
    }
    
    .categoria-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
    }
    
    .categoria-imagem {
      width: 100%;
      height: 120px;
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
      border-radius: 8px;
      background: white;
      border: 2px solid #e2e8f0;
      padding: 10px;
    }
    
    .categoria-imagem img {
      max-width: 100%;
      max-height: 100%;
      object-fit: contain;
      filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.05));
    }
    
    .spinner-loja {
      width: 50px;
      height: 50px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid var(--cor-primaria);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .bg-beneficios {
      background: linear-gradient(135deg, #f8fafc, #e2e8f0);
      position: relative;
      border-top: 3px solid var(--cor-primaria);
      border-bottom: 3px solid var(--cor-primaria);
    }
    
    .bg-beneficios i {
      color: var(--cor-primaria);
    }
    
    .bg-beneficios h5 {
      color: #1e293b;
      font-weight: 700;
    }
    
    .bg-beneficios p {
      color: #475569;
    }
    
    /* Carrossel */
    .carousel-container {
      position: relative;
      width: 100%;
      overflow: hidden;
      border-radius: 16px;
    }
    
    .carousel-wrapper {
      position: relative;
      padding-bottom: 66.67%; /* Aspect ratio 3:2 */
      overflow: hidden;
    }
    
    .carousel-slides {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      display: flex;
      transition: transform 0.5s ease-in-out;
    }
    
    .carousel-slide {
      min-width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .carousel-slide img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .carousel-control {
      position: absolute;
      top: 50%;
      transform: translateY(-50%);
      background: rgba(255, 255, 255, 0.9);
      border: none;
      border-radius: 50%;
      width: 45px;
      height: 45px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.3s ease;
      z-index: 10;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
    }
    
    .carousel-control:hover {
      background: white;
      transform: translateY(-50%) scale(1.1);
    }
    
    .carousel-control.prev {
      left: 15px;
    }
    
    .carousel-control.next {
      right: 15px;
    }
    
    .carousel-control i {
      font-size: 20px;
      color: var(--cor-primaria);
    }
    
    .carousel-indicators {
      position: absolute;
      bottom: 15px;
      left: 50%;
      transform: translateX(-50%);
      display: flex;
      gap: 8px;
      z-index: 10;
    }
    
    .carousel-indicators button {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      border: 2px solid white;
      background: rgba(255, 255, 255, 0.5);
      cursor: pointer;
      transition: all 0.3s ease;
      padding: 0;
    }
    
    .carousel-indicators button.active {
      background: white;
      transform: scale(1.2);
    }
    
    .carousel-placeholder {
      background: linear-gradient(135deg, #f8f9fa, #e9ecef);
      min-height: 400px;
    }
    
    .spinner-carrossel {
      width: 50px;
      height: 50px;
      border: 4px solid #e9ecef;
      border-top: 4px solid var(--cor-primaria);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class HomeComponent implements OnInit {
  produtosDestaque: Produto[] = [];
  categorias: Categoria[] = [];
  carregando = true;
  
  // Carrossel
  imagensCarrossel: string[] = [];
  slideAtual = 0;
  intervaloCarrossel: any;
  carregandoCarrossel = true;

  constructor(
    private produtoService: ProdutoService,
    private parametroService: ParametroSistemaService
  ) {}

  ngOnInit(): void {
    this.carregarProdutosDestaque();
    this.carregarCategorias();
    this.carregarImagensCarrossel();
  }

  carregarProdutosDestaque(): void {
    this.carregando = true;
    this.produtoService.obterDestaques().subscribe({
      next: (produtos) => {
        this.produtosDestaque = produtos.slice(0, 8);
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
      }
    });
  }

  carregarCategorias(): void {
    this.produtoService.obterCategorias().subscribe({
      next: (categorias) => {
        this.categorias = categorias.slice(0, 8);
      }
    });
  }
  
  carregarImagensCarrossel(): void {
    // Buscar as 4 URLs das imagens
    forkJoin([
      this.parametroService.obterPorChave('CarrosselImagem1'),
      this.parametroService.obterPorChave('CarrosselImagem2'),
      this.parametroService.obterPorChave('CarrosselImagem3'),
      this.parametroService.obterPorChave('CarrosselImagem4')
    ]).subscribe({
      next: (parametros) => {
        this.imagensCarrossel = parametros
          .map(p => p.valor)
          .filter(url => url && url.trim() !== '');
        
        if (this.imagensCarrossel.length > 1) {
          this.iniciarCarrossel();
        }
        
        this.carregandoCarrossel = false;
      },
      error: () => {
        console.warn('Não foi possível carregar imagens do carrossel');
        this.imagensCarrossel = [];
        this.carregandoCarrossel = false;
      }
    });
  }
  
  iniciarCarrossel(): void {
    this.intervaloCarrossel = setInterval(() => {
      this.proximoSlide();
    }, 5000); // Muda a cada 5 segundos
  }
  
  proximoSlide(): void {
    this.slideAtual = (this.slideAtual + 1) % this.imagensCarrossel.length;
  }
  
  anteriorSlide(): void {
    this.slideAtual = this.slideAtual === 0 
      ? this.imagensCarrossel.length - 1 
      : this.slideAtual - 1;
  }
  
  irParaSlide(index: number): void {
    this.slideAtual = index;
    // Resetar o timer quando usu\u00e1rio navega manualmente
    if (this.intervaloCarrossel) {
      clearInterval(this.intervaloCarrossel);
      this.iniciarCarrossel();
    }
  }
    getIconeCategoria(nomeCategoria: string): string {
    const nome = nomeCategoria.toLowerCase();
    
    // Kimono / Gi
    if (nome.includes('kimono') || nome.includes('gi')) {
      return 'bi-person-arms-up';
    }
    
    // Blusa / Camisa / Rashguard
    if (nome.includes('blusa') || nome.includes('camisa') || nome.includes('rashguard') || nome.includes('rash')) {
      return 'bi-shirt';
    }
    
    // Short / Bermuda / Calça
    if (nome.includes('short') || nome.includes('bermuda') || nome.includes('calça') || nome.includes('calca')) {
      return 'bi-shorts';
    }
    
    // Faixa / Belt
    if (nome.includes('faixa') || nome.includes('belt')) {
      return 'bi-award';
    }
    
    // Acessórios
    if (nome.includes('acessório') || nome.includes('acessorio')) {
      return 'bi-bag';
    }
    
    // Padrão
    return 'bi-tag';
  }
    ngOnDestroy(): void {
    if (this.intervaloCarrossel) {
      clearInterval(this.intervaloCarrossel);
    }
  }
}
