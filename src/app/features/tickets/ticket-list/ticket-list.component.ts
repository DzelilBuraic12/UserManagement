import { Component, OnInit } from '@angular/core';
import { TicketService } from '../../../core/services/ticket.service';
import { Ticket } from '../../../shared/models/ticket.model';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { TicketQuery } from '../../../shared/models/ticket-query';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-ticket-list',
  imports: [CommonModule, RouterModule,FormsModule],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.scss'
})
export class TicketListComponent implements OnInit {
  tickets: Ticket[] = [];
  total = 0;
  page: number = 1;
  pageSize: number = 10;
  selectedStatusId: number = 0;
  searchTerm: string = '';
  message: string = '';
  showHistory: boolean = false;
  loading: boolean = false;  

  constructor(
    private ticketService: TicketService,
    public authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    console.log("TICKET LIST ngOnInit: STARTED!");
    const created = this.route.snapshot.queryParamMap.get('created');
    const updated = this.route.snapshot.queryParamMap.get('updated');

    if (created) {
      this.message = 'Ticket created successfully.';
    }

    if (updated) {
      this.message = 'Ticket updated successfully.';
    }
    this.loadTickets();
  }

  toggleHistory() {
    this.showHistory = !this.showHistory;
    this.loadTickets();
  }

  loadTickets() {
    this.loading = true;  

    let query: TicketQuery = {
      page: this.page,
      pageSize: this.pageSize,
      includeClosed: this.showHistory
    }

    if (this.selectedStatusId !== 0) {
      query.statusId = this.selectedStatusId;
    }
    if (this.searchTerm.trim() !== '') {
      query.search = this.searchTerm.trim();
    }

    if (this.authService.isUser()) {
      query.createdById = this.authService.getCurrentUserId() ?? undefined;
    } else if (this.authService.isTechnician()) {
      query.myAssignedOnly = "true";
    }

    this.ticketService.getTickets(query)
      .subscribe({
        next: (result) => {
          this.tickets = result.data;
          this.total = result.total;
          this.loading = false;  
        },
        error: (err) => {
          console.error('Error loading tickets', err);
          this.loading = false;  
        }
      })
  }

  goToPreviousPage() {
    if (this.page > 1) {
      this.page = this.page - 1;
      this.loadTickets()
    }
  }

  goToNextPage() {
    let number = this.page * this.pageSize;

    if (number < this.total) {
      this.page = this.page + 1;
      this.loadTickets()
    }
  }

  goToTicketDetail(id: number) {
    this.router.navigate(['/tickets', id])
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }

  clearFilters() {
    this.selectedStatusId = 0;
    this.searchTerm = '';
    this.page = 1;

    this.loadTickets();
  }
}


