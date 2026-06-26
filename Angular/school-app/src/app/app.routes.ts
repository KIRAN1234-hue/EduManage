  import { Routes } from '@angular/router';
  import { authGuard } from './core/guards/auth.guard';
  import { roleGuard } from './core/guards/role.guard';
  import { MainLayoutComponent } from './shared/components/layout/main-layout/main-layout.component';

  export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },

    // ── Public routes ─────────────────────────────────────────────────────────
    {
      path: 'login',
      loadComponent: () =>
        import('./features/auth/login/login.component').then(m => m.LoginComponent)
    },
    {
      path: 'register/teacher',
      loadComponent: () =>
        import('./features/auth/register-teacher/register-teacher.component')
          .then(m => m.RegisterTeacherComponent)
    },
    {
      path: 'register/student',
      loadComponent: () =>
        import('./features/auth/register-student/register-student.component')
          .then(m => m.RegisterStudentComponent)
    },
    {
      path: 'register/parent',
      loadComponent: () =>
        import('./features/auth/register-parent/register-parent.component')
          .then(m => m.RegisterParentComponent)
    },
    {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component')
        .then(m => m.ForgotPasswordComponent)
  },
    {
      path: 'unauthorized',
      loadComponent: () =>
        import('./shared/components/unauthorized/unauthorized.component')
          .then(m => m.UnauthorizedComponent)
    },

    // ── Admin (Principal) ─────────────────────────────────────────────────────
    {
      path: 'admin',
      component: MainLayoutComponent,
      canActivate: [authGuard, roleGuard('principal')],
      children: [
        {
          path: '',
          loadComponent: () =>
            import('./features/admin/dashboard/admin-dashboard.component')
              .then(m => m.AdminDashboardComponent)
        },
        {
          path: 'users',
          loadComponent: () =>
            import('./features/admin/user-management/user-management.component')
              .then(m => m.UserManagementComponent)
        },
        {
          path: 'attendance',
          loadComponent: () =>
            import('./features/teacher/attendance/teacher-attendance.component')
              .then(m => m.TeacherAttendanceComponent)
        },
        {
          path: 'marks',
          loadComponent: () =>
            import('./features/teacher/marks/teacher-marks.component')
              .then(m => m.TeacherMarksComponent)
        },

        {
          path: 'assignments',
          loadComponent: () =>
            import('./features/teacher/assignments/teacher-assignments.component')
              .then(m => m.TeacherAssignmentsComponent)
        },

        // NEW ROUTES
        {
          path: 'notices',
          loadComponent: () =>
            import('./shared/components/notices/notices.component')
              .then(m => m.NoticesComponent)
        },
        {
          path: 'leave-approvals',
          loadComponent: () =>
            import('./features/admin/leave-approvals/leave-approvals.component')
              .then(m => m.LeaveApprovalsComponent)
        },
        {
    path: 'timetable',
    loadComponent: () =>
      import('./features/admin/timetable/timetable.component')
        .then(m => m.TimetableComponent)
  },
  {
    path: 'exams',
    loadComponent: () =>
      import('./features/admin/exam-schedule/exam-schedule.component')
        .then(m => m.ExamScheduleComponent)
  },

  {
    path: 'fees',
    loadComponent: () =>
      import('./features/admin/fees/fee-management.component')
        .then(m => m.FeeManagementComponent)
  },
  {
    path: 'library',
    loadComponent: () =>
      import('./features/admin/library/library-management.component')
        .then(m => m.LibraryManagementComponent)
  },

  {
  path: 'complaints',
  loadComponent: () =>
    import('./features/admin/complaints/complaints-management.component')
      .then(m => m.ComplaintsManagementComponent)
},
{
  path: 'audit-log',
  loadComponent: () =>
    import('./features/admin/audit-log/audit-log.component')
      .then(m => m.AuditLogComponent)
},


        { path: '**', redirectTo: '' }
      ]
    },

    // ── Teacher ───────────────────────────────────────────────────────────────
    {
      path: 'teacher',
      component: MainLayoutComponent,
      canActivate: [authGuard, roleGuard('teacher')],
      children: [
        {
          path: '',
          loadComponent: () =>
            import('./features/teacher/dashboard/teacher-dashboard.component')
              .then(m => m.TeacherDashboardComponent)
        },
        {
          path: 'attendance',
          loadComponent: () =>
            import('./features/teacher/attendance/teacher-attendance.component')
              .then(m => m.TeacherAttendanceComponent)
        },
        {
          path: 'marks',
          loadComponent: () =>
            import('./features/teacher/marks/teacher-marks.component')
              .then(m => m.TeacherMarksComponent)
        },

        // NEW ROUTES
        {
          path: 'assignments',
          loadComponent: () =>
            import('./features/teacher/assignments/teacher-assignments.component')
              .then(m => m.TeacherAssignmentsComponent)
        },
        {
          path: 'notices',
          loadComponent: () =>
            import('./shared/components/notices/notices.component')
              .then(m => m.NoticesComponent)
        },
        {
          path: 'leaves',
          loadComponent: () =>
            import('./features/admin/leave-approvals/leave-approvals.component')
              .then(m => m.LeaveApprovalsComponent)
        },

        {
    path: 'timetable',
    loadComponent: () =>
      import('./features/admin/timetable/timetable.component')
        .then(m => m.TimetableComponent)
  },
  {
    path: 'library',
    loadComponent: () =>
      import('./features/admin/library/library-management.component')
        .then(m => m.LibraryManagementComponent)
  },

  {
  path: 'doubts',
  loadComponent: () =>
    import('./features/teacher/doubts/teacher-doubts.component')
      .then(m => m.TeacherDoubtsComponent)
},


        { path: '**', redirectTo: '' }
      ]
    },

    // ── Student ───────────────────────────────────────────────────────────────
    {
      path: 'student',
      component: MainLayoutComponent,
      canActivate: [authGuard, roleGuard('student')],
      children: [
        {
          path: '',
          loadComponent: () =>
            import('./features/student/dashboard/student-dashboard.component')
              .then(m => m.StudentDashboardComponent)
        },
        {
          path: 'attendance',
          loadComponent: () =>
            import('./features/student/attendance/student-attendance.component')
              .then(m => m.StudentAttendanceComponent)
        },
        {
          path: 'marks',
          loadComponent: () =>
            import('./features/student/marks/student-marks.component')
              .then(m => m.StudentMarksComponent)
        },

        // NEW ROUTES
        {
          path: 'assignments',
          loadComponent: () =>
            import('./features/student/assignments/student-assignments.component')
              .then(m => m.StudentAssignmentsComponent)
        },
        {
          path: 'leave',
          loadComponent: () =>
            import('./features/student/leave/student-leave.component')
              .then(m => m.StudentLeaveComponent)
        },
        {
          path: 'notices',
          loadComponent: () =>
            import('./shared/components/notices/notices.component')
              .then(m => m.NoticesComponent)
        },

        {
    path: 'timetable',
    loadComponent: () =>
      import('./features/admin/timetable/timetable.component')
        .then(m => m.TimetableComponent)
  },
  {
    path: 'exams',
    loadComponent: () =>
      import('./features/admin/exam-schedule/exam-schedule.component')
        .then(m => m.ExamScheduleComponent)
  },

  {
    path: 'library',
    loadComponent: () =>
      import('./features/student/library/student-library.component')
        .then(m => m.StudentLibraryComponent)
  },
  {
    path: 'fees',
    loadComponent: () =>
      import('./features/student/fees/student-fees.component')
        .then(m => m.StudentFeesComponent)
  },

    {
  path: 'doubts',
  loadComponent: () =>
    import('./features/student/doubts/student-doubts.component')
      .then(m => m.StudentDoubtsComponent)
},
{
  path: 'complaints',
  loadComponent: () =>
    import('./features/student/complaints/student-complaints.component')
      .then(m => m.StudentComplaintsComponent)
},

        { path: '**', redirectTo: '' }
      ]
    },

    // ── Parent ────────────────────────────────────────────────────────────────
    {
      path: 'parent',
      component: MainLayoutComponent,
      canActivate: [authGuard, roleGuard('parent')],
      children: [
        {
          path: '',
          loadComponent: () =>
            import('./features/parent/dashboard/parent-dashboard.component')
              .then(m => m.ParentDashboardComponent)
        },
        {
          path: 'attendance',
          loadComponent: () =>
            import('./features/parent/attendance/parent-attendance.component')
              .then(m => m.ParentAttendanceComponent)
        },

        // NEW ROUTES
        {
          path: 'notices',
          loadComponent: () =>
            import('./shared/components/notices/notices.component')
              .then(m => m.NoticesComponent)
        },

        {
    path: 'fees',
    loadComponent: () =>
      import('./features/parent/fees/parent-fees.component')
        .then(m => m.ParentFeesComponent)
  },


        { path: '**', redirectTo: '' }
      ]
    },

    { path: '**', redirectTo: 'login' }
  ];