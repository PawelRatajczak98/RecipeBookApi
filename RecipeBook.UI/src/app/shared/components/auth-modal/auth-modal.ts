import { Component, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Login } from '../../../auth/components/login/login';
import { Register } from '../../../auth/components/register/register';

@Component({
  selector: 'app-auth-modal',
  standalone: true,
  imports: [CommonModule, Login, Register],
  template: `
    @if (isVisible()) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <button class="close-btn" (click)="closeModal()">&times;</button>
          
          <div class="modal-tabs">
            <button 
              [class.active]="currentTab() === 'login'"
              (click)="currentTab.set('login')">
              Zaloguj
            </button>
            <button 
              [class.active]="currentTab() === 'register'"
              (click)="currentTab.set('register')">
              Zarejestruj
            </button>
          </div>

          <div class="modal-body">
            @if (currentTab() === 'login') {
              <app-login (loginSuccess)="onAuthSuccess()"></app-login>
            } @else {
              <app-register (registerSuccess)="onRegisterSuccess()"></app-register>
            }
          </div>
        </div>
      </div>
    }
  `,
  styleUrl: './auth-modal.css'
})
export class AuthModal {
  isVisible = signal(false);
  currentTab = signal<'login' | 'register'>('login');
  
  authSuccess = output<void>();

  showModal(tab: 'login' | 'register' = 'login') {
    this.currentTab.set(tab);
    this.isVisible.set(true);
  }

  closeModal() {
    this.isVisible.set(false);
  }

  onAuthSuccess() {
    this.authSuccess.emit();
    this.closeModal();
  }

  onRegisterSuccess() {
    this.currentTab.set('login');
  }
}