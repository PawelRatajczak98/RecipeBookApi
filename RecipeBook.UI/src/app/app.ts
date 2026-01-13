import { Component, inject, signal, viewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router'; 
import { CommonModule } from '@angular/common';
import { UserInfoService } from './user/services/user-info.service';
import { AuthService } from './auth/services/auth.service';
import { AuthModal } from './shared/components/auth-modal/auth-modal';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterLink, RouterOutlet, RouterLinkActive, AuthModal],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('RecipeBook.UI');
  protected userInfo = inject(UserInfoService);
  private authService = inject(AuthService);
  private router = inject(Router); // <-- dodajemy router
  protected readonly routerLinkOptions = signal({ exact: true });
  protected authModal = viewChild(AuthModal);

  showLogin() {
    this.authModal()?.showModal('login');
  }

  showRegister() {
    this.authModal()?.showModal('register');
  }

  onAuthSuccess() {
  }

  logout() {
    this.authService.logout();
    this.userInfo.clear();
    this.router.navigate(['/recipes']);
  }
}
