import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <footer class="footer-loja">
      <div class="container">
        <div class="row align-items-center">
          <div class="col-md-6 text-center text-md-start mb-2 mb-md-0">
            <div style="font-weight:600; color:var(--cor-clara)">Contato: contato&#64;rlm.dev.br</div>
          </div>
          <div class="col-md-6 text-center text-md-end">
            <small style="color:var(--cor-clara-2); font-weight:600">
              Desenvolvido por <a href="https://www.rlm.dev.br/" target="_blank" style="color:var(--cor-accent); text-decoration:none; font-weight:700;">RLM</a> — Produto Registrado
            </small>
          </div>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer-loja {
      background: linear-gradient(180deg, var(--cor-primaria), var(--cor-primaria-claro));
      color: var(--cor-clara);
      padding: 1rem 0;
      margin-top: auto;
    }
    
    .footer-loja a {
      color: var(--cor-clara-2);
    }
  `]
})
export class FooterComponent {
  anoAtual = new Date().getFullYear();
}
