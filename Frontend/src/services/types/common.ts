export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PagedRequest {
  page?: number;
  pageSize?: number;
}
