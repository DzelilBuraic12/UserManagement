export interface Ticket {
    id: number;
    title: string;
    description?: string;
    statusName: string;
    priority: string;
    createdAt: number;
    dueDate: number;
    technicianId: number;
    technicianName: string;
    createdByName: string;
    createdById: number;

}
