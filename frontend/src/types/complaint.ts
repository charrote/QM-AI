// ─── 客诉记录 ──────────────────────────────────────────────

export interface Complaint {
  id: number
  complaintCode: string
  customerId: number
  severity: string
  subject: string
  description: string
  status: string
  fiveW2HJson?: string
  assignedTo?: number
  dueDate?: string
  acknowledgedAt?: string
  closedAt?: string
  createdBy: number
  createdAt: string
  updatedAt: string
  customerName?: string
}

// ─── 8D 报告 ──────────────────────────────────────────────

export interface D8Report {
  id: number
  complaintId: number
  d0Description?: string
  d1Team?: string
  d2Description?: string
  d3Measures?: string
  d4AnalysisMethod?: string
  d4Content?: string
  d4RootCause?: string
  d5Actions?: string
  d6Verification?: string
  d7Preventive?: string
  d8Thanks?: string
  currentDiscipline: number
  status: string
  createdAt: string
  updatedAt: string
}

// ─── 客诉事件时间线 ────────────────────────────────────────

export interface ComplaintEvent {
  id: number
  complaintId: number
  eventType: string
  eventData?: string
  createdBy: number
  createdAt: string
}

// ─── Create/Update DTOs ───────────────────────────────────

export interface CreateComplaint {
  complaintCode: string
  customerId: number
  severity: string
  subject: string
  description: string
  assignedTo?: number
  dueDate?: string
}

export interface UpdateComplaint {
  severity?: string
  subject?: string
  description?: string
  assignedTo?: number | null
  dueDate?: string | null
}

export interface TransitionStatus { newStatus: string }

export interface AdvanceDiscipline { disciplineIndex: number | null }

export interface CreateD8Report { complaintId: number; d0Description: string }

export interface UpdateD8Report {
  d0Description?: string; d1Team?: string; d2Description?: string; d3Measures?: string; d4AnalysisMethod?: string; d4Content?: string; d4RootCause?: string; d5Actions?: string; d6Verification?: string; d7Preventive?: string; d8Thanks?: string }

// ─── Constants ─────────────────────────────────────────────

export const COMPLAINT_SEVERITY_OPTIONS = [ { value:'critical', label:'致命', type:'danger' }, { value:'major', label:'严重', type:'warning' }, { value:'minor', label:'轻微', type:'info' } ];

export const COMPLAINT_SEVERITY_MAP: Record<string,string> = { critical:'致命', major:'严重', minor:'轻微' };

export const COMPLAINT_STATUS_OPTIONS = [ { value:'new', label:'新建', type:'info' }, { value:'acknowledged', label:'已确认', type:'primary' }, { value:'in_progress', label:'处理中', type:'warning' }, { value:'overdue', label:'已逾期', type:'danger' }, { value:'awaiting_verify', label:'等待验证', type:'primary' }, { value:'closed', label:'已关闭', type:'success' } ];

export const COMPLAINT_STATUS_MAP: Record<string,string> = { new:'新建', acknowledged:'已确认', in_progress:'处理中', overdue:'已逾期', awaiting_verify:'等待验证', closed:'已关闭' };

export const D8_DISCIPLINE_LABELS = [ { discipline:0, label:'D0·问题概述' }, { discipline:1, label:'D1·改善小组' }, { discipline:2, label:'D2·问题描述' }, { discipline:3, label:'D3·临时围堵' }, { discipline:4, label:'D4·根本原因' }, { discipline:5, label:'D5·纠正措施' }, { discipline:6, label:'D6·实施验证' }, { discipline:7, label:'D7·预防措施' }, { discipline:8, label:'D8·结案致谢' } ];

export const D8_DISCIPLINE_MAP: Record<number,string> = Object.fromEntries(D8_DISCIPLINE_LABELS.map(d=>[d.discipline,d.label]));

export const D8_STATUS_OPTIONS = [ { value:'in_progress', label:'进行中', type:'primary' }, { value:'completed', label:'已完成', type:'success' }, { value:'closed', label:'已关闭', type:'info' } ];

export const EVENT_TYPE_MAP: Record<string,string> = { created:'创建', acknowledged:'确认接收', status_change:'状态变更', d8_update:'8D更新', verify:'验证', close:'关闭' };
