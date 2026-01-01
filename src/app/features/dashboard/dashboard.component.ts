import { OnInit, Component, OnDestroy } from "@angular/core";
import { AuthService } from "../../core/services/auth.service";
import { User } from "../../shared/models/user.model";
import { CommonModule } from "@angular/common";
import { Router } from "@angular/router";
import { RequestSummary } from "../../shared/models/RequestSummary.model";
import { TicketService } from "../../core/services/ticket.service";
import { RecentActivity } from "../../shared/models/recent-activity.model";
import { HighPriorityTicket } from "../../shared/models/high-priority-ticket.model";
import { PriorityBreakdown } from "../../shared/models/priority-breakdown.model";
import { DashboardSummary } from "../../shared/models/dashboard-summary.model";
import { Subject, takeUntil } from "rxjs";

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss'],
    imports: [CommonModule]
})
export class DashboardComponent implements OnInit, OnDestroy {
    currentUser: User | null = null;
    summary: RequestSummary | null = null;
    errorMessage: string = '';
    recentActivities: RecentActivity[] = [];
    highPriorityTickets: HighPriorityTicket[] = [];
    priorityBreakdown: PriorityBreakdown = { high: 0, normal: 0, low: 0 }
    resolvedToday: number = 0;
    dashboardSummary: DashboardSummary = {
        open: 0,
        inProgress: 0,
        resolved: 0,
        closed: 0,
        trends: {
            open: 0,
            inProgress: 0,
            resolved: 0,
            closed: 0
        }
    };

    private destroy$ = new Subject<void>();

    constructor(
        private authService: AuthService,
        private router: Router,
        private ticketService: TicketService
    ) {}

    ngOnInit(): void {
        this.authService.currentUser$
            .pipe(takeUntil(this.destroy$))
            .subscribe(user => {
                this.currentUser = user;
                
                if (user) {
                    this.loadData();
                }
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadData(): void {
        this.loadSummary();
        this.loadRecentActivity();

        if (this.currentUser?.role === 'Admin' || this.currentUser?.role === 'Technician') {
            this.loadDashboardSummary();
            this.loadHighPriorityTickets();
            this.loadPriorityBreakdown();
            this.loadResolvedToday();
        }
    }

    private loadSummary(): void {
        this.ticketService.getSummary()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.summary = result;
                    this.errorMessage = '';
                },
                error: (err) => {
                    this.errorMessage = 'Error loading summary.';
                    console.error('Error loading summary', err);
                }
            });
    }

    logout(): void {
        this.authService.logout();
    }

    navigateToUsers(): void {
        this.router.navigate(['/users']);
    }

    navigateToTickets(): void {
        this.router.navigate(["/tickets"]);
    }

    createTicket(): void {
        this.router.navigate(["/tickets/create"]);
    }

    loadRecentActivity(): void {
        this.ticketService.getRecentActivity()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.recentActivities = result;
                },
                error: (err) => {
                    console.error('Error loading recent activity', err);
                }
            });
    }

    loadHighPriorityTickets(): void {
        this.ticketService.getHighPriorityTickets()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.highPriorityTickets = result;
                },
                error: (err) => {
                    console.error('Error loading high priority tickets.', err);
                }
            });
    }

    loadPriorityBreakdown(): void {
        this.ticketService.getPriorityBreakdown()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.priorityBreakdown = result;
                },
                error: (err) => {
                    console.error('Error loading priority breakdown', err);
                }
            });
    }

    loadResolvedToday(): void {
        this.ticketService.getResolvedToday()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.resolvedToday = result.resolvedToday;
                },
                error: (err) => {
                    console.error('Error loading resolved', err);
                }
            });
    }
    
    loadDashboardSummary(): void {
        this.ticketService.getDashboardSummary()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.dashboardSummary = result;
                },
                error: (err) => {
                    console.error('Error loading dashboard summary', err);
                }
            });
    }

    calculateWidth(count: number): number {
        const total = this.priorityBreakdown.high + this.priorityBreakdown.normal + 
        this.priorityBreakdown.low;

        if (total === 0)
            return 0;

        return (count / total) * 100;
    }

    formatTrend(value: number): string {
        if (value > 0) {
            return `+${value} today`;
        } else if (value < 0) {
            return `${value} today`;
        } else {
            return 'No change';
        }
    }
}
