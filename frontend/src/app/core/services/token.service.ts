import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class TokenService {
    private readonly ACCESS_TOKEN_KEY = 'access_token';
    private readonly REFRESH_TOKEN_KEY = 'refresh_token';

    saveTokens(access_token: string, refresh_token:string): void{
        localStorage.setItem(this.ACCESS_TOKEN_KEY, access_token);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, refresh_token);
    }

    getAccessToken(): string | null {
        return localStorage.getItem(this.ACCESS_TOKEN_KEY);
    }

    getRefreshToken(): string | null {
        return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }

    clearTokens() : void{
        localStorage.removeItem(this.ACCESS_TOKEN_KEY);
        localStorage.removeItem(this.REFRESH_TOKEN_KEY); 
    }

    hasToken(): boolean {
        return this.getAccessToken() !== null;
    }
}