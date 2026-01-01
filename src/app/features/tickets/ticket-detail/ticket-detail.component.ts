import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router} from '@angular/router';
import { TicketService } from '../../../core/services/ticket.service';
import { Ticket } from '../../../shared/models/ticket.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from "@angular/forms";
import { AuthService } from '../../../core/services/auth.service';
import { Technician } from '../../../shared/models/technician.model';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-ticket-detail',
  imports: [CommonModule, FormsModule],
  templateUrl: './ticket-detail.component.html',
  styleUrls: ['./ticket-detail.component.scss']
})
export class TicketDetailComponent implements OnInit{

  ticket: Ticket | null = null;
  ticketId: number = 0;
  errorMessage: string = '';
  isLoading: boolean = false;
  message: string = '';
  selectedStatusId: number = 1;
  selectedTechnicianId: number = 0;
  technicians: Technician[] = [];
  allowedStatuses: { id: number; name: string}[] = [];

  constructor(
    private route: ActivatedRoute,
    private ticketService: TicketService,
    private router: Router,
    public authService: AuthService,
    private userService: UserService
  ){}

  loadTechnicians() : void {
    this.userService.getTechnicians().subscribe({
      next: (result) => {
        this.technicians = result;
      },
      error: () => {
        this.errorMessage = 'Error loading technicians';
      }
    });
  }

  loadTicketData() : void {
    const id = this.route.snapshot.paramMap.get('id');

    if(id){
      this.ticketId = parseInt(id, 10)

      this.ticketService.getTicketById(this.ticketId).subscribe({
        next: (result) => {
          this.ticket = result;
          console.log('ticket.technicianId:', this.ticket?.technicianId);
          console.log('currentUserId:', this.authService.getCurrentUserId());
          console.log('isTechnician():', this.authService.isTechnician());
          const canSeeTechBlock =
          this.authService.isTechnician() &&
          this.ticket?.technicianId === this.authService.getCurrentUserId();
        console.log('canSeeTechBlock:', canSeeTechBlock);
          const currentStatusId = result.statusId;
          if(currentStatusId === 1) {
            this.allowedStatuses = [{ id: 2, name: 'In Progress'}]
          }else if(currentStatusId === 2) {
            this.allowedStatuses = [{ id: 3, name: 'Resolved'}]
          }else if(currentStatusId === 3) {
            this.allowedStatuses = [{ id: 4, name: 'Closed'}]
          }else {
            this.allowedStatuses = []
          }

          if(this.allowedStatuses.length > 0) {
            this.selectedStatusId = this.allowedStatuses[0].id;
          }else {
            this.selectedStatusId = 0;
          }
          this.isLoading = false;
          console.log('ticket.createdById:', result.createdById);
          console.log('currentUserId:', this.authService.getCurrentUserId());
        },
        error: () => {
          this.errorMessage = 'Error loading ticket details.';
          this.isLoading = false;
        }
      })
    }
    if(this.authService.isAdmin()){
      this.loadTechnicians()
    }
  }
  ngOnInit(): void {
    console.log('Role:', this.authService.getCurrentUserRole());
    console.log('isUser():', this.authService.isUser());
    this.loadTicketData();
  }

  goBack(): void {
    this.router.navigate(["/tickets"]);
  }

  editTicket(): void {
    this.router.navigate([`/tickets/edit/${this.ticketId}`])
  }

  changeStatus(statusId: number) {
    console.log('chnageStatus called with statusid', statusId);
    if(statusId === 4){
      const confirmed = confirm('Are you sure you want to close this ticket?');
      if(!confirmed) {
        return;
      }
    }

    this.ticketService.changeStatus(this.ticketId, statusId).subscribe({
      next: () => {
        this.errorMessage = '';
        this.message = "Status changed.";
        this.loadTicket();
      },
      error: () => {
        this.message = '';
        this.errorMessage = 'Cannot change status.';
      }
    })
  }

  assignTechnician(technicianId: number) {
    this.ticketService.assignTechnician(this.ticketId, technicianId).subscribe({
      next: () => {
        this.errorMessage = '';
        this.message = "Technician assigned.";
        this.loadTicket();
      },
      error: () => {
        this.message = '';
        this.errorMessage = 'Cannot change technician';
      }
    })
  }

  loadTicket() {
    this.loadTicketData();
  }

  get currentUserId(): number | null {
    return this.authService.getCurrentUserId();
  }
}
