// Mirrors JewelryHub.Application.Common.Models.PagedResult<T> — the standard
// shape for every paged list endpoint in the backend.
export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
