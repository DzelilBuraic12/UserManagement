import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { Observable } from "rxjs";
import { User } from "../../shared/models/user.model";
import { Technician } from "../../shared/models/technician.model";

@Injectable ({
    providedIn:'root'
})

export class UserService {
    private apiUrl = environment.apiUrl + '/users';

    constructor(private http: HttpClient){}

    getAll(): Observable<User[]> {
       return this.http.get<User[]>(`${this.apiUrl}/all`);
    }

    getById(id: number): Observable<User>{
        var url = this.apiUrl + "/" + id;
        return this.http.get<User>(url);
    }

    createUser(user: any): Observable<User>{
        return this.http.post<User>(this.apiUrl, user)
    }

    updateUser(id:number,user:any): Observable<User>{
        var url = this.apiUrl + "/" + id;
        return this.http.put<User>(url,user)
    }

    deactivateUser(id: number){
        return this.http.post(`${this.apiUrl}/${id}/deactivate`, null);
    }

    activateUser(id: number){
        return this.http.post(`${this.apiUrl}/${id}/activate`, null);
    }

    getTechnicians(): Observable<Technician[]> {
        return this.http.get<Technician[]>(`${this.apiUrl}/technicians`)
    }    

}