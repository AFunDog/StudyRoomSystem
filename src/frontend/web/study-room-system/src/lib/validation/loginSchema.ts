import { z } from "zod";

export const loginSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    // 支持中文、字母、数字、点和下划线
    .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._]+$/, "用户名只能包含中文、字母、数字、点或下划线"),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位"),
  agreePolicy: z.boolean().refine(val => {
  console.log("agreePolicy 校验值：", val)
  return val
}, '请同意隐私政策'),
  autoLogin: z.boolean().optional()
});

export const adminLoginSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    // 支持中文、字母、数字、点和下划线
    .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._]+$/, "用户名只能包含中文、字母、数字、点或下划线"),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位"),
});