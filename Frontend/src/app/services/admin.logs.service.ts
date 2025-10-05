import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LogsQuery } from '../models/log-query';
import { LogItem } from '../models/log-item';
import { PagedResult } from '../models/paged-result';



@Injectable({
  providedIn: 'root'
})
export class AdminLogsService {
  private BASE_URL = 'https://localhost:7152/api/admin/logs';

  constructor(private http: HttpClient) {}

  getLogs(q: LogsQuery): Observable<PagedResult<LogItem>> {
    let params = new HttpParams()
      .set('page', String(q.page ?? 1))
      .set('pageSize', String(q.pageSize ?? 20));

    if (q.level)   params = params.set('level', q.level);
    if (q.search)  params = params.set('search', q.search);
    if (q.fromUtc) params = params.set('fromUtc', q.fromUtc);
    if (q.toUtc)   params = params.set('toUtc', q.toUtc);

    return this.http.get<PagedResult<LogItem>>(this.BASE_URL, { params });
  }

  getById(id: string): Observable<LogItem> {
    return this.http.get<LogItem>(`${this.BASE_URL}/${id}`);
  }
}
