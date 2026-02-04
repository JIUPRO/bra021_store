import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CarrinhoService } from '../../services/carrinho.service';
import { ClienteService } from '../../services/cliente.service';
import { Cliente } from '../../models/cliente.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark navbar-loja fixed-top">
      <div class="container">
        <a class="navbar-brand d-flex align-items-center fw-bold" routerLink="/">
          <img src="assets/images/logo.jpg" alt="Loja Brazil-021" style="height:68px; width:68px; object-fit:cover; border-radius:8px; margin-right:12px;" />
          <span class="brand-text">Loja Brazil-021 Jiu-Jitsu</span>
        </a>
        
        <button class="navbar-toggler" type="button" (click)="menuAberto = !menuAberto">
          <span class="navbar-toggler-icon"></span>
        </button>
        
        <div class="collapse navbar-collapse" [class.show]="menuAberto">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0">
            <li class="nav-item">
              <a class="nav-link" routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">
                <i class="bi bi-house me-1"></i>Início
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" routerLink="/produtos" routerLinkActive="active">
                <i class="bi bi-grid me-1"></i>Produtos
              </a>
            </li>
          </ul>
          
          <ul class="navbar-nav ms-auto mb-2 mb-lg-0 align-items-center">
            <!-- Carrinho -->
            <li class="nav-item me-3">
              <a class="nav-link position-relative" routerLink="/carrinho">
                <i class="bi bi-cart3 fs-5"></i>
                <span *ngIf="quantidadeCarrinho > 0" class="badge-carrinho">
                  {{ quantidadeCarrinho }}
                </span>
              </a>
            </li>
            
            <!-- Usuário não logado -->
            <ng-container *ngIf="!clienteLogado">
              <li class="nav-item">
                <a class="nav-link" routerLink="/login">
                  <i class="bi bi-person me-1"></i>Entrar
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link" routerLink="/cadastro">
                  <i class="bi bi-person-plus me-1"></i>Cadastrar
                </a>
              </li>
            </ng-container>
            
            <!-- Usuário logado -->
            <ng-container *ngIf="clienteLogado">
              <li class="nav-item dropdown">
                <a class="nav-link dropdown-toggle" href="#" (click)="dropdownAberto = !dropdownAberto; $event.preventDefault()">
                  <i class="bi bi-person-circle me-1"></i>
                  {{ clienteLogado.nome.split(' ')[0] }}
                </a>
                <ul class="dropdown-menu dropdown-menu-end" [class.show]="dropdownAberto">
                  <li>
                    <a class="dropdown-item" routerLink="/meus-pedidos" (click)="dropdownAberto = false">
                      <i class="bi bi-bag me-2"></i>Meus Pedidos
                    </a>
                  </li>
                  <li><hr class="dropdown-divider"></li>
                  <li>
                    <a class="dropdown-item text-danger" href="#" (click)="logout(); $event.preventDefault()">
                      <i class="bi bi-box-arrow-right me-2"></i>Sair
                    </a>
                  </li>
                </ul>
              </li>
            </ng-container>
          </ul>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar-loja {
      background: linear-gradient(135deg, var(--cor-primaria), var(--cor-primaria-claro));
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }
    
    .nav-link {
      color: white !important;
      font-weight: 500;
      transition: opacity 0.3s ease;
    }
    
    .nav-link:hover {
      opacity: 0.8;
    }
    
    .badge-carrinho {
      position: absolute;
      top: -5px;
      right: -5px;
      background-color: var(--cor-perigo);
      color: white;
      border-radius: 50%;
      width: 20px;
      height: 20px;
      font-size: 0.7rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .brand-text {
      color: var(--cor-clara);
      font-size: 1rem;
      margin-left: 4px;
    }
    
    .dropdown-menu {
      border: none;
      box-shadow: 0 5px 20px rgba(0, 0, 0, 0.15);
      border-radius: 10px;
    }
  `]
})
export class NavbarComponent implements OnInit {
  quantidadeCarrinho = 0;
  clienteLogado: Cliente | null = null;
  menuAberto = false;
  dropdownAberto = false;

  constructor(
    private carrinhoService: CarrinhoService,
    private clienteService: ClienteService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carrinhoService.quantidade$.subscribe(quantidade => {
      this.quantidadeCarrinho = quantidade;
    });

    this.clienteService.clienteLogado$.subscribe(cliente => {
      this.clienteLogado = cliente;
    });
  }

  logout(): void {
    this.clienteService.logout();
    this.dropdownAberto = false;
    this.router.navigate(['/']);
  }
}
