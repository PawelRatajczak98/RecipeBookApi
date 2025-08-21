import { Injectable, signal, WritableSignal } from "@angular/core";
import { UserDtoModel } from "../models/user-dto.model";

@Injectable({providedIn : 'root'})
export class UserInfoService{
    
    currentUser: WritableSignal<UserDtoModel | null> = signal(null);

    constructor() {}

    public setUser(user : UserDtoModel): void {
        this.currentUser.set(user);
    }

    public clearUser(): void {
        this.currentUser.set(null);
    }
}