import { Component, Input, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RecipeFeedbackService } from '../../services/recipe-feedback.service';
import { CommentDto, CommentCreateDto } from '../../models/comment.model';
import { LikeSummaryDto } from '../../models/like.model';

@Component({
  selector: 'app-recipe-feedback',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './recipe-feedback.component.html',
  styleUrl: './recipe-feedback.component.css'
})
export class RecipeFeedbackComponent implements OnInit {
  @Input({ required: true }) recipeId!: number;
  @Input() showCommentsButton: boolean = true;
  @Input() iconSize: 'small' | 'medium' | 'large' = 'medium';

  // Signals for reactive state management (Angular 20 best practice)
  likeSummary = signal<LikeSummaryDto>({ totalLikes: 0, likedByCurrentUser: false });
  comments = signal<CommentDto[]>([]);
  commentsCount = computed(() => this.comments().length);
  isLoadingLikes = signal(false);
  isLoadingComments = signal(false);
  showCommentsSection = signal(false);
  newCommentContent = signal('');
  isSubmittingComment = signal(false);

  constructor(
    private feedbackService: RecipeFeedbackService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadLikesSummary();
  }

  /**
   * Ładuje podsumowanie lajków dla przepisu
   */
  loadLikesSummary(): void {
    this.isLoadingLikes.set(true);
    this.feedbackService.getLikesSummary(this.recipeId).subscribe({
      next: (summary) => {
        this.likeSummary.set(summary);
        this.isLoadingLikes.set(false);
      },
      error: (err) => {
        console.error('Błąd podczas ładowania lajków:', err);
        this.isLoadingLikes.set(false);
        this.showError('Nie udało się załadować informacji o lajkach');
      }
    });
  }

  /**
   * Toggle lajka - dodaje lub usuwa w zależności od stanu
   */
  toggleLike(): void {
    if (this.isLoadingLikes()) return;

    const currentState = this.likeSummary();
    this.isLoadingLikes.set(true);

    if (currentState.likedByCurrentUser) {
      // Usuń lajk
      this.feedbackService.deleteLike(this.recipeId).subscribe({
        next: () => {
          this.likeSummary.set({
            totalLikes: currentState.totalLikes - 1,
            likedByCurrentUser: false
          });
          this.isLoadingLikes.set(false);
          this.showSuccess('Usunięto polubienie');
        },
        error: (err) => {
          console.error('Błąd podczas usuwania lajka:', err);
          this.isLoadingLikes.set(false);
          this.showError(err.error?.message || 'Nie udało się usunąć polubienia');
        }
      });
    } else {
      // Dodaj lajk
      this.feedbackService.addLike(this.recipeId).subscribe({
        next: () => {
          this.likeSummary.set({
            totalLikes: currentState.totalLikes + 1,
            likedByCurrentUser: true
          });
          this.isLoadingLikes.set(false);
          this.showSuccess('Polubiono przepis');
        },
        error: (err) => {
          console.error('Błąd podczas dodawania lajka:', err);
          this.isLoadingLikes.set(false);
          this.showError(err.error?.message || 'Nie udało się polubić przepisu');
        }
      });
    }
  }

  /**
   * Ładuje komentarze dla przepisu
   */
  loadComments(): void {
    this.isLoadingComments.set(true);
    this.feedbackService.getComments(this.recipeId).subscribe({
      next: (comments) => {
        this.comments.set(comments);
        this.isLoadingComments.set(false);
      },
      error: (err) => {
        console.error('Błąd podczas ładowania komentarzy:', err);
        this.isLoadingComments.set(false);
        this.showError('Nie udało się załadować komentarzy');
      }
    });
  }

  /**
   * Przełącza widoczność sekcji komentarzy
   */
  toggleCommentsSection(): void {
    const newState = !this.showCommentsSection();
    this.showCommentsSection.set(newState);

    if (newState && this.comments().length === 0) {
      this.loadComments();
    }
  }

  /**
   * Dodaje nowy komentarz
   */
  addComment(): void {
    const content = this.newCommentContent().trim();

    if (!content) {
      this.showError('Komentarz nie może być pusty');
      return;
    }

    if (content.length > 500) {
      this.showError('Komentarz nie może przekraczać 500 znaków');
      return;
    }

    this.isSubmittingComment.set(true);

    const dto: CommentCreateDto = {
      recipeId: this.recipeId,
      commentContent: content
    };

    this.feedbackService.addComment(dto).subscribe({
      next: (newComment) => {
        // Dodaj nowy komentarz na początek listy
        this.comments.update(comments => [newComment, ...comments]);
        this.newCommentContent.set('');
        this.isSubmittingComment.set(false);
        this.showSuccess('Komentarz został dodany');
      },
      error: (err) => {
        console.error('Błąd podczas dodawania komentarza:', err);
        this.isSubmittingComment.set(false);
        this.showError(err.error?.message || 'Nie udało się dodać komentarza');
      }
    });
  }

  /**
   * Usuwa komentarz użytkownika
   */
  deleteComment(): void {
    if (!confirm('Czy na pewno chcesz usunąć swój komentarz?')) {
      return;
    }

    this.feedbackService.deleteComment(this.recipeId).subscribe({
      next: () => {
        // Przeładuj komentarze po usunięciu
        this.loadComments();
        this.showSuccess('Komentarz został usunięty');
      },
      error: (err) => {
        console.error('Błąd podczas usuwania komentarza:', err);
        this.showError(err.error?.message || 'Nie udało się usunąć komentarza');
      }
    });
  }

  /**
   * Formatuje datę do czytelnego formatu
   */
  formatDate(date: Date): string {
    const d = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Teraz';
    if (diffMins < 60) return `${diffMins} min temu`;
    if (diffHours < 24) return `${diffHours} godz. temu`;
    if (diffDays < 7) return `${diffDays} dni temu`;

    return d.toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  /**
   * Zwraca rozmiar ikony w pikselach
   */
  getIconSize(): number {
    switch (this.iconSize) {
      case 'small': return 18;
      case 'large': return 28;
      default: return 22;
    }
  }

  /**
   * Pokazuje wiadomość o sukcesie
   */
  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Zamknij', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  /**
   * Pokazuje wiadomość o błędzie
   */
  private showError(message: string): void {
    this.snackBar.open(message, 'Zamknij', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
