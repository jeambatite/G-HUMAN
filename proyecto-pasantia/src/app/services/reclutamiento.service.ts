import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReclutamientoService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getPostulantes(estado?: string, puesto?: string, busquedaCv?: string): Observable<any[]> {
    let params = '';
    if (estado) params += `&estado=${estado}`;
    if (puesto) params += `&puesto=${puesto}`;
    if (busquedaCv) params += `&busquedaCv=${busquedaCv}`;
    return this.http.get<any[]>(`${this.apiUrl}/reclutamiento/postulantes?${params}`, { headers: this.headers() });
  }

  actualizarEstado(id: number, estado: string, notas?: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/reclutamiento/postulantes/${id}/estado`, { estado, notas }, { headers: this.headers() });
  }

  eliminarPostulante(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/reclutamiento/postulantes/${id}`, { headers: this.headers() });
  }

  getFiltros(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reclutamiento/filtros`, { headers: this.headers() });
  }

  crearFiltro(nombre: string, palabrasClave: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reclutamiento/filtros`, { nombre, palabrasClave }, { headers: this.headers() });
  }

  eliminarFiltro(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/reclutamiento/filtros/${id}`, { headers: this.headers() });
  }

  toggleFiltro(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/reclutamiento/filtros/${id}/toggle`, {}, { headers: this.headers() });
  }

  aplicarFiltros(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reclutamiento/aplicar-filtros`, { headers: this.headers() });
  }
}   