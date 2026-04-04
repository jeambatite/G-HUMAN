import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // 1. ELIMINAMOS la URL fija y usamos siempre environment.apiUrl
  private apiUrl = environment.apiUrl; 

  constructor(private http: HttpClient) { }

  // 2. Login centralizado usando la URL del environment
  login(username: string, password: string): Observable<any> {
    // Asegúrate de que en .NET el endpoint sea /api/auth/login o solo /auth/login
    return this.http.post(`${this.apiUrl}/api/auth/login`, { username, password });
  }

  // 3. Corregimos verificarPin para que use la URL correcta y absoluta
  verificarPin(pin: string): Observable<any> {
    const idEmpleado = this.getIdEmpleado();
    return this.http.post(
      `${this.apiUrl}/api/auth/verificar-pin/${idEmpleado}`, // Agregamos /api/ si tu backend lo requiere
      { pin },
      { headers: new HttpHeaders({ 'Authorization': `Bearer ${this.getToken()}` }) }
    );
  }

  // --- El resto de tus métodos de SessionStorage están perfectos ---

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

  sesionExpirada(): boolean {
    const loginTime = sessionStorage.getItem('loginTime');
    if (!loginTime) return true;
    const minutosTranscurridos = (Date.now() - parseInt(loginTime)) / 1000 / 60;
    return minutosTranscurridos > 10; 
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