import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { UserListComponent } from './features/users/user-list/user-list.component';
import { UserDetailComponent } from './features/users/user-detail/user-detail.component';
import { TicketDetailComponent } from './features/tickets/ticket-detail/ticket-detail.component';
import { TicketListComponent } from './features/tickets/ticket-list/ticket-list.component';
import { TicketFormComponent } from './features/tickets/ticket-form/ticket-form.component';
import { UserFormComponent } from './features/users/user-form/user-form/user-form.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { HomeComponent } from './features/home/home.component';

export const routes: Routes = [
    { path: '', redirectTo:'/home' , pathMatch: 'full'},
    { path: 'home', component: HomeComponent},
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent},
    { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard]},
    { path: 'users', component: UserListComponent },
    { path: 'users/create', component: UserFormComponent },
    { path: 'users/:id/edit', component: UserFormComponent },
    { path: 'users/:id', component: UserDetailComponent, canActivate: [authGuard]},
    { path: 'tickets', component: TicketListComponent, canActivate: [authGuard]},
    { path: 'tickets/create', component: TicketFormComponent, canActivate: [authGuard]},
    { path: 'tickets/edit/:id', component: TicketFormComponent, canActivate: [authGuard]},
    { path: 'tickets/:id', component: TicketDetailComponent},

];
