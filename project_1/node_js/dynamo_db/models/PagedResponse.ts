export interface PagedResponse<T> {
  pageSize?: number;
  // TODO: create pageNumber in future ticket about pagination
  // pageNumber: number;
  total: number;
  data: T[];
  lastKey: unknown;
}
