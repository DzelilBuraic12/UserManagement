import { Component, OnInit } from "@angular/core";
import { User } from "../../../shared/models/user.model";
import { ActivatedRoute, Router } from "@angular/router";
import { UserService } from "../../../core/services/user.service";
import { CommonModule } from "@angular/common";

@Component ({
    selector: 'app-user-detail',
    templateUrl: './user-detail.component.html',
    styleUrl: './user-detail.component.scss',
    imports: [CommonModule]
})

export class UserDetailComponent implements OnInit {

    userId: number = 0;
    user: User | null = null;
    isLoading: boolean = true;
    errorMessage: string = '';

    constructor(
        private route: ActivatedRoute,
        private userService: UserService,
        private router: Router
    ){}

    ngOnInit(): void {
        const idParam = this.route.snapshot.paramMap.get('id');

        if(idParam){
            this.userId = parseInt(idParam,10);

            this.userService.getById(this.userId).subscribe({
            next: (user) => {
                this.user = user;
                this.isLoading = false;
            },
            error: (err) => {
                this.errorMessage = 'Error loading user details.';
                this.isLoading = false;
            }
        });
        }else{
            this.errorMessage = 'Invalid user ID.';
            this.isLoading = false;
            this.goBack();
        }
    }

    goBack(): void {
        this.router.navigate(['/users']);
    }
}