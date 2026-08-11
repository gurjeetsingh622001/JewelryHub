import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Mirrors JewelryHub.Application.Features.Uploads.Common.UploadedFileDto.
export interface UploadedFile {
  url: string;
  fileName: string;
  sizeBytes: number;
}

/**
 * Thin wrapper around the backend's generic UploadsController — Seller/Admin
 * only (matches the controller's [Authorize(Roles = "Seller,Admin")]).
 * Callers store the returned url in whatever field the existing create/
 * submit command already expects (Product's imageUrl, KYC's fileUrl) —
 * uploading is a separate pre-step, not baked into those commands.
 */
@Injectable({ providedIn: 'root' })
export class UploadsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/uploads`;

  uploadProductImage(file: File): Observable<UploadedFile> {
    return this.upload(`${this.baseUrl}/product-images`, file);
  }

  uploadKycDocument(file: File): Observable<UploadedFile> {
    return this.upload(`${this.baseUrl}/kyc-documents`, file);
  }

  private upload(url: string, file: File): Observable<UploadedFile> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadedFile>(url, formData);
  }
}
