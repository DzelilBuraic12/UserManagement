import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { RequestSummary } from "../../shared/models/RequestSummary.model";
import { RecentActivity } from "../../shared/models/recent-activity.model";
import { HighPriorityTicket } from "../../shared/models/high-priority-ticket.model";
import { PriorityBreakdown } from "../../shared/models/priority-breakdown.model";
import { DashboardSummary } from "../../shared/models/dashboard-summary.model";
@Injectable({ 
    providedIn: 'root'
})

export class TicketService {
    private apiUrl = `${environment.apiUrl}/Request`;

    constructor(private http: HttpClient) {}

    getTickets(query: any): Observable<any> {
        return this.http.get(this.apiUrl, { params: query });
    }

    getTicketById(id: number): Observable<any> {
        return this.http.get(`${this.apiUrl}/${id}`);
    }

    createTicket(dto: any): Observable<any> {
        return this.http.post(this.apiUrl,dto);
    }

    updateTicket(id: number, dto: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`,dto);
    }

    assignTechnician(id: number, technicianId: number): Observable<any>{
        return this.http.post(`${this.apiUrl}/${id}/assign-technician`, technicianId);
    }

    changeStatus(id: number, newStatusId: number): Observable<any>{
        return this.http.post(`${this.apiUrl}/${id}/change-status`,newStatusId)
    }

    getSummary(): Observable<RequestSummary>{
        return this.http.get<RequestSummary>(`${this.apiUrl}/summary`);
    }

    getRecentActivity(): Observable<RecentActivity[]> {
        return this.http.get<RecentActivity[]>(`${this.apiUrl}/recent-activity`);
    }

    getHighPriorityTickets(): Observable<HighPriorityTicket[]> {
        return this.http.get<HighPriorityTicket[]>(`${this.apiUrl}/high-priority`);
    }

    getPriorityBreakdown(): Observable<PriorityBreakdown> {
        return this.http.get<PriorityBreakdown>(`${this.apiUrl}/priority-breakdown`);
    }

    getResolvedToday(): Observable<{ resolvedToday: number}>{
        return this.http.get<{ resolvedToday: number }>(`${this.apiUrl}/resolved-today`);
    }

    getDashboardSummary(): Observable<DashboardSummary> {
        return this.http.get<DashboardSummary>(`${this.apiUrl}/summary-with-trends`);
    }

}