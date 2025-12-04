import { http } from '@/lib/utils';

export const userRequest = {
  // 获取用户列表（这里需要你后端支持返回所有用户，如果只返回当前用户，可以扩展）
  getUsers: () => http.get('/user'),

  // 更新用户信息
  updateUser: (data: any) => http.put('/user', data),

  // 删除用户
  deleteUser: (id: string) => http.delete(`/user?id=${id}`),

  // 管理员注册
  registerAdmin: (data: any) => http.post('/user/registerAdmin', data),

  // 锁定用户
  blockUser: (id: string) => http.post('/user/block', { id }),
};
