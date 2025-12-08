import { z } from "zod";

// 普通用户注册用 schema
export const registerSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    .regex(/^[a-zA-Z0-9._]+$/, "用户名只能包含字母、数字、点或下划线"),
  campusId: z.string({ required_error: '请输入学号/工号' }),
  phone: z.string({ required_error: '请输入手机号' })
    .regex(/^1[3-9]\d{9}$/, '请输入有效的手机号'),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位")
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
  confirmPassword: z.string({ required_error: '请输入确认密码' }),
  agreePolicy: z.preprocess(val => val === 'on' || val === true,
    z.boolean().refine(value => value, '请同意隐私政策')),
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: '确认密码与密码不一致'
});

// 管理员添加用户用 schema
export const adminAddUserSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    .regex(/^[a-zA-Z0-9._]+$/, "用户名只能包含字母、数字、点或下划线"),
  campusId: z.string({ required_error: '请输入学号/工号' }),
  phone: z.string({ required_error: '请输入手机号' })
    .regex(/^1[3-9]\d{9}$/, '请输入有效的手机号'),
  email: z.string().email('请输入有效的邮箱').optional(), // 管理员可选填邮箱
  displayName: z.string().optional(), // 管理员可选填昵称
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位")
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
  confirmPassword: z.string({ required_error: '请输入确认密码' }),
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: '确认密码与密码不一致'
});
