export interface LogItem {
  logType: 'Info' | 'Warning' | 'Error' | string; // şimdilik string
  message: string;
  source?: string;
  userId?: string;
  createdAtUtc: string;      // ISO UTC
  correlationId?: string;
  exception?: string;
  data?: any;
}
