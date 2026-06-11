import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SearchReport } from '../models/report';

/** Thin wrapper over the .NET Web API endpoints. */
@Injectable({ providedIn: 'root' })
export class SolicitorApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  /** The default locations the UI pre-populates (editable by the user). */
  getDefaultLocations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/locations`);
  }

  /** Runs a search across the given locations and returns the insight report. */
  search(locations: string[]): Observable<SearchReport> {
    return this.http.post<SearchReport>(`${this.baseUrl}/search`, { locations });
  }
}
