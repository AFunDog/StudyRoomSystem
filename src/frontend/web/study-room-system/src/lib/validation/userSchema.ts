import { z } from "zod";

// 普通用户注册用 schema
export const registerSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    // 支持中文、字母、数字、点和下划线
    .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._]+$/, "用户名只能包含中文、字母、数字、点或下划线"),
  campusId: z.string({ required_error: '请输入学号/工号' })
    .min(4, "学号/工号至少 4 位")
    .max(20, "学号/工号不能超过 20 位"),
  phone: z.string({ required_error: '请输入手机号' })
    .regex(/^1[3-9]\d{9}$/, '请输入有效的手机号'),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位")
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
  confirmPassword: z.string({ required_error: '请输入确认密码' }),
  agreePolicy: z.boolean({required_error : '请同意隐私政策'}).refine(val => val, '请同意隐私政策'),
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: '确认密码与密码不一致'
});

// 管理员添加用户用 schema
export const adminAddUserSchema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    // 支持中文、字母、数字、点和下划线
    .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._]+$/, "用户名只能包含中文、字母、数字、点或下划线"),
  campusId: z.string({ required_error: '请输入学号/工号' })
    .min(4, "学号/工号至少 4 位")
    .max(20, "学号/工号不能超过 20 位"),
  phone: z.string({ required_error: '请输入手机号' })
    .regex(/^1[3-9]\d{9}$/, '请输入有效的手机号'),
  // 管理员可选填邮箱
  email: z.preprocess(
    (val) => {
      if (typeof val !== "string") return val
      const trimmed = val.trim()
      return trimmed === "" ? undefined : trimmed
    },
    z.string().email("请输入有效的邮箱").optional()
  ),
  // 管理员可选填昵称
  displayName: z.preprocess(
    (val) => {
      if (typeof val !== "string") return val
      const trimmed = val.trim()
      return trimmed === "" ? undefined : trimmed
    },
    z.string()
      .max(20, "用户名不能超过 20 位")
      .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._-]+$/, "昵称只能包含中文、字母、数字、点、下划线或中横线")
      .optional()
  ),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位")
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
  confirmPassword: z.string({ required_error: '请输入确认密码' }),
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: '确认密码与密码不一致'
});

// 管理员编辑用户用 schema
export const editUserSchema = z.object({
  id: z.string(), // 简单规则，只要求是字符串
  // 管理员可选填昵称
  displayName: z.preprocess(
    (val) => {
      if (typeof val !== "string") return val
      const trimmed = val.trim()
      return trimmed === "" ? undefined : trimmed
    },
    z.string()
      .max(20, "用户名不能超过 20 位")
      .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._-]+$/, "昵称只能包含中文、字母、数字、点、下划线或中横线")
      .optional()
  ),
  campusId: z.string({ required_error: "请输入学号/工号" })
    .min(4, "学号/工号至少 4 位")
    .max(20, "学号/工号不能超过 20 位"),
  phone: z.string({ required_error: "请输入手机号" })
    .regex(/^1[3-9]\d{9}$/, "请输入有效的手机号"),
  // 管理员可选填邮箱
  email: z.preprocess(
    (val) => {
      if (typeof val !== "string") return val
      const trimmed = val.trim()
      return trimmed === "" ? undefined : trimmed
    },
    z.string().email("请输入有效的邮箱").optional()
  ),
  role: z.enum(["User", "Admin"], { required_error: "请选择角色" }),
});


// 用户中心修改基本信息用 schema
export const userCenterProfileSchema = z.object({
  displayName: z
    .string({ required_error: "请输入昵称" })
    .max(20, "昵称不能超过 20 位")
    .regex(/^[\u4e00-\u9fa5a-zA-Z0-9._-]+$/, "昵称只能包含中文、字母、数字、点、下划线或中横线"),
  campusId: z
    .string({ required_error: "请输入学号/工号" })
    .min(4, "学号/工号至少 4 位")
    .max(20, "学号/工号不能超过 20 位"),
  phone: z
    .string({ required_error: "请输入手机号" })
    .regex(/^1[3-9]\d{9}$/, "请输入有效的手机号"),
  // 邮箱可选
  email: z.preprocess(
    (val) => {
      if (typeof val !== "string") return val
      const trimmed = val.trim()
      return trimmed === "" ? undefined : trimmed
    },
    z.string().email("请输入有效的邮箱").optional()
  ),
});

// 用户中心修改密码用 schema
export const userCenterPasswordSchema = z
  .object({
    oldPassword: z
      .string({ required_error: "请输入原密码" }),
    newPassword: z
      .string({ required_error: "请输入新密码" })
      .min(8, "密码至少 8 位")
      .max(32, "密码不能超过 32 位")
      .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
    confirmPassword: z.string({ required_error: "请输入确认密码" }),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "确认密码与新密码不一致",
});