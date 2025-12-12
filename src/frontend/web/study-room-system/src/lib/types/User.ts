export interface User {
    id: string;
    createTime: string;
    userName: string;
    displayName: string;
    password: string;
    campusId: string;
    phone: string;
    email: string;
    role: string;
    avatar : string;
}

export interface UserEditInput {
    id: string;
    displayName: string;
    campusId: string;
    phone: string;
    email?: string;
    role: "User" | "Admin";
}