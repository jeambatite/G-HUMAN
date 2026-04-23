import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmpleadoService {
  private apiUrl = environment.apiUrl;


  constructor(private http: HttpClient, private authService: AuthService) { }

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/empleados`, { headers: this.headers() });
  }

  crear(empleado: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/empleados`, empleado, { headers: this.headers() });
  }

  editar(id: number, empleado: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/empleados/${id}`, empleado, { headers: this.headers() });
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/empleados/${id}`, { headers: this.headers() });
  }

  getDatosSensibles(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/empleados/${id}/datos-sensibles`, { headers: this.headers() });
  }
  agregarAusencia(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/empleados/${id}/ausencias`, {}, { headers: this.headers() });
  }
  quitarAusencia(id: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/empleados/${id}/ausencias/quitar`, {}, { headers: this.headers() });
}
  getPaginado(pagina: number, tamanoPagina: number, filtros?: any): Observable<any> {
    let params = `pagina=${pagina}&tamanoPagina=${tamanoPagina}`;
    if (filtros?.nombre) params += `&nombre=${filtros.nombre}`;
    if (filtros?.rol) params += `&rol=${filtros.rol}`;
    if (filtros?.estado) params += `&estado=${filtros.estado}`;
    if (filtros?.departamento) params += `&departamento=${filtros.departamento}`;
    return this.http.get(
      `${this.apiUrl}/empleados/paginado?${params}`,
      { headers: this.headers() }
    );
  }
}