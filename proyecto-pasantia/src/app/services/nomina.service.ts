import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NominaService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getEmpleados(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/nomina/empleados`, { headers: this.headers() });
  }

  getConfig(): Observable<any> {
    return this.http.get(`${this.apiUrl}/nomina/config`, { headers: this.headers() });
  }

  actualizarBono(empleadoId: number, bono: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/nomina/bono`, { empleadoId, bono }, { headers: this.headers() });
  }

  bonoGlobal(bono: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/nomina/bono-global`, { bono }, { headers: this.headers() });
  }

  getHistorial(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/nomina/historial`, { headers: this.headers() });
  }

  testRun(secretKey: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/nomina/test-run`, { secretKey }, { headers: this.headers() });
  }
}