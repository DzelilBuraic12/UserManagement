import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { AuthService } from "../../../core/services/auth.service";
import { CommonModule } from "@angular/common";
@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    imports: [ReactiveFormsModule,CommonModule,RouterLink],
})

export class LoginComponent implements OnInit{
    loginForm!: FormGroup;
    errorMessage: string = '';
    isLoading: boolean = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router
    ){}

    ngOnInit(): void {
        this.loginForm = this.fb.group({
            email: ['', [Validators.email, Validators.required]],
            password: ['', [Validators.minLength(6), Validators.required]]
        })
    }

    onSubmit(): void{
        if(this.loginForm.invalid) {
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        this.authService.login(this.loginForm.value).subscribe({
            next: () => {
                this.router.navigate(['/dashboard']);
            },
            error: (err) => {
                this.errorMessage = err.error?.message || 'Login failed. Please try again.';
                this.isLoading = false;
            },
            complete: () => {
                this.isLoading = false;
            }
        });
    }

    goToRegister(): void {
        this.router.navigate(['/register']);
    }
    
    get email(){
        return this.loginForm.get('email');
    }

    get password(){
        return this.loginForm.get('password');
    }
}

