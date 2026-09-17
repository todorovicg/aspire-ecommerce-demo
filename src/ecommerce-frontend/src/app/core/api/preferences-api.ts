import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';

import { Preferences } from '../../shared/models/preferences';
import { ApiResponse, toApiFailure, unwrap } from './api-response';

@Injectable({ providedIn: 'root' })
export class PreferencesApi {
  private readonly http = inject(HttpClient);

  get(): Promise<Preferences> {
    return this.request(this.http.get<ApiResponse<Preferences>>('/api/preferences'));
  }

  update(language: string): Promise<Preferences> {
    return this.request(this.http.put<ApiResponse<Preferences>>('/api/preferences', { language }));
  }

  private async request<T>(call: Observable<ApiResponse<T>>): Promise<T> {
    try {
      return unwrap(await firstValueFrom(call));
    } catch (error) {
      throw toApiFailure(error);
    }
  }
}
