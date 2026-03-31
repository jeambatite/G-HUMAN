import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ReporteService {
  private apiUrl = 'http://localhost:5256/api';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getPorDepartamento(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes/por-departamento`, { headers: this.headers() });
  }

  getPorRol(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes/por-rol`, { headers: this.headers() });
  }

  getPorGenero(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes/por-genero`, { headers: this.headers() });
  }

  getPorEstado(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes/por-estado`, { headers: this.headers() });
  }

  getSueldoPorDepartamento(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes/sueldo-por-departamento`, { headers: this.headers() });
  }

  getIngresosPorAnio(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes/ingresos-por-anio`, { headers: this.headers() });
  }
}