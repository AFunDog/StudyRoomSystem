import { http } from '@/lib/utils';

export const userRequest = {

  // 用户注册(也可以管理员注册)
  register: (data: {
    role: string;
    userName: string;
    password: string;
    displayName?: string | null;
    campusId: string;
    phone: string;
    email?: string;
  }) => http.post('/user/register', data),

  // 获取登录用户信息
  getUser: () => http.get('/user'),

  // 根据用户 ID 获取用户信息
  getUserById: (id: string) => http.get(`/user/${id}`),

  // 更新用户信息
  updateUser: (data: {id: string; displayName: string; campusId: string; phone: string; email?: string; avatar?: string}) => 
    http.put('/user/information', data),

  // 删除用户
  deleteUser: (id: string) => http.delete(`/user/${id}`),

  // 用户更新密码
  updatePassword: (data: { id: string; oldPassword: string; newPassword: string }) => 
    http.put('/user/password', data),

  // 管理员修改用户角色
  updateUserRole: (data: { id: string; role: string }) => 
    http.put('/user/role', data),

  // 管理员获取所有用户（分页）
  getAllUsers: (page: number = 1, pageSize: number = 10) =>
    http.get(`/user/all?page=${page}&pageSize=${pageSize}`),
};
