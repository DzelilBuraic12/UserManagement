import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from "@angular/forms";
import { AuthService } from "../../../core/services/auth.service";
import { Router, RouterLink } from "@angular/router";
import { CommonModule } from "@angular/common";
@Component ({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
    imports: [ReactiveFormsModule,CommonModule,RouterLink]
})

export class RegisterComponent implements OnInit {
    registerForm!: FormGroup;
    errorMessage: string = '';
    successMessage: string = '';
    isLoading: boolean = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router
    ) {}

    static passwordsMatch(group: AbstractControl): ValidationErrors | null{
        const password = group.get('password');
        const confirmPassword = group.get('confirmPassword');

        const passwordValue = password?.value;
        const confirmPasswordValue = confirmPassword?.value;

        if(!password || !confirmPassword){
            return null;
        }

        if(passwordValue !== confirmPasswordValue){
            return { passwordsMismatch: true };
        }

        return null;
    }

    ngOnInit(): void {
        const strongPasswordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

        this.registerForm = this.fb.group({
            firstName: ['', [Validators.required]],
            lastName: ['', [Validators.required]],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6), Validators.pattern(strongPasswordPattern)]],
            confirmPassword: ['', [Validators.required, Validators.minLength(6), Validators.pattern(strongPasswordPattern)]]
        }, {
            validators: RegisterComponent.passwordsMatch
        })
    }

    onSubmit(): void{
        if(this.registerForm.invalid)
        {
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        this.authService.register(this.registerForm.value).subscribe({
            next: () => {
                this.successMessage = 'Registration successful! Redirecting...';
                this.router.navigate(["/login"]);
            },
            error: (err) => {
                this.errorMessage = err.error?.message;
                this.isLoading = false;
            },
            complete: () => {
                this.isLoading = false;
            }
        })
    }

    get firstName(){
        return this.registerForm.get('firstName');
    }
    get lastName(){
        return this.registerForm.get('lastName');
    }
    get email(){
        return this.registerForm.get('email');
    }
    get password(){
        return this.registerForm.get('password');
    }
    get confirmPassword(){
        return this.registerForm.get('confirmPassword');
    }
    get passwordsMismatch(){
        return this.registerForm.errors?.['passwordsMismatch'] &&
                this.confirmPassword?.touched;
    }


}