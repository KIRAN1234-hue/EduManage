export interface AuditLogEntry {
  id:         string;
  userName:   string;
  userEmail:  string;
  action:     string;
  entityName: string;
  entityId:   string;
  newValues?: string;
  oldValues?: string;
  ipAddress:  string;
  timestamp:  string;
}

export interface AuditLogResponse {
  total:    number;
  page:     number;
  pageSize: number;
  data:     AuditLogEntry[];
}