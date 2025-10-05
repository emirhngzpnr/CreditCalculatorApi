export interface LogsQuery {
  level?: 'Info' | 'Warning' | 'Error' | '';
  search?: string;
  fromUtc?: string;    // ISO
  toUtc?: string;      // ISO
  page?: number;
  pageSize?: number;
}
