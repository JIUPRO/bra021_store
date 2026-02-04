import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-pedido-sucesso',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-8 text-center">
          <div class="success-icon mb-4">
            <i class="bi bi-check-circle-fill"></i>
          </div>
          
          <h1 class="fw-bold mb-3">Pedido Realizado com Sucesso!</h1>
          <p class="lead text-muted mb-4">
            Obrigado pela sua compra. Seu pedido foi recebido e está sendo processado.
          </p>
          
          <div class="card mb-4">
            <div class="card-body">
              <h5 class="card-title">Número do Pedido</h5>
              <h2 class="text-primary fw-bold">{{ numeroPedido }}</h2>
              <p class="card-text text-muted">
                Guarde este número para acompanhar seu pedido.
              </p>
            </div>
          </div>
          
          <div class="alert alert-info mb-4">
            <i class="bi bi-info-circle me-2"></i>
            <strong>Importante:</strong> Você receberá um email e uma mensagem no WhatsApp 
            com os detalhes do seu pedido em breve.
          </div>
          
          <div class="d-flex justify-content-center gap-3">
            <a routerLink="/produtos" class="btn btn-primary btn-lg">
              <i class="bi bi-grid me-2"></i>Continuar Comprando
            </a>
            <a routerLink="/meus-pedidos" class="btn btn-outline-primary btn-lg">
              <i class="bi bi-bag me-2"></i>Meus Pedidos
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .success-icon {
      font-size: 6rem;
      color: #198754;
      animation: scaleIn 0.5s ease-out;
    }
    
    @keyframes scaleIn {
      0% {
        transform: scale(0);
        opacity: 0;
      }
      50% {
        transform: scale(1.2);
      }
      100% {
        transform: scale(1);
        opacity: 1;
      }
    }
  `]
})
export class PedidoSucessoComponent implements OnInit {
  numeroPedido = '';

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.numeroPedido = params['numero'] || 'N/A';
    });
  }
}
