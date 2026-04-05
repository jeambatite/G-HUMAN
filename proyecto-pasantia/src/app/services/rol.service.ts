import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RolService {
    private apiUrl = environment.apiUrl;


  constructor(private http: HttpClient, private authService: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/roles`, { headers: this.headers() });
  }

  crear(rol: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/roles`, rol, { headers: this.headers() });
  }

  editar(id: number, rol: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/roles/${id}`, rol, { headers: this.headers() });
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/roles/${id}`, { headers: this.headers() });
  }
}