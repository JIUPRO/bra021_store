import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

type SweetAlertResult = {
  isConfirmed: boolean;
  isDenied?: boolean;
  isDismissed?: boolean;
};

@Injectable({
  providedIn: 'root'
})
export class AlertService {

  constructor() { }

  success(title: string = 'Sucesso!', message: string = ''): Promise<SweetAlertResult> {
    return Swal.fire({
      icon: 'success',
      title: title,
      text: message,
      confirmButtonColor: '#2F6A49',
      confirmButtonText: 'OK'
    });
  }

  error(title: string = 'Erro!', message: string = ''): Promise<SweetAlertResult> {
    return Swal.fire({
      icon: 'error',
      title: title,
      text: message,
      confirmButtonColor: '#2F6A49',
      confirmButtonText: 'OK'
    });
  }

  warning(title: string = 'Atenção!', message: string = ''): Promise<SweetAlertResult> {
    return Swal.fire({
      icon: 'warning',
      title: title,
      text: message,
      confirmButtonColor: '#2F6A49',
      confirmButtonText: 'OK'
    });
  }

  info(title: string = 'Informação', message: string = ''): Promise<SweetAlertResult> {
    return Swal.fire({
      icon: 'info',
      title: title,
      text: message,
      confirmButtonColor: '#2F6A49',
      confirmButtonText: 'OK'
    });
  }

  question(title: string = 'Confirmação', message: string = ''): Promise<SweetAlertResult> {
    return Swal.fire({
      icon: 'question',
      title: title,
      text: message,
      showCancelButton: true,
      confirmButtonColor: '#2F6A49',
      cancelButtonColor: '#868F89',
      confirmButtonText: 'Sim',
      cancelButtonText: 'Cancelar'
    });
  }

  confirm(title: string = 'Confirmação', message: string = ''): Promise<boolean> {
    return Swal.fire({
      icon: 'question',
      title: title,
      text: message,
      showCancelButton: true,
      confirmButtonColor: '#2F6A49',
      cancelButtonColor: '#868F89',
      confirmButtonText: 'Confirmar',
      cancelButtonText: 'Cancelar'
    }).then((result: SweetAlertResult) => result.isConfirmed);
  }

  loading(message: string = 'Processando...'): void {
    Swal.fire({
      title: message,
      allowOutsideClick: false,
      allowEscapeKey: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });
  }

  close(): void {
    Swal.close();
  }
}
