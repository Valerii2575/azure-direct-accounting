import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.css']
})
export class AuthCallbackComponent implements OnInit{
  loading = true;
  error: string | null = null;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const code = params['code'];
      const state = params['state'];
      const error = params['error'];

      if(error){
        this.error = `Authentication failed: ${error}`;
        this.loading = false;
        return;
      }

      if(code && state){
        this.handleAuthCallback(code, state);
      } else {
        this.error = 'Missing authentication parameters';
        this.loading = false;
      }
    });
  }

    handleAuthCallback(code: string, state: string) : void {
      this.authService.handleCallback(code, state).subscribe({
        next: (authResponse) => {
          this.authService.procassAuthResponse(authResponse);
          this.loading = false;
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.error = error.error?.message || 'Authentication failed';
          this.loading = false;
        }
      });
    }

    goHome(): void {
      this.router.navigate(['/']);
    }
}
