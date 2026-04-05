import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
    private apiUrl = environment.apiUrl;


  constructor(private http: HttpClient, private authService: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  tieneUsuario(idEmpleado: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/usuarios/tiene-usuario/${idEmpleado}`, { headers: this.headers() });
  }

  crear(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/usuarios`, data, { headers: this.headers() });
  }
}