import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5256/api';

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, { username, password });
  }
  refreshSesion(): void {
    sessionStorage.setItem('loginTime', Date.now().toString());
  }

  guardarSesion(data: any): void {
    sessionStorage.setItem('token', data.token);
    sessionStorage.setItem('rol', data.rol);
    sessionStorage.setItem('nombre', data.nombreEmpleado);
    sessionStorage.setItem('idEmpleado', data.idEmpleado.toString());
    sessionStorage.setItem('username', data.username);
    sessionStorage.setItem('permisos', JSON.stringify(data.permisos));
    sessionStorage.setItem('loginTime', Date.now().toString());
  }

  getPermisos(): string[] {
    const p = sessionStorage.getItem('permisos');
    return p ? JSON.parse(p) : [];
  }

  tienePermiso(permiso: string): boolean {
    return this.getPermisos().includes(permiso);
  }
  verificarPin(pin: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/auth/verificar-pin/${this.getIdEmpleado()}`,
      { pin },
      { headers: new HttpHeaders({ 'Authorization': `Bearer ${this.getToken()}` }) }
    );
  }

  sesionExpirada(): boolean {
    const loginTime = sessionStorage.getItem('loginTime');
    if (!loginTime) return true;
    const minutosTranscurridos = (Date.now() - parseInt(loginTime)) / 1000 / 60;
    return minutosTranscurridos > 10; // minutos
  }

  cerrarSesion(): void {
    sessionStorage.clear();
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  getRol(): string | null {
    return sessionStorage.getItem('rol');
  }

  getNombre(): string | null {
    return sessionStorage.getItem('nombre');
  }

  getIdEmpleado(): number {
    return Number(sessionStorage.getItem('idEmpleado'));
  }

  estaAutenticado(): boolean {
    return !!this.getToken();
  }
}