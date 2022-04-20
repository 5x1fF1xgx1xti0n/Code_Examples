import { FilterItem } from './FilterItem';

export interface PageRequest {
  pageSize?: number;
  // TODO: create pageNumber in future ticket about pagination
  // pageNumber: number;
  lastKey?: unknown;
  filters: FilterItem[];
  descending?: boolean;
}
