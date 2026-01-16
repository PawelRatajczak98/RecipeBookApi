import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { UserLoginDtoModel } from '../../../user/models/user-login-dto.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule,CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Login {
  private fb = inject(FormBuilder);
  protected authService = inject(AuthService);
  private router = inject(Router);
  
  loginSuccess = output<void>();

  loginForm = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.authService.loginAndLoad(this.loginForm.getRawValue()).subscribe({
      next: () => {
        this.loginSuccess.emit();
        this.router.navigate(['/recipes']);
      },
      error: (err) => {
        console.error('Błąd logowania:', err);
      }
    });
  }

  onLogout() {
    this.authService.logout();
    this.loginForm.reset();
  }
}

