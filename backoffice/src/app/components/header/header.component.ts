import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService, Usuario } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="top-header">
      <div class="header-title d-flex align-items-center">
        <img src="assets/images/logo.jpg" alt="Loja Brazil-021" class="header-logo" />
        <div>
          <div style="font-weight:700; color:var(--cor-escura)">Loja Brazil-021</div>
          <small style="color:var(--cor-accent); font-weight:700; font-size:0.75rem">SCHOOL OF JIU-JITSU</small>
        </div>
      </div>
      <div class="header-actions">
        <div class="dropdown">
          <button class="btn d-flex align-items-center" (click)="menuAberto = !menuAberto">
            <i class="bi bi-person-circle fs-5 me-2"></i>
            <span>{{ usuarioAtual?.nome || 'Administrador' }}</span>
            <i class="bi bi-chevron-down ms-2"></i>
          </button>
          <div class="dropdown-menu" [class.show]="menuAberto">
            <a class="dropdown-item" [routerLink]="['/usuarios']">
              <i class="bi bi-people me-2"></i>Gerenciar Usuários
            </a>
            <div class="dropdown-divider"></div>
            <a class="dropdown-item text-danger" href="javascript:void(0)" (click)="logout()">
              <i class="bi bi-box-arrow-right me-2"></i>Sair
            </a>
          </div>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .top-header {
      height: var(--header-height);
      background: white;
      border-bottom: 2px solid var(--cor-primaria);
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 24px;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    
    .header-title {
      font-size: 1.1rem;
      font-weight: 600;
      color: #10b981;
    }

    .header-logo {
      height: 56px;
      width: 56px;
      object-fit: cover;
      border-radius: 8px;
      margin-right: 12px;
    }
    
    .header-actions {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .header-actions .btn {
      border: none;
      background: transparent;
      color: #6c757d;
      padding: 8px 12px;
      border-radius: 8px;
      transition: all 0.3s ease;
      position: relative;
    }
    
    .header-actions .btn:hover {
      background: #f0f0f0;
      color: #10b981;
    }
    
    .header-actions .badge {
      position: absolute;
      top: 2px;
      right: 2px;
      font-size: 0.6rem;
      padding: 2px 5px;
      background-color: #ef4444;
    }
    
    .dropdown {
      position: relative;
    }
    
    .dropdown-menu {
      position: absolute;
      top: 100%;
      right: 0;
      min-width: 180px;
      background: white;
      border: none;
      box-shadow: 0 5px 20px rgba(0, 0, 0, 0.15);
      border-radius: 8px;
      padding: 8px 0;
      display: none;
    }
    
    .dropdown-menu.show {
      display: block;
    }
    
    .dropdown-item {
      display: block;
      padding: 8px 16px;
      color: #212529;
      text-decoration: none;
      transition: all 0.3s ease;
    }
    
    .dropdown-item:hover {
      background: #f0fdf4;
      color: #10b981;
    }
    
    .dropdown-item.text-danger:hover {
      background: #fee2e2;
      color: #ef4444;
    }
    
    .dropdown-divider {
      height: 1px;
      margin: 8px 0;
      background: #e0e0e0;
    }
  `]
})
export class HeaderComponent implements OnInit {
  menuAberto = false;
  usuarioAtual: Usuario | null = null;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.usuarioAtual = this.authService.obterUsuarioAtual();
    this.authService.usuario$.subscribe(usuario => {
      this.usuarioAtual = usuario;
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
