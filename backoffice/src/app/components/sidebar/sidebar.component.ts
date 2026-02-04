import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="logo-container">
          <img src="assets/images/logo.jpg" alt="Loja Brazil-021" class="logo-image" />
        </div>
        <div class="logo-text">
          <div class="logo-line">Loja</div>
          <div class="logo-line logo-highlight">Brazil-021</div>
          <div class="logo-line logo-subtitle">School of Jiu-Jitsu</div>
        </div>
      </div>
      
      <ul class="sidebar-menu">
        <li>
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">
            <i class="bi bi-speedometer2"></i>
            <span>Dashboard</span>
          </a>
        </li>
        <li>
          <a routerLink="/produtos" routerLinkActive="active">
            <i class="bi bi-box-seam"></i>
            <span>Produtos</span>
          </a>
        </li>
        <li>
          <a routerLink="/categorias" routerLinkActive="active">
            <i class="bi bi-tags"></i>
            <span>Categorias</span>
          </a>
        </li>
        <li>
          <a routerLink="/pedidos" routerLinkActive="active">
            <i class="bi bi-bag"></i>
            <span>Pedidos</span>
          </a>
        </li>
        <li>
          <a routerLink="/clientes" routerLinkActive="active">
            <i class="bi bi-people"></i>
            <span>Clientes</span>
          </a>
        </li>
        <li>
          <a routerLink="/estoque" routerLinkActive="active">
            <i class="bi bi-boxes"></i>
            <span>Estoque</span>
          </a>
        </li>
        <li>
          <a routerLink="/escolas" routerLinkActive="active">
            <i class="bi bi-building"></i>
            <span>Escolas</span>
          </a>
        </li>
        <li>
          <a routerLink="/parametros" routerLinkActive="active">
            <i class="bi bi-gear-fill"></i>
            <span>Parâmetros</span>
          </a>
        </li>
        <li>
          <a routerLink="/usuarios" routerLinkActive="active">
            <i class="bi bi-person-badge"></i>
            <span>Usuários</span>
          </a>
        </li>
        <li>
          <a routerLink="/relatorios" routerLinkActive="active">
            <i class="bi bi-graph-up"></i>
            <span>Relatórios</span>
          </a>
        </li>
      </ul>
      
      <div class="sidebar-footer">
        <a href="http://localhost:4200" target="_blank">
          <i class="bi bi-box-arrow-up-right"></i>
          <span>Ver Loja</span>
        </a>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      position: fixed;
      top: 0;
      left: 0;
      width: 260px;
      height: 100vh;
      background: var(--cor-menu-bg);
      color: white;
      z-index: 1000;
      display: flex;
      flex-direction: column;
    }
    
    .sidebar-header {
      padding: 24px 20px;
      border-bottom: 1px solid rgba(255, 255, 255, 0.2);
      text-align: center;
    }
    
    .logo-container {
      margin-bottom: 16px;
    }
    
    .logo-image {
      height: 80px;
      width: 80px;
      object-fit: cover;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
    }
    
    .logo-text {
      margin: 12px 0 8px 0;
      font-weight: 600;
      color: white;
    }
    
    .logo-line {
      line-height: 1.4;
      font-size: 1rem;
      color: white;
    }
    
    .logo-highlight {
      font-size: 1.3rem;
      font-weight: 800;
      color: #fbbf24;
      letter-spacing: 0.5px;
    }
    
    .logo-subtitle {
      font-size: 0.75rem;
      opacity: 0.95;
      margin-top: 4px;
    }
    
    .sidebar-menu {
      list-style: none;
      padding: 0;
      margin: 20px 0;
      flex: 1;
    }
    
    .sidebar-menu li {
      margin: 2px 0;
    }
    
    .sidebar-menu a {
      display: flex;
      align-items: center;
      padding: 14px 20px;
      color: rgba(255, 255, 255, 0.85);
      text-decoration: none;
      transition: all 0.3s ease;
      border-left: 3px solid transparent;
    }
    
    .sidebar-menu a:hover,
    .sidebar-menu a.active {
      background: rgba(255, 255, 255, 0.15);
      color: white;
      border-left-color: var(--cor-primaria-claro);
    }
    
    .sidebar-menu i {
      font-size: 1.1rem;
      margin-right: 12px;
      width: 24px;
      text-align: center;
    }
    
    .sidebar-footer {
      padding: 20px;
      border-top: 1px solid rgba(255, 255, 255, 0.2);
    }
    
    .sidebar-footer a {
      display: flex;
      align-items: center;
      color: rgba(255, 255, 255, 0.85);
      text-decoration: none;
      transition: all 0.3s ease;
    }
    
    .sidebar-footer a:hover {
      color: white;
    }
    
    .sidebar-footer i {
      margin-right: 12px;
    }
    
    @media (max-width: 992px) {
      .sidebar {
        transform: translateX(-100%);
      }
    }
  `]
})
export class SidebarComponent {}
