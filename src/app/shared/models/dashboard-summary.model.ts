export interface DashboardSummary {
  open: number;
  inProgress: number;
  resolved: number;
  closed: number;
  trends: DashboardTrends;
}

export interface DashboardTrends {
  open: number;
  inProgress: number;
  resolved: number;
  closed: number;
}
