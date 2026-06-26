import {
  Component, Input, Output, EventEmitter,
  inject, OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  selectCurrentUser,
  selectUserRole
} from '../../../../core/store/auth/auth.selectors';
import { logout } from '../../../../core/store/auth/auth.actions';

export interface NavItem {
  label: string;
  icon: string;
  route: string;
  badge?: number;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatTooltipModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  @Input() isOpen = true;
  @Output() toggleSidebar = new EventEmitter<void>();

  private store  = inject(Store);
  private router = inject(Router);

  user$ = this.store.select(selectCurrentUser);
  role$ = this.store.select(selectUserRole);

  navItems: NavItem[] = [];

  private adminNav: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',           route: '/admin'              },
    { label: 'User Management', icon: 'manage_accounts',     route: '/admin/users'        },
    { label: 'Classes',         icon: 'class',               route: '/admin/classes'      },
    { label: 'Timetable',       icon: 'calendar_today',      route: '/admin/timetable'    },
    { label: 'Attendance',      icon: 'fact_check',          route: '/admin/attendance'   },
    { label: 'Marks & Grades',  icon: 'grade',               route: '/admin/marks'        },
    { label: 'Exam Schedule',   icon: 'event_note',          route: '/admin/exams'        },
    { label: 'Fee Management',  icon: 'payments',            route: '/admin/fees'         },
    { label: 'Notice Board',    icon: 'campaign',            route: '/admin/notices'      },
    { label: 'Complaints',      icon: 'report_problem',      route: '/admin/complaints'   },
    { label: 'Library',         icon: 'local_library',       route: '/admin/library'      },
    { label: 'Audit Logs',      icon: 'history',             route: '/admin/audit-log'        },
    { label: 'Assignments',     icon: 'assignment',          route: '/admin/assignments'  },
    { label: 'Leave Approvals',  icon: 'event_available',    route: '/admin/leave-approvals'  },



  ];

  private teacherNav: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',           route: '/teacher'            },
    { label: 'Attendance',      icon: 'fact_check',          route: '/teacher/attendance' },
    { label: 'Marks Entry',     icon: 'grade',               route: '/teacher/marks'      },
    { label: 'Assignments',     icon: 'assignment',          route: '/teacher/assignments'},
    { label: 'Doubt Inbox',     icon: 'forum',               route: '/teacher/doubts'     },
    { label: 'Leave Approvals', icon: 'event_available',     route: '/teacher/leaves'     },
    { label: 'Timetable',       icon: 'calendar_today',      route: '/teacher/timetable'  },
    { label: 'Notices',         icon: 'notifications',       route: '/teacher/notices'    },
  ];

  private studentNav: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',           route: '/student'            },
    { label: 'Attendance',      icon: 'fact_check',          route: '/student/attendance' },
    { label: 'Report Card',     icon: 'description',         route: '/student/marks'},
    { label: 'Assignments',     icon: 'assignment',          route: '/student/assignments'},
    { label: 'Ask a Doubt',     icon: 'help',                route: '/student/doubts'     },
    { label: 'Timetable',       icon: 'calendar_today',      route: '/student/timetable'  },
    { label: 'Exam Schedule',   icon: 'event_note',          route: '/student/exams'      },
    { label: 'Notices',         icon: 'notifications',       route: '/student/notices'    },
    { label: 'Library',         icon: 'local_library',       route: '/student/library'    },
    { label: 'Leave',            icon: 'event_busy',    route: '/student/leave'           },
    { label: 'Complaints',  icon: 'report_problem', route: '/student/complaints' },
  ];

  private parentNav: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',           route: '/parent'             },
    { label: 'Child Progress',  icon: 'insights',            route: '/parent/progress'    },
    { label: 'Attendance',      icon: 'fact_check',          route: '/parent/attendance'  },
    { label: 'Fee Payment',     icon: 'payments',            route: '/parent/fees'        },
    { label: 'Homework',        icon: 'assignment',          route: '/parent/homework'    },
    { label: 'Chat Teacher',    icon: 'chat',                route: '/parent/chat'        },
    { label: 'PTM Schedule',    icon: 'groups',              route: '/parent/ptm'         },
    { label: 'Notices',         icon: 'notifications',       route: '/parent/notices'     },
  ];

  ngOnInit(): void {
    this.role$.subscribe(role => {
      switch (role?.toLowerCase()) {
        case 'principal': this.navItems = this.adminNav;   break;
        case 'teacher':   this.navItems = this.teacherNav; break;
        case 'student':   this.navItems = this.studentNav; break;
        case 'parent':    this.navItems = this.parentNav;  break;
      }
    });
  }

  isActive(route: string): boolean {
    return this.router.url === route ||
           (route !== '/admin' && this.router.url.startsWith(route));
  }

  onLogout(): void {
    this.store.dispatch(logout());
  }
}