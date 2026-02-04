import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { HeaderComponent } from './components/header/header.component';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, HeaderComponent],
  template: `
    <div class="app-container" [class.login-view]="!estaAutenticado()">
      <app-sidebar *ngIf="estaAutenticado()"></app-sidebar>
      <div class="main-content" [class.full-width]="!estaAutenticado()">
        <app-header *ngIf="estaAutenticado()"></app-header>
        <div class="content-area">
          <router-outlet></router-outlet>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .app-container {
      display: flex;
      min-height: 100vh;
      width: 100%;
    }
    
    .app-container.login-view {
      justify-content: center;
      align-items: center;
    }
    
    .main-content {
      flex: 1;
      margin-left: 260px;
      min-height: 100vh;
      background: #f5f6fa;
    }
    
    .main-content.full-width {
      margin-left: 0;
      width: 100%;
      height: 100vh;
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 0;
    }
    
    .content-area {
      padding: 24px;
      flex: 1;
    }
    
    .main-content.full-width .content-area {
      padding: 0;
      width: 100%;
      height: 100%;
    }
    
    @media (max-width: 992px) {
      .main-content {
        margin-left: 0;
      }
    }
  `]
})
export class AppComponent {
  private authService = inject(AuthService);
  title = 'Loja Virtual - Backoffice';

  estaAutenticado(): boolean {
    return this.authService.estaAutenticado();
  }
}
