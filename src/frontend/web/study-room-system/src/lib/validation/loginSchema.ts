import { z } from "zod";

export const loginSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    .regex(/^[a-zA-Z0-9._]+$/, "用户名只能包含字母、数字、点或下划线"),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位"),
  agreePolicy: z.preprocess(
    val => val === 'on' || val === true,
    z.boolean().refine(value => value, '请同意隐私政策')
  ),
  autoLogin: z.boolean().optional()
});
