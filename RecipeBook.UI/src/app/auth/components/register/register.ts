import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { RegisterRequestDto } from '../../models/register-request.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Register {
  private fb = inject(FormBuilder);
  protected authService = inject(AuthService);
  private router = inject(Router);
  
  registerSuccess = output<void>();

  registerForm = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(16)]]
  });

  onSubmit() {
    if (this.registerForm.invalid) return;
    
    const formValue = this.registerForm.getRawValue() as RegisterRequestDto;
    
    this.authService.register(formValue).subscribe({
      next: () => {
        this.registerSuccess.emit();
        this.router.navigate(['/recipes']); 
      },
      error: (error) => {
        console.error('Registration failed:', error);
      }
    });
  }
}
