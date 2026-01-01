import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators,ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TicketService } from '../../../core/services/ticket.service';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-ticket-form',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './ticket-form.component.html',
  styleUrl: './ticket-form.component.scss'
})
export class TicketFormComponent implements OnInit{

  ticketId: number = 0;
  isLoading: boolean = false;
  errorMessage: string = '';
  message: string = '';
  form!: FormGroup;
  originalDueDate?: string | null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private ticketService: TicketService,
    private router: Router,
    public authService: AuthService
  ){}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if(id) {
      this.ticketId = parseInt(id, 10);
    }

    this.initForm();

    if(this.ticketId > 0) {
      this.loadTicket();
    }
  }

  initForm() {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      statusId: [null],
      dueDate: ['', Validators.required], 
      priority: ['']
    })

  }

  loadTicket() {

    if(this.ticketId > 0){
      this.ticketService.getTicketById(this.ticketId).subscribe({
        next: (result) => {
          const dueIso: string |null = result.dueDate;
          const dueForInput = result.dueDate ? result.dueDate.substring(0, 10) : '';
          this.originalDueDate = dueForInput;

          this.form?.patchValue({
            title: result.title,
            description: result.description,
            statusId: result.statusId,
            dueDate: dueForInput,
            priority: result.priority
          })
          this.isLoading = false;
        }
      })
    }
  }
  

  submit() {
    if(this.ticketId == 0){
      this.createTicket();
    }else{
      this.updateTicket();
    }
  }

  createTicket() {
    this.ticketService.createTicket(this.form?.value).subscribe({
      next: () => {
        this.message = 'Ticket created successfully.';
      }
    })
  }

  updateTicket() {
    const dto: any = { ...this.form.value};

    if(this.originalDueDate && dto.dueDate === this.originalDueDate){
      delete dto.dueDate;
    }
    this.ticketService.updateTicket(this.ticketId,dto).subscribe({
      next: () => {
       this.message = 'Ticket updated successfully.'
      }
    })
  }

  goBack(){
    this.router.navigate(['/tickets']);
  }


}
