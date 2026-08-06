import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateAddressRequest, CustomerAddress } from './models';

@Injectable({ providedIn: 'root' })
export class AddressesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/customer-addresses`;

  getMyAddresses(): Observable<CustomerAddress[]> {
    return this.http.get<CustomerAddress[]>(this.baseUrl);
  }

  create(request: CreateAddressRequest): Observable<CustomerAddress> {
    return this.http.post<CustomerAddress>(this.baseUrl, request);
  }
}
