import { Component, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { UserService } from "../../../core/services/user.service";
import { User } from "../../../shared/models/user.model";
import { CommonModule } from "@angular/common";
import { UserFormComponent } from "../user-form/user-form/user-form.component";
import { Router, RouterLink } from "@angular/router";
import { AuthService } from "../../../core/services/auth.service";
import { Observable } from "rxjs";
import { FormsModule } from "@angular/forms";

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
  imports: [CommonModule, RouterLink, FormsModule],
  changeDetection: ChangeDetectionStrategy.Default
})
export class UserListComponent implements OnInit {
    users: User[] = [];
    allUsers: User[] = [];  
    isLoading: boolean = false;
    errorMessage: string = '';
    isModalOpen: boolean = false;
    selectedUser: User | null = null;
    showOnlyActive = true;

    currentUser$!: Observable<User | null>;

    constructor(
        private userService: UserService,
        private authService: AuthService,
        private router: Router
    ) {}

    ngOnInit(): void {
        this.currentUser$ = this.authService.currentUser$;
        this.loadUsers();
    }

    loadUsers(): void {
        this.isLoading = true;
        this.errorMessage = '';

        this.userService.getAll().subscribe({
            next: (users) => {
                this.allUsers = users;   
                this.users = this.showOnlyActive ? users.filter(u => u.isActive) : users;
                this.isLoading = false;
            },
            error: (err) => {
                this.errorMessage = 'Error loading users.';
                this.isLoading = false;
                console.log('Error', err);
            }
        })
    }

    getTotalUsers(): number {
        return this.allUsers.length;
    }

    getActiveUsers(): number {
        return this.allUsers.filter(u => u.isActive).length;
    }

    getInactiveUsers(): number {
        return this.allUsers.filter(u => !u.isActive).length;
    }

    getAvatarColor(name: string): string {
        const colors = [
            '#667eea', '#764ba2', '#f093fb', '#4facfe',
            '#43e97b', '#fa709a', '#fee140', '#30cfd0'
        ];
        const index = name?.charCodeAt(0) % colors.length || 0;
        return colors[index];
    }

    openAddModal(): void {
        this.isModalOpen = true;
    }

    openEditModal(user: User): void {
        this.selectedUser = user;
        this.isModalOpen = true;
    }

    deactivateUser(id: number): void {
        const confirmed = confirm('Are you sure you want to deactivate this user?');

        if (!confirmed) {
            return;
        }

        this.userService.deactivateUser(id).subscribe({
            next: () => {
                this.loadUsers();
                console.log("User deactivated.");
            },
            error: (err) => {
                this.errorMessage = "Error during deactivate user."
                console.log(err);
            }
        });
    }

    activateUser(id: number): void {
        const confirmed = confirm('Are you sure you want to activate this user?');
        if (!confirmed)
            return;

        this.userService.activateUser(id).subscribe({
            next: () => this.loadUsers(),
            error: (err) => {
                this.errorMessage = 'Error during activate user.';
                console.log(err);
            }
        })
    }

    onModalClosed(): void {
        this.isModalOpen = false;
        this.selectedUser = null;
    }

    onUserSaved(): void {
        this.isModalOpen = false;
        this.selectedUser = null;
        this.loadUsers();
    }

    goBack(): void {
        this.router.navigate(['/dashboard']);
    }
}
