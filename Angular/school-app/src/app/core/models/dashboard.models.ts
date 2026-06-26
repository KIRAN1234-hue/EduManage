export interface DashboardStats {
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;
  totalSubjects: number;
  pendingLeaveApplications: number;
  totalAssignments: number;
  totalNotices: number;
  attendanceTodayPercent: number;
  recentActivities: RecentActivity[];
}

export interface RecentActivity {
  type: string;
  message: string;
  occurredAt: string;
}