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

export interface UserCreateInput {
    userName: string;
    password: string;
    campusId: string;
    phone: string;
    email?: string;       // 可选字段
    displayName?: string; // 可选字段
}