export interface TicketQuery {
    page: number;
    pageSize: number;
    createdById?: number;
    myAssignedOnly?: boolean | string;
    statusId?: number;
    technicianId?: number;
    search?: string;
    includeClosed: boolean;
}