import { HttpClient } from "@angular/common/http";
import { TokenService } from "./token.service";
import { Router } from "@angular/router";
import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { BehaviorSubject, Observable, tap } from "rxjs";
import { User } from "../../shared/models/user.model";
import { AuthResponse, LoginRequest, RegisterRequest } from '../../shared/models/auth-response.model';

@Injectable({
    providedIn : 'root'
})

export class AuthService {
    private apiUrl = `${environment.apiUrl}/auth`;
    private currentUserSubject = new BehaviorSubject<User | null>(null);
    public currentUser$ = this.currentUserSubject.asObservable();

    constructor(
        private http: HttpClient,
        private tokenService: TokenService,
        private router: Router
    ){}

    login(request: LoginRequest): Observable<AuthResponse>{
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request)
        .pipe(
            tap(response => {
                console.log("LOGIN RESPONSE", response);

                const user: User = {
                    id: response.id,
                    firstName: response.firstName,
                    lastName: response.lastName,
                    email: response.email,
                    role: response.role,
                    isActive: true
                };

                this.tokenService.saveTokens(response.accessToken, response.refreshToken);
                this.currentUserSubject.next(user);

                if(user.role === 'Admin') {
                    this.router.navigate(['/admin-dashboard']);
                } else {
                    this.router.navigate(['/dashboard'])                                        
                    console.log("GOING TO /users");
                }
            })
        )
    }

    logout(): void{
        this.tokenService.clearTokens();
        this.currentUserSubject.next(null);
        this.router.navigate([`/login`])
    }

    isAuthenticated(): boolean {
        return this.tokenService.hasToken();
    }

    getCurrentUser(): User | null{
        return this.currentUserSubject.value;
    }

    getCurrentUserRole(): string | null {
        const user = this.currentUserSubject.value;
        return user ? user.role : null;
    }

    register(request: RegisterRequest): Observable<any>{
        return this.http.post<any>(`${this.apiUrl}/register`,request)
        
    }

    getCurrentUserId(): number | null {
        let current = this.getCurrentUser();
        if(current != null){
            return current.id
        }
        return null
    }

    isAdmin(): boolean {
        let admin = this.getCurrentUserRole();
        if(admin === 'Admin'){
            return true;
        }
        return false;
    }

    isTechnician(): boolean {
        let technician = this.getCurrentUserRole();
        if(technician === 'Technician'){
            return true;
        }
        return false;
    }

    isUser(): boolean {
        let user = this.getCurrentUserRole();
        if(user === 'User'){
            return true;
        }
        return false;
    }

}