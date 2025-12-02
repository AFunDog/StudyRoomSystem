<script setup lang="ts">
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form'
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { toast } from 'vue-sonner';

import { toTypedSchema } from '@vee-validate/zod';
import z from 'zod';
import { useField, useForm, type GenericObject } from 'vee-validate';
import { http } from '@/lib/utils';
import { onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import type { User } from '@/lib/types/user';
import { LockKeyhole, Eye, EyeOff } from 'lucide-vue-next';
import { email } from 'zod/v4';
import { AxiosError } from 'axios';
import { authRequest } from '@/lib/api/authRequest';
import { Checkbox } from '@/components/ui/checkbox';

const router = useRouter();
const schema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    .regex(/^[a-zA-Z0-9._]+$/, "用户名只能包含字母、数字、点或下划线"),
  campusId: z.string({ required_error: '请输入学号/工号' }),
  // displayName: z.string().min(4,'请输入昵称').nullable(),
  phone: z.string({ required_error: '请输入手机号' }).regex(/^1[3-9]\d{9}$/, '请输入有效的手机号'),
  // email : z.string({ }).email('请输入有效的邮箱').nullable(),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位")
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
  confirmPassword: z.string({ required_error: '请输入确认密码' }),
  agreePolicy: z.preprocess(val => val === 'on' || val === true, z.boolean().refine(value => value, '请同意隐私政策')),
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: '确认密码与密码不一致'
});
const formSchema = toTypedSchema(schema)
const form = useForm({ validationSchema: formSchema });

// const isShowPassword = ref(false);
const isRegisterLoading = ref(false); //注册按钮状态

//注册提交逻辑
const onSubmit = form.handleSubmit(async (values) => {
  try {
    console.log(values);
    isRegisterLoading.value = true; //开始注册，设置按钮为加载状态
    var res = await authRequest.register({
      userName: values.userName,
      password: values.password,
      campusId: values.campusId,
      phone: values.phone
    })

    // const user = res.data.user as User;

    //统一由后端返回错误信息
    // if (!user) {
    //   toast.error('注册失败');
    //   return;
    // }

    //注册成功信息
    toast.success('注册成功，请登录')
    router.push('/login');

  }
  catch (error) {
    if (error instanceof AxiosError)
      toast.error('注册失败', {
        description: error.response?.data.message,
      });
    else
      toast.error('注册失败');
    console.log(error);
  }
  finally {
    isRegisterLoading.value = false; //注册完成，恢复按钮状态
  }
});

//返回逻辑
function handleBack() {
  if (window.history.length <= 1) {
    // 没有历史记录，说明是直接打开注册页
    router.push('/login');
  } else {
    router.back();
  }
}

</script>
<template>
  <div class="flex flex-col items-center justify-center h-full">
    <div class="w-5/6 max-w-120">
      <Card>
        <CardHeader>
          <div class="text-2xl tracking-widest">注册</div>
          <div class="text-sm text-muted-foreground">欢迎来到智慧自习室预约管理系统</div>
        </CardHeader>
        <CardContent>
          <form class="flex flex-col gap-y-4" :validation-schema="formSchema" @submit="onSubmit">
            <FormField v-slot="{ componentField }" name="userName">
              <FormItem>
                <FormLabel>用户名</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" autocomplete="username" placeholder="请输入用户名" />
                </FormControl>
                <!-- <FormDescription>用户注册时的名称</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <FormField v-slot="{ componentField }" name="campusId">
              <FormItem>
                <FormLabel>学号/工号</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" placeholder="请输入学号/工号" />
                </FormControl>
                <!-- <FormDescription>用户注册时的名称</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <!-- <FormField v-slot="{ componentField }" name="displayName">
              <FormItem>
                <FormLabel>昵称</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" autocomplete="nickname" :placeholder="displayNamePlaceholder" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField> -->
            <FormField v-slot="{ componentField }" name="phone">
              <FormItem>
                <FormLabel>手机号</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" autocomplete="cc-number" placeholder="请输入手机号" />
                </FormControl>
                <!-- <FormDescription>用户注册时的名称</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <!-- <FormField v-slot="{ componentField }" name="email">
              <FormItem>
                <FormLabel>邮箱</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" placeholder="请输入邮箱" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField> -->
            <FormField v-slot="{ componentField }" name="password">
              <FormItem>
                <FormLabel>密码</FormLabel>
                <FormControl>
                  <div class="flex flex-row items-center relative">
                    <div class="absolute flex flex-row justify-center items-center w-10">
                      <LockKeyhole class="size-4"></LockKeyhole>
                    </div>
                    <Input v-bind="componentField" type="password" autocomplete="new-password" placeholder="请输入密码"
                      class="pl-10">
                    </Input>

                  </div>
                </FormControl>
                <!-- <FormDescription>登录密码</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <FormField v-slot="{ componentField }" name="confirmPassword">
              <FormItem>
                <FormLabel>确认密码</FormLabel>
                <FormControl>
                  <div class="flex flex-row items-center relative">
                    <div class="absolute flex flex-row justify-center items-center w-10">
                      <LockKeyhole class="size-4"></LockKeyhole>
                    </div>
                    <Input v-bind="componentField" type="password" autocomplete="new-password" placeholder="请输入确认密码"
                      class="pl-10">
                    </Input>

                    <!-- <div class="absolute flex flex-row justify-center items-center w-10 right-0">
                      <component :is="isShowPassword ? EyeOff : Eye" @click="isShowPassword = !isShowPassword"
                        class="size-4"></component>
                    </div> -->
                  </div>
                </FormControl>
                <!-- <FormDescription>登录密码</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <FormField v-slot="{ componentField }" name="agreePolicy">
              <FormItem>
                <FormControl>
                  <div>
                    <div class="flex flex-row gap-x-2 items-center">
                      <Checkbox v-bind="componentField">
                      </Checkbox>
                      <div class="text-sm [&>a]:text-primary">
                        我已阅读并同意
                        <a href="">《隐私政策》</a>
                        和
                        <a href="">《用户协议》</a>
                      </div>
                    </div>
                  </div>
                </FormControl>
                <!-- <FormDescription>登录密码</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <!-- <div>{{ registerMessage }}</div> -->
            <div class="flex flex-row justify-center items-center gap-x-4">
              <Button class="hover:cursor-pointer" type="button" variant="secondary" @click="handleBack()">返回</Button>
              <Button class="hover:cursor-pointer" type="submit" :disabled="isRegisterLoading">
                <Loader2 v-if="isRegisterLoading" class="size-4 animate-spin" />
                注册
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  </div>
</template>