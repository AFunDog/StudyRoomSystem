class AuthService {
    public async login(username: string, password: string) {

    }
    public async logout() {

    }
    public async register(userName: string, password: string, campusId: string, phone: string, email?: string, displayName?: string | null) {

    }
    public async registerAdmin(
        userName: string, password: string, campusId: string, phone: string, email?: string, displayName?: string | null
    ) {
        
    }
}


const authService = new AuthService();

export { authService };