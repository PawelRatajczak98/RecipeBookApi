export interface CommentDto {
  content: string;
  userName: string;
  createdAt: Date;
}

export interface CommentCreateDto {
  recipeId: number;
  commentContent: string;
}
