import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AlertService } from '../../services/alert.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-esqueceu-senha',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './esqueceu-senha.component.html',
  styleUrls: ['./esqueceu-senha.component.css']
})
export class EsqueceuSenhaComponent implements OnInit {
  etapa: number = 1; // 1 = email, 2 = código + nova senha
  email: string = '';
  codigo: string = '';
  novaSenha: string = '';
  confirmaSenha: string = '';
  
  carregando: boolean = false;
  tentouEnviar: boolean = false;

  constructor(
    private http: HttpClient,
    private alertService: AlertService,
    private router: Router
  ) {}

  ngOnInit(): void {}

  // Step 1: Get recovery code
  enviarEmail(): void {
    this.tentouEnviar = true;

    if (!this.email) {
      this.alertService.error('Por favor, insira seu email');
      return;
    }

    this.carregando = true;

    const payload = { email: this.email };
    const url = `${environment.apiUrl}/auth/clientes/esqueceu-senha`;

    this.http.post(url, payload).subscribe({
      next: (response: any) => {
        this.carregando = false;
        this.alertService.success('Foi enviado pro seu e-mail o código pra troca de senha.');
        
        // Move to step 2 after short delay
        setTimeout(() => {
          this.etapa = 2;
          this.tentouEnviar = false;
        }, 1500);
      },
      error: (error: any) => {
        this.carregando = false;
        this.alertService.error('Erro ao processar solicitação. Tente novamente.');
        console.error('Erro:', error);
      }
    });
  }

  // Step 2: Reset password
  resetarSenha(): void {
    this.tentouEnviar = true;

    if (!this.codigo || !this.novaSenha || !this.confirmaSenha) {
      this.alertService.error('Por favor, preencha todos os campos');
      return;
    }

    if (this.novaSenha.length < 6) {
      this.alertService.error('A senha deve ter no mínimo 6 caracteres');
      return;
    }

    if (this.novaSenha !== this.confirmaSenha) {
      this.alertService.error('As senhas não correspondem');
      return;
    }

    this.carregando = true;

    const payload = {
      email: this.email,
      codigo: this.codigo,
      novaSenha: this.novaSenha,
      confirmaSenha: this.confirmaSenha
    };

    const url = `${environment.apiUrl}/auth/clientes/resetar-senha`;

    this.http.post(url, payload).subscribe({
      next: (response: any) => {
        this.carregando = false;
        this.alertService.success('Senha redefinida com sucesso! Faça login com sua nova senha.');
        
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);
      },
      error: (error: any) => {
        this.carregando = false;
        if (error.error && error.error.mensagem) {
          this.alertService.error(error.error.mensagem);
        } else {
          this.alertService.error('Erro ao redefinir senha. Verifique o código e tente novamente.');
        }
        console.error('Erro:', error);
      }
    });
  }

  // Format inputs
  aoDigitarCodigo(): void {
    // Keep only alphanumeric
    this.codigo = this.codigo.replace(/[^a-zA-Z0-9]/g, '').toUpperCase().slice(0, 6);
  }

  voltarParaEmail(): void {
    this.etapa = 1;
    this.codigo = '';
    this.novaSenha = '';
    this.confirmaSenha = '';
    this.tentouEnviar = false;
  }

  irParaLogin(): void {
    this.router.navigate(['/login']);
  }
}
