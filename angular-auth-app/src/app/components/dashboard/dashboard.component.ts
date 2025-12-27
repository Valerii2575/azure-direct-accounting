import { Component, inject, OnInit } from '@angular/core';
import { UserInfo } from 'src/app/models/user/userInfo';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  private authService = inject(AuthService);

  user: UserInfo | null = null;

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {this.user = user;});
  }

  logout(){
    this.authService.logout();
  }

  refreshToken(): void {
    this.authService.refreshToken().subscribe({
      next: (authResponse) => {
        this.authService.procassAuthResponse(authResponse);
        alert('Token refreshed successfully!');
      },
      error: (error) => {
        alert('Token refresh failed: ' + error.message);
      }
    })
  }
}
