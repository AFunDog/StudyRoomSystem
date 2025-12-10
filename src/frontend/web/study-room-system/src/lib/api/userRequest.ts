import { http } from '@/lib/utils';

export const userRequest = {
  // 获取登录用户信息
  getUser: () => http.get('/user'),

  // 更新用户信息
  updateUser: (data: {id: string; displayName: string; campusId: string; phone: string; email?: string}) => 
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
