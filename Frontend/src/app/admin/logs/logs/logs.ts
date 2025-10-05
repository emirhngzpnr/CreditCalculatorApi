import { Component, OnInit, inject, computed, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { DatePipe, NgClass } from '@angular/common'; 
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AdminLogsService } from '../../../services/admin.logs.service';
import { LogItem } from '../../../models/log-item';

type LogLevel = '' | 'Info' | 'Warning' | 'Error';

@Component({
  selector: 'app-admin-logs',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, NgClass],
  templateUrl: './logs.html',
  styleUrl: './logs.css'
})
export class Logs implements OnInit {
  private api = inject(AdminLogsService);
  private fb  = inject(FormBuilder);

  form = this.fb.group({
    level: ['' as LogLevel],
    search: [''],
    from: [''],
    to:   ['']
  });

  // state
  rows: LogItem[] = [];
  total = 0;
  page  = 1;
  pageSize = 120;
  loading = false;

  // computed helper (UI’de sayfa bilgisi göstermek için)
  totalPages = computed(() => Math.max(1, Math.ceil(this.total / this.pageSize)));

  ngOnInit() {
    // sadece arama kutusu debounce ile fetch etsin
    this.form.controls.search.valueChanges
      ?.pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.fetch(1));

    this.fetch(1);
  }

  private toUtcIso(v?: string | null) {
    return v ? new Date(v).toISOString() : undefined;
  }

  fetch(page = this.page) {
    // tarih aralığı kontrolü
    const { from, to } = this.form.value;
    if (from && to && new Date(from) > new Date(to)) {
      // küçük bir guard: from>to ise to'yu temizle
      this.form.patchValue({ to: '' }, { emitEvent: false });
    }

    this.loading = true;
    const v = this.form.value;

    this.api.getLogs({
      level: (v.level || '') as LogLevel,
      search: v.search || undefined,
      fromUtc: this.toUtcIso(v.from),
      toUtc:   this.toUtcIso(v.to),
      page,
      pageSize: this.pageSize
    }).subscribe({
      next: res => {
        this.rows     = res.items;
        this.total    = res.totalCount;
        this.page     = res.page;
        this.pageSize = res.pageSize;
        this.loading  = false;
      },
      error: _ => this.loading = false
    });
  }
// logs.component.ts
badgeMap: Record<string, string> = {
  Info: 'text-bg-secondary',
  Warning: 'text-bg-warning',
  Error: 'text-bg-danger',
  LoginStatus: 'text-bg-primary',   
  Debug: 'text-bg-info',            
  Trace: 'text-bg-light text-dark'  
};

  apply()  { this.fetch(1); }
  reset()  { this.form.reset({ level: '' }); this.fetch(1); }
  prev()   { if (this.page > 1) this.fetch(this.page - 1); }
  next()   { if (this.page < this.totalPages()) this.fetch(this.page + 1); }
  go(p: number) { if (p >= 1 && p <= this.totalPages()) this.fetch(p); }
}
