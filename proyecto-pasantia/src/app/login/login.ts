import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  username: string = '';
  password: string = '';
  error: string = '';
  cargando: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  iniciarSesion(): void {
    if (!this.username || !this.password) {
      this.error = 'Usuario y contraseña son obligatorios.';
      return;
    }

    this.cargando = true;
    this.error = '';

    this.authService.login(this.username, this.password).subscribe({
      next: (data) => {
        this.authService.guardarSesion(data);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.error = 'Usuario o contraseña incorrectos.';
        this.cargando = false;
      }
    });
  }
  
}
