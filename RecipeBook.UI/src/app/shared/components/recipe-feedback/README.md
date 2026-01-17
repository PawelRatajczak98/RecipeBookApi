# Recipe Feedback Component

Komponent Angular 20 do obsługi **lajków** i **komentarzy** dla przepisów kulinarnych.

## Funkcjonalności

- ❤️ **System lajków (likes)** z licznikiem i statusem (polubiony/niepolubiony)
- 💬 **System komentarzy** z możliwością dodawania, przeglądania i usuwania
- 🎨 **Materiał Design** - wykorzystuje Angular Material
- ⚡ **Reaktywny** - używa Angular Signals (Angular 20)
- 📱 **Responsywny** - dostosowuje się do różnych rozdzielczości
- 🎭 **Animacje** - płynne przejścia i efekty wizualne
- ✅ **Walidacja** - kontrola długości komentarzy (max 500 znaków)
- 🔄 **Loading states** - wskaźniki ładowania dla wszystkich operacji
- 🚨 **Obsługa błędów** - przyjazne komunikaty dla użytkownika

## Użycie

### Podstawowe użycie

```typescript
import { RecipeFeedbackComponent } from '@shared/components/recipe-feedback/recipe-feedback.component';

@Component({
  selector: 'app-recipe-detail',
  standalone: true,
  imports: [RecipeFeedbackComponent],
  template: `
    <div class="recipe-card">
      <h2>{{ recipe.name }}</h2>
      <p>{{ recipe.description }}</p>

      <!-- Dodaj komponent feedback -->
      <app-recipe-feedback [recipeId]="recipe.id" />
    </div>
  `
})
export class RecipeDetailComponent {
  recipe = { id: 123, name: 'Spaghetti Carbonara', description: '...' };
}
```

### Zaawansowane użycie

```typescript
<!-- Bez przycisku komentarzy -->
<app-recipe-feedback
  [recipeId]="recipeId"
  [showCommentsButton]="false" />

<!-- Z małymi ikonami -->
<app-recipe-feedback
  [recipeId]="recipeId"
  iconSize="small" />

<!-- Z dużymi ikonami -->
<app-recipe-feedback
  [recipeId]="recipeId"
  iconSize="large" />
```

## Właściwości @Input

| Właściwość | Typ | Wymagane | Domyślna | Opis |
|------------|-----|----------|----------|------|
| `recipeId` | `number` | ✅ Tak | - | ID przepisu do obsługi |
| `showCommentsButton` | `boolean` | ❌ Nie | `true` | Czy pokazywać przycisk komentarzy |
| `iconSize` | `'small' \| 'medium' \| 'large'` | ❌ Nie | `'medium'` | Rozmiar ikon (18px, 22px, 28px) |

## API Endpoints

Komponent komunikuje się z następującymi endpointami:

### Lajki (Likes)

- **GET** `/api/likes/{recipeId}` - Pobiera podsumowanie lajków
  - Response: `{ totalLikes: number, likedByCurrentUser: boolean }`

- **POST** `/api/likes/{recipeId}` - Dodaje lajk
  - Response: `string` (komunikat)

- **DELETE** `/api/likes/{recipeId}` - Usuwa lajk
  - Response: `void`

### Komentarze (Comments)

- **GET** `/api/comments?recipeId={id}` - Pobiera komentarze
  - Response: `CommentDto[]`

- **POST** `/api/comments` - Dodaje komentarz
  - Body: `{ recipeId: number, commentContent: string }`
  - Response: `CommentDto`

- **DELETE** `/api/comments/{recipeId}` - Usuwa komentarz użytkownika
  - Response: `void`

## Modele danych

### CommentDto
```typescript
interface CommentDto {
  content: string;
  userName: string;
  createdAt: Date;
}
```

### CommentCreateDto
```typescript
interface CommentCreateDto {
  recipeId: number;
  commentContent: string;
}
```

### LikeSummaryDto
```typescript
interface LikeSummaryDto {
  totalLikes: number;
  likedByCurrentUser: boolean;
}
```

## Wymagane zależności

Upewnij się, że masz zainstalowane następujące pakiety Angular Material:

```bash
npm install @angular/material @angular/cdk
```

### Wymagane moduły Material:

- `MatIconModule`
- `MatButtonModule`
- `MatTooltipModule`
- `MatDialogModule`
- `MatSnackBarModule`
- `MatFormFieldModule`
- `MatInputModule`
- `MatCardModule`
- `MatProgressSpinnerModule`

## Konfiguracja

### 1. URL API

URL API jest ustawiony na `https://localhost:7090/api` bezpośrednio w serwisie. Jeśli używasz innego adresu, zmień wartość w [recipe-feedback.service.ts:11](../../../services/recipe-feedback.service.ts#L11):

```typescript
private readonly apiUrl = 'https://localhost:7090/api'; // Zmień na swój URL
```

### 2. HTTP Interceptor

Komponent wymaga autoryzacji (JWT token). Upewnij się, że masz skonfigurowany interceptor dla tokenów.

### 3. Material Theme

Dodaj do `styles.css`:

```css
@import '@angular/material/prebuilt-themes/indigo-pink.css';

/* Opcjonalne - niestandardowe kolory snackbar */
.success-snackbar {
  background-color: #4caf50 !important;
  color: white !important;
}

.error-snackbar {
  background-color: #f44336 !important;
  color: white !important;
}
```

## Struktura plików

```
shared/
├── components/
│   └── recipe-feedback/
│       ├── recipe-feedback.component.ts      # Logika komponentu
│       ├── recipe-feedback.component.html    # Template
│       ├── recipe-feedback.component.css     # Style
│       └── README.md                         # Dokumentacja
├── models/
│   ├── comment.model.ts                      # Modele komentarzy
│   └── like.model.ts                         # Modele lajków
└── services/
    └── recipe-feedback.service.ts            # Serwis API
```

## Przykład integracji w module przepisu

```typescript
// recipe-card.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { RecipeFeedbackComponent } from '@shared/components/recipe-feedback/recipe-feedback.component';

@Component({
  selector: 'app-recipe-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    RecipeFeedbackComponent
  ],
  template: `
    <mat-card class="recipe-card">
      <mat-card-header>
        <mat-card-title>{{ recipe.name }}</mat-card-title>
      </mat-card-header>

      <img mat-card-image [src]="recipe.imageUrl" [alt]="recipe.name">

      <mat-card-content>
        <p>{{ recipe.description }}</p>
      </mat-card-content>

      <mat-card-actions>
        <!-- Dodaj feedback na końcu karty -->
        <app-recipe-feedback
          [recipeId]="recipe.id"
          iconSize="medium" />
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .recipe-card {
      max-width: 400px;
      margin: 16px;
    }

    mat-card-actions {
      padding: 16px;
      border-top: 1px solid #e0e0e0;
    }
  `]
})
export class RecipeCardComponent {
  @Input() recipe!: any;
}
```

## Dostosowanie

### Kolory

Możesz dostosować kolory edytując plik CSS:

```css
/* Kolor lajka */
.like-button.liked {
  color: #e91e63; /* Zmień na swój kolor */
}

/* Kolor przycisku komentarza */
.comment-button.active {
  color: #2196f3; /* Zmień na swój kolor */
}
```

### Animacje

Możesz dostosować animacje w pliku CSS:

```css
/* Animacja bicia serca */
@keyframes heartBeat {
  0%, 100% { transform: scale(1); }
  25% { transform: scale(1.3); }
  50% { transform: scale(1.1); }
  75% { transform: scale(1.2); }
}

/* Animacja rozwijania komentarzy */
@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

## Rozwiązywanie problemów

### Problem: "Cannot find module '@angular/material'"
**Rozwiązanie:** Zainstaluj Angular Material:
```bash
ng add @angular/material
```

### Problem: "No provider for RecipeFeedbackService"
**Rozwiązanie:** Komponent jest standalone i automatycznie dostarcza serwis. Upewnij się, że importujesz komponent w miejscu użycia.

### Problem: "401 Unauthorized"
**Rozwiązanie:** Upewnij się, że użytkownik jest zalogowany i token JWT jest wysyłany w nagłówkach żądań.

### Problem: Komentarze nie ładują się
**Rozwiązanie:** Sprawdź:
1. Czy URL API jest poprawny w `recipe-feedback.service.ts` (linia 11)
2. Czy backend działa na podanym adresie (`https://localhost:7090`)
3. Czy endpoint `/api/comments` zwraca poprawne dane
4. Konsola przeglądarki (F12) - sprawdź błędy CORS lub 404

## Wsparcie przeglądarek

- Chrome (ostatnie 2 wersje) ✅
- Firefox (ostatnie 2 wersje) ✅
- Safari (ostatnie 2 wersje) ✅
- Edge (ostatnie 2 wersje) ✅

## Licencja

Ten komponent jest częścią projektu RecipeBook.
