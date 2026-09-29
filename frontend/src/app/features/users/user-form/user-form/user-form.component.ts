import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/user.service';
import { User } from '../../../../shared/models/user.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss',
})
export class UserFormComponent implements OnInit {
  userForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  errorMessage = '';
  userId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      role: ['User', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    this.userId = idParam ? Number(idParam) : null;
    this.isEditMode = !!this.userId;

    if (this.isEditMode) {
      this.userForm.get('password')?.clearValidators();
      this.userForm.get('password')?.updateValueAndValidity();
      this.loadUser();
    }
  }

  loadUser(): void {
    if (!this.userId) return;
    this.isLoading = true;

    this.userService.getById(this.userId).subscribe({
      next: (user: User) => {
        this.isLoading = false;
        this.userForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          role: user.role,
        });
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Error loading user.';
      },
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    const formData = this.userForm.value;

    const request$ = this.isEditMode && this.userId
      ? this.userService.updateUser(this.userId, formData)
      : this.userService.createUser(formData);

    request$.subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/users']);
      },
      error: (err) => {
        console.log(err);
        this.isLoading = false;
        this.errorMessage = 'Error saving user.';
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/users']);
  }

  get firstName() {
    return this.userForm.get('firstName');
  }
  get lastName() {
    return this.userForm.get('lastName');
  }
  get email() {
    return this.userForm.get('email');
  }
  get role() {
    return this.userForm.get('role');
  }
  get password() {
    return this.userForm.get('password');
  }
}
